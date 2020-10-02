// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Amg88xx;
using UnitsNet;
using Xunit;

namespace Iot.Device.Amg88xx.Tests
{
    public class Amg88xxTests
    {
        [Fact]
        public void GetSensorTemperatureTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // target temperature is 25°C which is equivalent to 0x190
            i2cDevice.DataToRead.Enqueue(0x90);
            i2cDevice.DataToRead.Enqueue(0x01);

            Assert.Equal(25, sensor.GetSensorTemperature().DegreesCelsius);
            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.TTHL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.TTHH, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void GetRawImageTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // using a simple linear sequence of numbers as reference image:
            //   0 to 0x1f8 (504d)
            int[,] referenceImage = new int[Amg88xx.Columns, Amg88xx.Rows];
            for (int y = 0; y < Amg88xx.Rows; y++)
            {
                for (int x = 0; x < Amg88xx.Columns; x++)
                {
                    int rawValue = (y * Amg88xx.Columns + x) * 8;
                    referenceImage[x, y] = rawValue;
                    i2cDevice.DataToRead.Enqueue((byte)(rawValue & 0xff));
                    i2cDevice.DataToRead.Enqueue((byte)(rawValue >> 8));
                }
            }

            // read image from sensor
            var rawImage = sensor.GetRawImage();

            // expectation: one write access to register T01L (lower byte of first pixel) to trigger readout
            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.T01L, i2cDevice.DataWritten.Dequeue());

            // expectation: all pixels have been read, so nothing is remaining
            Assert.Empty(i2cDevice.DataToRead);

            for (int y = 0; y < Amg88xx.Rows; y++)
            {
                for (int x = 0; x < Amg88xx.Columns; x++)
                {
                    Assert.Equal(referenceImage[x, y], rawImage[x, y]);
                }
            }
        }

        [Fact]
        public void GetThermalImageTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // using a simple linear sequence of numbers as reference image:
            //   0 to 126°C (0x1f8/504d)
            Temperature[,] referenceImage = new Temperature[Amg88xx.Columns, Amg88xx.Rows];
            for (int y = 0; y < Amg88xx.Rows; y++)
            {
                for (int x = 0; x < Amg88xx.Columns; x++)
                {
                    int rawValue = (y * Amg88xx.Columns + x) * 8;
                    byte tl = (byte)(rawValue & 0xff);
                    byte th = (byte)(rawValue >> 8);
                    referenceImage[x, y] = Amg88xxUtils.ConvertPixelReading(tl, th);
                    i2cDevice.DataToRead.Enqueue(tl);
                    i2cDevice.DataToRead.Enqueue(th);
                }
            }

            // read image from sensor
            var rawImage = sensor.GetThermalImage();

            // expectation: one write access to register T01L (lower byte of first pixel) to trigger readout
            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.T01L, i2cDevice.DataWritten.Dequeue());

            // expectation: all pixels have been read, so nothing is remaining
            Assert.Empty(i2cDevice.DataToRead);

            for (int y = 0; y < Amg88xx.Rows; y++)
            {
                for (int x = 0; x < Amg88xx.Columns; x++)
                {
                    Assert.Equal(referenceImage[x, y], rawImage[x, y]);
                }
            }
        }

        #region Status

        [Theory]
        [InlineData(0, false)]
        [InlineData((byte)StatusFlag.OVF_IRS, true)]
        [InlineData(0xff ^ (byte)StatusFlag.OVF_IRS, false)]
        public void HasTemperatureOverflowTest(byte statusRegisterContent, bool temperatureOverflow)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(statusRegisterContent);

            bool status = sensor.HasTemperatureOverflow();

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.STAT, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(temperatureOverflow, status);
        }

        [Fact]
        public void ClearTemperatureOverflowTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.ClearTemperatureOverflow();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.SCLR, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)StatusFlag.OVF_IRS, i2cDevice.DataWritten.Dequeue());
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData((byte)StatusFlag.OVF_THS, true)]
        [InlineData(0xff ^ (byte)StatusFlag.OVF_THS, false)]
        public void HasThermistorOverflowTest(byte statusRegisterContent, bool thermistorOverflow)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(statusRegisterContent);

            bool status = sensor.HasThermistorOverflow();

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.STAT, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(thermistorOverflow, status);
        }

        [Fact]
        public void ClearThermistorOverflowTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.ClearThermistorOverflow();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.SCLR, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)StatusFlag.OVF_THS, i2cDevice.DataWritten.Dequeue());
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData((byte)StatusFlag.INTF, true)]
        [InlineData(0xff ^ (byte)StatusFlag.INTF, false)]
        public void HasInterruptTest(byte statusRegisterContent, bool interrupt)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(statusRegisterContent);

            bool status = sensor.HasInterrupt();

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.STAT, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(interrupt, status);
        }

        [Fact]
        public void ClearInterruptTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.ClearInterrupt();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.SCLR, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)StatusFlag.INTF, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void ClearAllStatusTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.ClearAllStatus();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.SCLR, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)(StatusFlag.OVF_IRS | StatusFlag.OVF_THS | StatusFlag.INTF), i2cDevice.DataWritten.Dequeue());
        }

        #endregion

        #region Moving average

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetMovingAverageModeTest(bool expectedMode)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // bit 5: if set, moving average is on
            i2cDevice.DataToRead.Enqueue((byte)(expectedMode ? 0b0001_0000 : 0));

            bool mode = sensor.GetMovingAverageMode();

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.AVE, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedMode, mode);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SetMovingAverageModeTest(bool targetMode)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // bit 5: if set, moving average is on
            i2cDevice.DataToRead.Enqueue((byte)(targetMode ? 0b0001_0000 : 0));

            sensor.SetMovingAverageMode(targetMode);

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.AVE, i2cDevice.DataWritten.Dequeue());
            // bit 5: if set, moving average is on
            Assert.Equal((byte)(targetMode ? 0b0001_0000 : 0), i2cDevice.DataWritten.Dequeue());
        }

        #endregion

        #region Frame Rate
        [Theory]
        [InlineData(0b0000_0000, FrameRate.FPS10)]
        [InlineData(0b0000_0001, FrameRate.FPS1)]
        public void GetFrameRateTest(byte registerContent, FrameRate expectedFrameRate)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(registerContent);

            var frameRate = sensor.GetFrameRate();

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.FPSC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedFrameRate, frameRate);
        }

        [Theory]
        [InlineData(FrameRate.FPS1)]
        [InlineData(FrameRate.FPS10)]
        public void SetFrameRateTest(FrameRate targetFrameRate)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.SetFrameRate(targetFrameRate);

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.FPSC, i2cDevice.DataWritten.Dequeue());
            // if bit 0 of FPSC register is set the frame rate is 1 otherwise 10 fps.
            Assert.Equal((byte)(targetFrameRate == FrameRate.FPS1 ? 0b0000_0001 : 0), i2cDevice.DataWritten.Dequeue());
        }
        #endregion

        #region Operating Mode
        [Theory]
        [InlineData(0b0000_0000, OperatingMode.Normal)]
        [InlineData(0b0001_0000, OperatingMode.Sleep)]
        [InlineData(0b0010_0001, OperatingMode.StandBy10)]
        [InlineData(0b0010_0000, OperatingMode.StandBy60)]
        public void GetOperatingModeTest(byte registerContent, OperatingMode expectedOperatingMode)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(registerContent);

            var operatingMode = sensor.GetOperatingMode();

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.PCLT, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedOperatingMode, operatingMode);
        }

        [Theory]
        [InlineData(OperatingMode.Normal, 0x00)]
        [InlineData(OperatingMode.Sleep, 0x10)]
        [InlineData(OperatingMode.StandBy10, 0x21)]
        [InlineData(OperatingMode.StandBy60, 0x20)]
        public void SetOperatingModeTest(OperatingMode targetOperatingMode, byte expectedMode)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.SetOperatingMode(targetOperatingMode);

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.PCLT, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedMode, i2cDevice.DataWritten.Dequeue());
        }
        #endregion

        #region Reset

        [Fact]
        public void InitialResetTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.InitialReset();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.RST, i2cDevice.DataWritten.Dequeue());
            // writing 0x3f into the reset register initiates an initial reset
            Assert.Equal(0x3f, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void FlagResetTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.FlagReset();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.RST, i2cDevice.DataWritten.Dequeue());
            // writing 0x30 into the reset register initiates a flag reset
            Assert.Equal(0x30, i2cDevice.DataWritten.Dequeue());
        }

        #endregion

        #region Interrupt Control

        [Theory]
        [InlineData(InterruptMode.AbsoluteMode, 0b0000_0010)]
        [InlineData(InterruptMode.DifferenceMode, 0b0000_0000)]
        public void GetInterruptModeTest(InterruptMode expectedMode, byte registerValue)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(registerValue);

            InterruptMode mode = sensor.GetInterruptMode();

            Assert.Single(i2cDevice.DataWritten);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            // bit 1 represents the interrupt mode (not set: difference mode, set: absolute mode)
            Assert.Equal(expectedMode, mode);
        }

        [Theory]
        [InlineData(InterruptMode.AbsoluteMode, true)]
        [InlineData(InterruptMode.DifferenceMode, false)]
        public void SetInterruptModeTest(InterruptMode mode, bool modeBitIsSet)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // assume that none of the register's bits is set, yet => this will be read out by the binding
            i2cDevice.DataToRead.Enqueue(0);

            sensor.SetInterruptMode(mode);

            Assert.Equal(3, i2cDevice.DataWritten.Count);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            // bit 1 represents the interrupt mode (not set: difference mode, set: absolute mode)
            Assert.Equal(modeBitIsSet, (i2cDevice.DataWritten.Dequeue() & 0b0000_0010) != 0);
        }
        #endregion
    }
}
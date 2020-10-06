// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Amg88xx;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;

namespace Iot.Device.Amg88xx.Tests
{
    public class Amg88xxTests
    {
        private readonly ITestOutputHelper _output;

        public Amg88xxTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Thermal image

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
        public void GetThermalRawImageTest()
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
            var rawImage = sensor.GetThermalRawImage();

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

            Temperature[,] referenceImage = new Temperature[Amg88xx.Columns, Amg88xx.Rows];
            Random rnd = new Random();
            for (int y = 0; y < Amg88xx.Rows; y++)
            {
                for (int x = 0; x < Amg88xx.Columns; x++)
                {
                    referenceImage[x, y] = Temperature.FromDegreesCelsius(rnd.Next(-80, 321) * Amg88xxUtils.PixelTemperatureResolution);
                    (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(referenceImage[x, y]);
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

        #endregion

        #region Status

        [Theory]
        [InlineData(1 << (byte)StatusFlagBit.OVF_IRS, true)]
        [InlineData(0xff ^ (1 << (byte)StatusFlagBit.OVF_IRS), false)]
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
            Assert.Equal(1 << (byte)StatusClearBit.OVFCLR, i2cDevice.DataWritten.Dequeue());
        }

        [Theory]
        [InlineData(1 << (byte)StatusFlagBit.OVF_THS, true)]
        [InlineData(0xff ^ (1 << (byte)StatusFlagBit.OVF_THS), false)]
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
            Assert.Equal(1 << (byte)StatusClearBit.OVFTHCLR, i2cDevice.DataWritten.Dequeue());
        }

        [Theory]
        [InlineData(1 << (byte)StatusFlagBit.INTF, true)]
        [InlineData(0xff ^ (1 << (byte)StatusFlagBit.INTF), false)]
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
            Assert.Equal(1 << (byte)StatusClearBit.INTCLR, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void ClearAllFlagsTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.ClearAllFlags();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.SCLR, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)(1 << (byte)StatusClearBit.OVFCLR) | (1 << (byte)StatusClearBit.OVFTHCLR) | (1 << (byte)StatusClearBit.INTCLR), i2cDevice.DataWritten.Dequeue());
        }

        #endregion

        #region Moving average

        [Theory]
        [InlineData(0, false)]
        [InlineData(1 << (byte)MovingAverageModeBit.MAMOD, true)]
        public void GetMovingAverageModeStateTest(byte registerContent, bool expectedState)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(registerContent);

            bool state = sensor.GetMovingAverageModeState();

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.AVE, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedState, state);
        }

        [Theory]
        [InlineData(false, 0x00, 0x00)]
        [InlineData(true, 0x00, 1 << (byte)MovingAverageModeBit.MAMOD)]
        [InlineData(true, 0xff & ~(1 << (byte)MovingAverageModeBit.MAMOD), 0xff)]
        [InlineData(false, 0xff, 0xff & ~(1 << (byte)MovingAverageModeBit.MAMOD))]
        public void SetMovingAverageModeStateTest(bool targetState, byte initialRegisterContent, byte expectedRegisterContent)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // bit 5: if set, moving average is on
            i2cDevice.DataToRead.Enqueue(initialRegisterContent);

            sensor.SetMovingAverageModeState(targetState);

            Assert.Equal(3, i2cDevice.DataWritten.Count);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.AVE, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.AVE, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedRegisterContent, i2cDevice.DataWritten.Dequeue());
        }

        #endregion

        #region Frame Rate
        [Theory]
        [InlineData(0x00, 10)]
        [InlineData(1 << (byte)FrameRateBit.FPS, 1)]
        public void GetFrameRateTest(byte registerContent, int expectedFrameRate)
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
        [InlineData(1, 0x00, 1 << (byte)FrameRateBit.FPS)]
        [InlineData(10, 0x00, 0x00)]
        [InlineData(1, 0xff, 0xff)]
        [InlineData(10, 0xff, 0xff & ~(1 << (byte)FrameRateBit.FPS))]
        public void SetFrameRateTest(int targetFrameRate, byte initialRegisterContent, byte expectedRegisterContent)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(initialRegisterContent);

            sensor.SetFrameRate(targetFrameRate);

            Assert.Equal(3, i2cDevice.DataWritten.Count);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.FPSC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.FPSC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedRegisterContent, i2cDevice.DataWritten.Dequeue());
        }
        #endregion

        #region Operating Mode
        [Theory]
        [InlineData((byte)OperatingMode.Normal, OperatingMode.Normal)]
        [InlineData((byte)OperatingMode.Sleep, OperatingMode.Sleep)]
        [InlineData((byte)OperatingMode.StandBy10, OperatingMode.StandBy10)]
        [InlineData((byte)OperatingMode.StandBy60, OperatingMode.StandBy60)]
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
        [InlineData(OperatingMode.Normal, (byte)OperatingMode.Normal)]
        [InlineData(OperatingMode.Sleep, (byte)OperatingMode.Sleep)]
        [InlineData(OperatingMode.StandBy10, (byte)OperatingMode.StandBy10)]
        [InlineData(OperatingMode.StandBy60, (byte)OperatingMode.StandBy60)]
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
            Assert.Equal((byte)ResetType.Initial, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void FlagResetTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.FlagReset();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.RST, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)ResetType.Flag, i2cDevice.DataWritten.Dequeue());
        }

        #endregion

        #region Interrupt Control

        [Theory]
        [InlineData(InterruptMode.AbsoluteMode, 1 << (byte)InterruptModeBit.INTMODE)]
        [InlineData(InterruptMode.DifferenceMode, 0x00)]
        public void GetInterruptModeTest(InterruptMode expectedMode, byte registerValue)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(registerValue);

            InterruptMode mode = sensor.GetInterruptMode();

            Assert.Single(i2cDevice.DataWritten);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedMode, mode);
        }

        [Theory]
        [InlineData(0x00, InterruptMode.AbsoluteMode, 1 << (byte)InterruptModeBit.INTMODE)]
        [InlineData(0x00, InterruptMode.DifferenceMode, 0x00)]
        [InlineData(0xff, InterruptMode.AbsoluteMode, 0xff)]
        [InlineData(0xff, InterruptMode.DifferenceMode, 0xff & ~(1 << (byte)InterruptModeBit.INTMODE))]
        public void SetInterruptModeTest(byte initialRegisterContent, InterruptMode mode, byte expectedRegisterContent)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(initialRegisterContent);

            sensor.SetInterruptMode(mode);

            Assert.Equal(3, i2cDevice.DataWritten.Count);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedRegisterContent, i2cDevice.DataWritten.Dequeue());
        }

        [Theory]
        [InlineData(0x00, 1 << (byte)InterruptModeBit.INTEN)]
        [InlineData(0xff, 0xff)]
        public void EnableInterruptPinTest(byte initialRegisterContent, byte expectedRegisterContent)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(initialRegisterContent);

            sensor.EnableInterruptPin();

            Assert.Equal(3, i2cDevice.DataWritten.Count);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedRegisterContent, i2cDevice.DataWritten.Dequeue());
        }

        [Theory]
        [InlineData(0x00, 0x00)]
        [InlineData(0xff, 0xff & ~(1 << (byte)InterruptModeBit.INTEN))]
        public void DisableInterruptPinTest(byte initialRegisterContent, byte expectedRegisterContent)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(initialRegisterContent);

            sensor.DisableInterruptPin();

            Assert.Equal(3, i2cDevice.DataWritten.Count);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedRegisterContent, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void GetInterruptLowerLevelTest()
        {
            // expected temeperature: 72.75°C
            // two's complement representation:
            byte expectedTl = 0x23;
            byte expectedTh = 0x01;

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(expectedTl);
            i2cDevice.DataToRead.Enqueue(expectedTh);

            var temperature = sensor.GetInterruptLowerLevel();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.INTLL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTLH, i2cDevice.DataWritten.Dequeue());

            Assert.Equal(Amg88xxUtils.ConvertToTemperature(expectedTl, expectedTh).DegreesCelsius, temperature.DegreesCelsius);
        }

        [Fact]
        public void SetInterruptLowerLevelTest()
        {
            var expectedTemperature = Temperature.FromDegreesCelsius(125);

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.SetInterruptLowerLevel(expectedTemperature);

            Assert.Equal(4, i2cDevice.DataWritten.Count);

            (byte refTl, byte refTh) = Amg88xxUtils.ConvertFromTemperature(expectedTemperature);
            Assert.Equal((byte)Register.INTLL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(refTl, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTLH, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(refTh, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void GetInterruptUpperLevelTest()
        {
            // expected temeperature: 72.75°C
            // two's complement representation:
            byte expectedTl = 0x23;
            byte expectedTh = 0x01;

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(expectedTl);
            i2cDevice.DataToRead.Enqueue(expectedTh);

            var temperature = sensor.GetInterruptUpperLevel();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.INTHL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTHH, i2cDevice.DataWritten.Dequeue());

            Assert.Equal(Amg88xxUtils.ConvertToTemperature(expectedTl, expectedTh).DegreesCelsius, temperature.DegreesCelsius);
        }

        [Fact]
        public void SetInterruptUpperLevelTest()
        {
            var expectedTemperature = Temperature.FromDegreesCelsius(125);

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.SetInterruptUpperLevel(expectedTemperature);

            Assert.Equal(4, i2cDevice.DataWritten.Count);

            (byte refTl, byte refTh) = Amg88xxUtils.ConvertFromTemperature(expectedTemperature);
            Assert.Equal((byte)Register.INTHL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(refTl, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTHH, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(refTh, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void GetInterruptHysteresisLevelTest()
        {
            // expected temeperature: 72.75°C
            // two's complement representation:
            byte expectedTl = 0x23;
            byte expectedTh = 0x01;

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(expectedTl);
            i2cDevice.DataToRead.Enqueue(expectedTh);

            var temperature = sensor.GetInterruptHysteresisLevel();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.INTSL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTSH, i2cDevice.DataWritten.Dequeue());

            Assert.Equal(Amg88xxUtils.ConvertToTemperature(expectedTl, expectedTh).DegreesCelsius, temperature.DegreesCelsius);
        }

        [Fact]
        public void SetInterruptHysteresisLevelTest()
        {
            Temperature expectedTemperature = Temperature.FromDegreesCelsius(125);

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.SetInterruptHysteresisLevel(expectedTemperature);

            Assert.Equal(4, i2cDevice.DataWritten.Count);

            (byte refTl, byte refTh) = Amg88xxUtils.ConvertFromTemperature(expectedTemperature);
            Assert.Equal((byte)Register.INTSL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(refTl, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTSH, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(refTh, i2cDevice.DataWritten.Dequeue());
        }

        [Theory]
        [InlineData(1, 1, new byte[] { 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0001 })]
        [InlineData(2, 2, new byte[] { 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0010, 0b0000_0000 })]
        [InlineData(3, 3, new byte[] { 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0100, 0b0000_0000, 0b0000_0000 })]
        [InlineData(4, 4, new byte[] { 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_1000, 0b0000_0000, 0b0000_0000, 0b0000_0000 })]
        [InlineData(5, 5, new byte[] { 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0001_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000 })]
        [InlineData(6, 6, new byte[] { 0b0000_0000, 0b0000_0000, 0b0010_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000 })]
        [InlineData(7, 7, new byte[] { 0b0000_0000, 0b0100_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000 })]
        [InlineData(8, 8, new byte[] { 0b1000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0000 })]
        public void GetInterruptFlagTableTest(int expectedColumn, int expectedRow, byte[] testData)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // prepare register content (INT0 - INT7)
            for (int i = 7; i >= 0; i--)
            {
                i2cDevice.DataToRead.Enqueue(testData[i]);
            }

            var flags = sensor.GetInterruptFlagTable();

            Assert.Equal(8, i2cDevice.DataWritten.Count);

            var expectedAddresses = new Register[]
            {
                Register.INT0, Register.INT1, Register.INT2, Register.INT3,
                Register.INT4, Register.INT5, Register.INT6, Register.INT7,
            };

            foreach (byte address in expectedAddresses)
            {
                Assert.Equal(address, i2cDevice.DataWritten.Dequeue());
            }

            for (int col = 0; col < Amg88xx.Columns; col++)
            {
                for (int row = 0; row < Amg88xx.Rows; row++)
                {
                    Assert.Equal(col == expectedColumn - 1 && row == expectedRow - 1, flags[col, row]);

                }
            }
        }

        #endregion
    }
}
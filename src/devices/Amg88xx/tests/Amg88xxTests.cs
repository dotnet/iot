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

        [Theory]
        [InlineData((byte)Status.Flag.NONE, false, false)]
        [InlineData((byte)Status.Flag.OVF_IRS, true, false)]
        [InlineData((byte)Status.Flag.INTF, false, true)]
        [InlineData((byte)(Status.Flag.OVF_IRS | Status.Flag.INTF), true, true)]
        public void GetStatus(byte flags, bool temperatureOverflow, bool interrupt)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(flags);
            Status status = sensor.GetStatus();

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.STAT, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(temperatureOverflow, status.TemperatureOverflow);
            Assert.Equal(interrupt, status.Interrupt);
        }

        [Fact]
        public void ClearStatusTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.ClearStatus();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.SCLR, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Status.Flag.ALL, i2cDevice.DataWritten.Dequeue());
        }

        [Theory]
        [InlineData(Status.Flag.NONE)]
        [InlineData(Status.Flag.OVF_IRS)]
        [InlineData(Status.Flag.INTF)]
        [InlineData(Status.Flag.ALL)]
        public void ClearStatusTest_ByFlag(Status.Flag flags)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.ClearStatus(new Status((byte)flags));

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.SCLR, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)flags, i2cDevice.DataWritten.Dequeue());
        }

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
    }
}

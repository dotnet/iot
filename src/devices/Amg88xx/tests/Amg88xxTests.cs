// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Amg88xx;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;

namespace Iot.Device.Amg88xx.Tests
{
    public class Amg88xxTests
    {
        private const double PixelTemperatureResolution = 0.25;

        #region Thermal image
        [Theory]
        [InlineData(0x07, 0xff, 127.9375)]
        [InlineData(0x01, 0x90, 25.0)]
        [InlineData(0x00, 0x04, 0.25)]
        [InlineData(0x00, 0x00, 0.0)]
        [InlineData(0x08, 0x04, -0.25)]
        [InlineData(0x0b, 0xbb, -59.6875)]
        public void SensorTemperatureGetTest(byte th, byte tl, double expectedTemperature)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // target temperature is 25°C which is equivalent to 0x190
            i2cDevice.DataToRead.Enqueue(tl);
            i2cDevice.DataToRead.Enqueue(th);

            Assert.Equal(expectedTemperature, sensor.SensorTemperature.DegreesCelsius);
            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.TTHL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.TTHH, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void RawPixelIndexerTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // using a simple linear sequence of numbers as reference image:
            //   0 to 0x1f8 (504d)
            Int16[] referenceImage = new Int16[Amg88xx.PixelCount];
            for (int n = 0; n < Amg88xx.PixelCount; n++)
            {
                referenceImage[n] = (short)(n * 8);
                // enqueue lower byte (TLxx)
                i2cDevice.DataToRead.Enqueue((byte)(referenceImage[n] & 0xff));
                // enqueue higher byte (THxx)
                i2cDevice.DataToRead.Enqueue((byte)(referenceImage[n] >> 8));
            }

            // read image from sensor
            sensor.ReadImage();

            // expectation: one write access to register T01L (lower byte of first pixel) to trigger readout
            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.T01L, i2cDevice.DataWritten.Dequeue());

            // expectation: all pixels have been read, so nothing is remaining
            Assert.Empty(i2cDevice.DataToRead);

            for (int n = 0; n < Amg88xx.PixelCount; n++)
            {
                Int16 rawPixel = sensor[n];
                Assert.Equal(referenceImage[n], rawPixel);
            }
        }

        [Fact]
        public void PixelTemperatureIndexerTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            Temperature[,] referenceImage = new Temperature[Amg88xx.Width, Amg88xx.Height];
            Random rnd = new Random();
            for (int y = 0; y < Amg88xx.Height; y++)
            {
                for (int x = 0; x < Amg88xx.Width; x++)
                {
                    referenceImage[x, y] = Temperature.FromDegreesCelsius(rnd.Next(-80, 321) * PixelTemperatureResolution);
                    (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(referenceImage[x, y]);
                    i2cDevice.DataToRead.Enqueue(tl);
                    i2cDevice.DataToRead.Enqueue(th);
                }
            }

            // read image from sensor
            sensor.ReadImage();

            // expectation: one write access to register T01L (lower byte of first pixel) to trigger readout
            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.T01L, i2cDevice.DataWritten.Dequeue());

            // expectation: all pixels have been read, so nothing is remaining
            Assert.Empty(i2cDevice.DataToRead);

            for (int y = 0; y < Amg88xx.Height; y++)
            {
                for (int x = 0; x < Amg88xx.Width; x++)
                {
                    Assert.Equal(referenceImage[x, y], sensor[x, y]);
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
        public void UseMovingAverageModeGetTest(byte registerContent, bool expectedState)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(registerContent);

            bool actualState = sensor.UseMovingAverageMode;

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.AVE, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedState, actualState);
        }

        [Theory]
        [InlineData(false, 0x00, 0x00)]
        [InlineData(true, 0x00, 1 << (byte)MovingAverageModeBit.MAMOD)]
        [InlineData(true, 0xff & ~(1 << (byte)MovingAverageModeBit.MAMOD), 0xff)]
        [InlineData(false, 0xff, 0xff & ~(1 << (byte)MovingAverageModeBit.MAMOD))]
        public void UseMovingAverageModeSetTest(bool targetState, byte initialRegisterContent, byte expectedRegisterContent)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // bit 5: if set, moving average is on
            i2cDevice.DataToRead.Enqueue(initialRegisterContent);

            sensor.UseMovingAverageMode = targetState;

            Assert.Equal(3, i2cDevice.DataWritten.Count);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.AVE, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.AVE, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedRegisterContent, i2cDevice.DataWritten.Dequeue());
        }

        #endregion

        #region Frame Rate
        [Theory]
        [InlineData(0x00, FrameRate.Rate10FramesPerSecond)]
        [InlineData(1 << (byte)FrameRateBit.FPS, FrameRate.Rate1FramePerSecond)]
        public void FrameRateGetTest(byte registerContent, FrameRate expectedFrameRate)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(registerContent);

            var actualFrameRate = sensor.FrameRate;

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.FPSC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedFrameRate, actualFrameRate);
        }

        [Theory]
        [InlineData(FrameRate.Rate1FramePerSecond, 0x00, 1 << (byte)FrameRateBit.FPS)]
        [InlineData(FrameRate.Rate10FramesPerSecond, 0x00, 0x00)]
        [InlineData(FrameRate.Rate1FramePerSecond, 0xff, 0xff)]
        [InlineData(FrameRate.Rate10FramesPerSecond, 0xff, 0xff & ~(1 << (byte)FrameRateBit.FPS))]
        public void FrameRateSetTest(FrameRate targetFrameRate, byte initialRegisterContent, byte expectedRegisterContent)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(initialRegisterContent);

            sensor.FrameRate = targetFrameRate;

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
        [InlineData((byte)OperatingMode.StandBy10Seconds, OperatingMode.StandBy10Seconds)]
        [InlineData((byte)OperatingMode.StandBy60Seconds, OperatingMode.StandBy60Seconds)]
        public void OperatingModeGetTest(byte registerContent, OperatingMode expectedOperatingMode)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(registerContent);

            var actualOperatingMode = sensor.OperatingMode;

            Assert.Single(i2cDevice.DataWritten);
            Assert.Equal((byte)Register.PCLT, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedOperatingMode, actualOperatingMode);
        }

        [Theory]
        [InlineData(OperatingMode.Normal, (byte)OperatingMode.Normal)]
        [InlineData(OperatingMode.Sleep, (byte)OperatingMode.Sleep)]
        [InlineData(OperatingMode.StandBy10Seconds, (byte)OperatingMode.StandBy10Seconds)]
        [InlineData(OperatingMode.StandBy60Seconds, (byte)OperatingMode.StandBy60Seconds)]
        public void OperatingModeSetTest(OperatingMode targetOperatingMode, byte expectedMode)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.OperatingMode = targetOperatingMode;

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.PCLT, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedMode, i2cDevice.DataWritten.Dequeue());
        }
        #endregion

        #region Reset

        [Fact]
        public void ResetTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.Reset();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.RST, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)ResetType.Initial, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void FlagResetTest()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.ResetAllFlags();

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.RST, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)ResetType.Flag, i2cDevice.DataWritten.Dequeue());
        }

        #endregion

        #region Interrupt Control

        [Theory]
        [InlineData(InterruptMode.Absolute, 1 << (byte)InterruptModeBit.INTMODE)]
        [InlineData(InterruptMode.Difference, 0x00)]
        public void InterruptModeGetTest(InterruptMode expectedMode, byte registerValue)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(registerValue);

            InterruptMode actualMode = sensor.InterruptMode;

            Assert.Single(i2cDevice.DataWritten);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedMode, actualMode);
        }

        [Theory]
        [InlineData(0x00, InterruptMode.Absolute, 1 << (byte)InterruptModeBit.INTMODE)]
        [InlineData(0x00, InterruptMode.Difference, 0x00)]
        [InlineData(0xff, InterruptMode.Absolute, 0xff)]
        [InlineData(0xff, InterruptMode.Difference, 0xff & ~(1 << (byte)InterruptModeBit.INTMODE))]
        public void InterruptModeSetTest(byte initialRegisterContent, InterruptMode targetMode, byte expectedRegisterContent)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(initialRegisterContent);

            sensor.InterruptMode = targetMode;

            Assert.Equal(3, i2cDevice.DataWritten.Count);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedRegisterContent, i2cDevice.DataWritten.Dequeue());
        }

        [Theory]
        [InlineData(true, 1 << (byte)InterruptModeBit.INTEN)]
        [InlineData(false, 0x00)]
        public void InterruptPinEnabledGetTest(bool expectedState, byte registerValue)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(registerValue);

            bool actualState = sensor.InterruptPinEnabled;

            Assert.Single(i2cDevice.DataWritten);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedState, actualState);
        }

        [Theory]
        [InlineData(0x00, true, 1 << (byte)InterruptModeBit.INTEN)]
        [InlineData(0xff, true, 0xff)]
        [InlineData(0x00, false, 0x00)]
        [InlineData(0xff, false, 0xff & ~(1 << (byte)InterruptModeBit.INTEN))]
        public void InterruptPinEnabledSetTest(byte initialRegisterContent, bool targetState, byte expectedRegisterContent)
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(initialRegisterContent);

            sensor.InterruptPinEnabled = targetState;

            Assert.Equal(3, i2cDevice.DataWritten.Count);
            // register address is expected two times: once for reading the current register value and once for writing the new one
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTC, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(expectedRegisterContent, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void InterruptLowerLevelGetTest()
        {
            // expected temeperature: 72.75°C
            // two's complement representation:
            byte expectedTl = 0x23;
            byte expectedTh = 0x01;

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(expectedTl);
            i2cDevice.DataToRead.Enqueue(expectedTh);

            Temperature actualTemperature = sensor.InterruptLowerLevel;

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.INTLL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTLH, i2cDevice.DataWritten.Dequeue());

            Assert.Equal(Amg88xxUtils.ConvertToTemperature(expectedTl, expectedTh).DegreesCelsius, actualTemperature.DegreesCelsius);
        }

        [Fact]
        public void InterruptLowerLevelSetTest()
        {
            var expectedTemperature = Temperature.FromDegreesCelsius(125);

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.InterruptLowerLevel = expectedTemperature;

            Assert.Equal(4, i2cDevice.DataWritten.Count);

            (byte refTl, byte refTh) = Amg88xxUtils.ConvertFromTemperature(expectedTemperature);
            Assert.Equal((byte)Register.INTLL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(refTl, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTLH, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(refTh, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void InterruptUpperLevelGetTest()
        {
            // expected temeperature: 72.75°C
            // two's complement representation:
            byte expectedTl = 0x23;
            byte expectedTh = 0x01;

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(expectedTl);
            i2cDevice.DataToRead.Enqueue(expectedTh);

            Temperature actualTemperature = sensor.InterruptUpperLevel;

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.INTHL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTHH, i2cDevice.DataWritten.Dequeue());

            Assert.Equal(Amg88xxUtils.ConvertToTemperature(expectedTl, expectedTh).DegreesCelsius, actualTemperature.DegreesCelsius);
        }

        [Fact]
        public void InterruptUpperLevelSetTest()
        {
            var expectedTemperature = Temperature.FromDegreesCelsius(125);

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.InterruptUpperLevel = expectedTemperature;

            Assert.Equal(4, i2cDevice.DataWritten.Count);

            (byte refTl, byte refTh) = Amg88xxUtils.ConvertFromTemperature(expectedTemperature);
            Assert.Equal((byte)Register.INTHL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(refTl, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTHH, i2cDevice.DataWritten.Dequeue());
            Assert.Equal(refTh, i2cDevice.DataWritten.Dequeue());
        }

        [Fact]
        public void InterruptHysteresisLevelGetTest()
        {
            // expected temeperature: 72.75°C
            // two's complement representation:
            byte expectedTl = 0x23;
            byte expectedTh = 0x01;

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            i2cDevice.DataToRead.Enqueue(expectedTl);
            i2cDevice.DataToRead.Enqueue(expectedTh);

            Temperature actualTemperature = sensor.InterruptHysteresis;

            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.INTSL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.INTSH, i2cDevice.DataWritten.Dequeue());

            Assert.Equal(Amg88xxUtils.ConvertToTemperature(expectedTl, expectedTh).DegreesCelsius, actualTemperature.DegreesCelsius);
        }

        [Fact]
        public void InterruptHysteresisLevelSetTest()
        {
            Temperature expectedTemperature = Temperature.FromDegreesCelsius(125);

            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            sensor.InterruptHysteresis = expectedTemperature;

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

            for (int col = 0; col < Amg88xx.Width; col++)
            {
                for (int row = 0; row < Amg88xx.Height; row++)
                {
                    Assert.Equal(col == expectedColumn - 1 && row == expectedRow - 1, flags[col, row]);
                }
            }
        }

        #endregion
    }
}

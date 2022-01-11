// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;
using Moq;
using Xunit;
using static Iot.Device.SensorHub.SensorHub;

namespace Iot.Device.SensorHub.Tests
{
    public class SensorHubTests
    {
        private Mock<I2cDevice> _mockI2cDevice;

        public SensorHubTests()
        {
            _mockI2cDevice = new();
            _mockI2cDevice.SetupGet(c => c.ConnectionSettings).Returns(new I2cConnectionSettings(1, 0x17));
        }

        [Fact]
        public void Should_Throw_When_NoDevice_Found()
        {
            Func<SensorHub> sh = () => new(_mockI2cDevice.Object);
            _mockI2cDevice.Setup(x => x.ReadByte()).Throws(new IOException("Not Connected"));
            var exception = Assert.Throws<IOException>(sh);
            Assert.StartsWith("No response from SensorHub", exception.Message);
        }

        [Fact]
        public void Should_Throw_When_No_Offboard_Temperature_Sensor_Found()
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.SetupSequence(x => x.ReadByte())
                .Returns((byte)SensorHub.StatusFunctionError.TEMP_NOT_FOUND);

            var exception = Assert.Throws<IOException>(() => sh.TryReadOffBoardTemperature(out var t));
            Assert.Equal("No offboard temperature sensor found", exception.Message);
        }

        [Fact]
        public void Should_Not_Read_Offboard_Temperature_On_Sensor_Overflow()
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.SetupSequence(x => x.ReadByte())
                .Returns((byte)StatusFunctionError.TEMP_OVERFLOW);

            Assert.False(sh.TryReadOffBoardTemperature(out _));
        }

        [Theory]
        [InlineData(0x00)] // No Error
        [InlineData(0x04)] // Light Brightness Overflow
        [InlineData(0x08)] // Light Brightness Not Found
        public void Should_Read_Correct_Offboard_Sensor_Temperature(byte statusReg)
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.SetupSequence(x => x.ReadByte())
                .Returns(statusReg)
                .Returns(128);

            sh.TryReadOffBoardTemperature(out var t);
            double expected = 128.0;
            Assert.Equal(t.DegreesCelsius, expected);
        }

        [Fact]
        public void Should_Read_Correct_Barometer_Temperature()
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.SetupSequence(x => x.ReadByte())
                .Returns(SensorHub.NO_ERROR)
                .Returns(50);

            sh.TryReadBarometerTemperature(out var t);
            double expected = 50.0;
            Assert.Equal(t.DegreesCelsius, expected);
        }

        [Fact]
        public void Should_Read_Correct_Barometer_Pressure()
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.SetupSequence(x => x.ReadByte())
                .Returns(SensorHub.NO_ERROR)
                .Returns(0) // H
                .Returns(19) // M
                .Returns(136); // L

            sh.TryReadBarometerPressure(out var p);
            double expected = 5000.0;
            Assert.Equal(p.Pascals, expected);
        }

        [Theory]
        [InlineData(0x00)] // No error
        [InlineData(0x01)] // Ext. Temperature Overflow
        [InlineData(0x02)] // Ext. Temperature Not Found
        public void Should_Read_Correct_Illuminance(byte statusReg)
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.SetupSequence(x => x.ReadByte())
                .Returns(statusReg)
                .Returns(3) // H
                .Returns(12); // L

            sh.TryReadIlluminance(out var l);
            double expected = 780.0;
            Assert.Equal(l.Lux, expected);
        }

        [Theory]
        [InlineData(0x04)] // Light Brightness Overflow
        [InlineData(0x08)] // Light Brightness Not Found
        public void Should_Not_Read_Illuminance(byte statusReg)
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.SetupSequence(x => x.ReadByte())
                .Returns(statusReg);

            Assert.False(sh.TryReadIlluminance(out _));
        }

        [Fact]
        public void Should_Read_Correct_Humidity()
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.SetupSequence(x => x.ReadByte())
                .Returns(SensorHub.NO_ERROR)
                .Returns(50);

            sh.TryReadRelativeHumidity(out var h);
            double expected = 50.0;
            Assert.Equal(h.Percent, expected);
        }

        [Fact]
        public void Should_Read_Correct_Onboard_Sensor_Temperature()
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.SetupSequence(x => x.ReadByte())
                .Returns(SensorHub.NO_ERROR)
                .Returns(50);

            sh.TryReadOnBoardTemperature(out var t);
            double expected = 50.0;
            Assert.Equal(t.DegreesCelsius, expected);
        }

        [Fact]
        public void Should_MotionDetect()
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.Setup(x => x.ReadByte()).Returns(1);
            Assert.True(sh.IsMotionDetected);
        }

        [Fact]
        public void Should_Not_MotionDetect()
        {
            SensorHub sh = new(_mockI2cDevice.Object);
            _mockI2cDevice.Setup(x => x.ReadByte()).Returns(0);
            Assert.False(sh.IsMotionDetected);
        }
    }
}

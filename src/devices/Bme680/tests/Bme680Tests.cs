// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.IO;
using System.Linq;
using Xunit;

namespace Iot.Device.Bme680.Tests
{
    /// <summary>
    /// Unit tests for <see cref="Bme680"/>.
    /// </summary>
    public class Bme680Tests
    {
        /// <summary>
        /// A mock communications channel to a device on an I2C bus.
        /// </summary>
        private readonly MockI2cDevice _mockI2cDevice;

        /// <summary>
        /// The <see cref="Bme680"/> to test with.
        /// </summary>
        private readonly Bme680 _bme680;

        /// <summary>
        /// The expected chip ID of the BME68x product family.
        /// </summary>
        private const byte _expectedChipId = 0x61;

        /// <summary>
        /// Initialize a new instance of the <see cref="Bme680"/> class.
        /// </summary>
        public Bme680Tests()
        {
            // Arrange.
            var settings = new I2cConnectionSettings(default, Bme680.DefaultI2cAddress);
            _mockI2cDevice = new MockI2cDevice(settings);

            // By default, return the expected chip ID.
            _mockI2cDevice.ReadByteSetupReturns = _expectedChipId;

            _bme680 = new Bme680(_mockI2cDevice);
        }

        /// <summary>
        /// Ensure that the default I2C address is 0x76.
        /// </summary>
        [Fact]
        public void DefaultI2cAddress_HasValue_0x76()
        {
            // Arrange.
            byte expected = 0x76;

            // Act.
            var actual = Bme680.DefaultI2cAddress;

            // Assert.
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Ensure that the secondary I2C address is 0x77.
        /// </summary>
        [Fact]
        public void SecondaryI2cAddress_HasValue_0x77()
        {
            // Arrange.
            byte expected = 0x77;

            // Act.
            var actual = Bme680.SecondaryI2cAddress;

            // Assert.
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// On construction, ensure that an <see cref="ArgumentNullException"/> is thrown if
        /// <see cref="Bme680.Bme680(IComDevice)"/> is called with a null <see cref="IComDevice"/>.
        /// </summary>
        [Fact]
        public void Bme680_NullComDevice_ThrowsArgumentNullException()
        {
            // Arrange, Act and Assert.
            Assert.Throws<ArgumentNullException>(() => new Bme680(null));
        }

        /// <summary>
        /// On construction, ensure that all invalid addresses throw an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        [Fact]
        public void Bme680_AddressOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var invalidAddresses = Enumerable.Range(byte.MinValue, byte.MaxValue)
                .Where(address =>
                    address != Bme680.DefaultI2cAddress &&
                    address != Bme680.SecondaryI2cAddress);

            foreach (var invalidAddress in invalidAddresses)
            {
                var settings = new I2cConnectionSettings(default, invalidAddress);
                var mockI2cDevice = new MockI2cDevice(settings);

                // Act and Assert.
                Assert.Throws<ArgumentOutOfRangeException>(() => new Bme680(mockI2cDevice));
            }
        }

        /// <summary>
        /// On construction, if the chip ID does not match what is expected (0x61), then a <see cref="IOException"/> is thrown.
        /// </summary>
        [Fact]
        public void Bme680_WrongChipId_ThrowsIOException()
        {
            // Arrange.
            _mockI2cDevice.ReadByteSetupReturns = default;

            // Act and Assert.
            Assert.Throws<IOException>(() => new Bme680(_mockI2cDevice));
        }

        /// <summary>
        /// Ensure the <see cref="Register.Ctrl_meas"/> is read from.
        /// </summary>
        [Fact]
        public void GetPowerMode_CallsWriteByte_WithCorrectRegister()
        {
            // Act.
            var powerMode = _bme680.PowerMode;

            // Assert.
            Assert.Equal((byte)Register.Ctrl_meas, _mockI2cDevice.WriteByteCalledWithValue);
        }

        /// <summary>
        /// For each set <see cref="PowerMode"/>, ensure the correct value is returned.
        /// </summary>
        [Theory]
        [InlineData(0b_0000_0000, PowerMode.Sleep)]
        [InlineData(0b_1111_1100, PowerMode.Sleep)]
        [InlineData(0b_0000_0001, PowerMode.Forced)]
        [InlineData(0b_1111_1101, PowerMode.Forced)]
        public void GetPowerMode_Returns_CorrectPowerMode(byte readBits, PowerMode expected)
        {
            // Arrange.
            _mockI2cDevice.ReadByteSetupReturns = readBits;

            // Act.
            var actual = _bme680.PowerMode;

            // Assert.
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Given a new data bit value, return the correct boolean value.
        /// </summary>
        /// <param name="readBits">A given byte containing the new data bit.</param>
        /// <param name="expected">The corresponding boolean value.</param>
        [Theory]
        [InlineData(0b_000_0000, false)]
        [InlineData(0b_0111_1111, false)]
        [InlineData(0b_1000_0000, true)]
        [InlineData(0b_1111_1111, true)]
        public void HasNewData_Returns_CorrectValue(byte readBits, bool expected)
        {
            // Arrange.
            _mockI2cDevice.ReadByteSetupReturns = readBits;

            // Act.
            var actual = _bme680.HasNewData;

            // Assert.
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Ensure the <see cref="Register.eas_status_0"/> is read from.
        /// </summary>
        [Fact]
        public void HasNewData_CallsWriteByte_WithCorrectRegister()
        {
            // Act.
            var hasNewData = _bme680.HasNewData;

            // Assert.
            Assert.Equal((byte)Register.eas_status_0, _mockI2cDevice.WriteByteCalledWithValue);
        }

        /// <summary>
        /// It should write the <see cref="Register.Ctrl_meas"/> register so the register value can be read from.
        /// </summary>
        [Fact]
        public void SetPressureOversampling_CallsWriteByte_WithCorrectRegister()
        {
            // Arrange.
            var expected = (byte)Register.Ctrl_meas;

            // Act.
            _bme680.SetPressureOversampling(Oversampling.Skipped);

            // Assert.
            Assert.Equal(expected, _mockI2cDevice.WriteByteCalledWithValue);
        }

        /// <summary>
        /// Given any state of read bits, the correct value the correct value should be written.
        /// </summary>
        /// <param name="readBits">The read bits to test with.</param>
        [Theory]
        [InlineData(0b_0000_0000)]
        [InlineData(0b_1111_1111)]
        public void SetPressureOversampling_CallsWrite_WithCorrectValue(byte readBits)
        {
            // Arrange.
            _mockI2cDevice.ReadByteSetupReturns = readBits;
            var oversampling = Oversampling.x2;
            byte cleared = (byte)(readBits & 0b_1110_0011);
            byte expectedBits = (byte)(cleared | ((byte)oversampling << 2));
            byte[] expected = new[] { (byte)Register.Ctrl_meas, expectedBits };

            // Act.
            _bme680.SetPressureOversampling(oversampling);

            // Assert.
            Assert.Equal(expected, _mockI2cDevice.WriteCalledWithValue);
        }

        /// <summary>
        /// It should write the <see cref="Register.Ctrl_hum"/> register so the register value can be read from.
        /// </summary>
        [Fact]
        public void SetHumidityOversampling_CallsWriteByte_WithCorrectRegister()
        {
            // Arrange.
            var expected = (byte)Register.Ctrl_hum;

            // Act.
            _bme680.SetHumidityOversampling(Oversampling.Skipped);

            // Assert.
            Assert.Equal(expected, _mockI2cDevice.WriteByteCalledWithValue);
        }

        /// <summary>
        /// Given any state of read bits, the correct value the correct value should be written.
        /// </summary>
        /// <param name="readBits">The read bits to test with.</param>
        [Theory]
        [InlineData(0b_0000_0000)]
        [InlineData(0b_1111_1111)]
        public void SetHumidityOversampling_CallsWrite_WithCorrectValue(byte readBits)
        {
            // Arrange.
            _mockI2cDevice.ReadByteSetupReturns = readBits;
            var oversampling = Oversampling.x2;
            byte cleared = (byte)(readBits & 0b_1111_1000);
            byte expectedBits = (byte)(cleared | (byte)oversampling);
            byte[] expected = new[] { (byte)Register.Ctrl_hum, expectedBits };

            // Act.
            _bme680.SetHumidityOversampling(oversampling);

            // Assert.
            Assert.Equal(expected, _mockI2cDevice.WriteCalledWithValue);
        }

        [Fact]
        public void SetPowerMode_CallsWriteByte_WithCorrectRegister()
        {
            // Arrange.
            var expected = (byte)Register.Ctrl_meas;

            // Act.
            _bme680.SetPowerMode(PowerMode.Forced);

            // Assert.
            Assert.Equal(expected, _mockI2cDevice.WriteByteCalledWithValue);
        }

        [Theory]
        [InlineData(0b_0000_0000)]
        [InlineData(0b_1111_1111)]
        public void SetPowerMode_CallsWrite_WithCorrectValue(byte readBits)
        {
            // Arrange.
            _mockI2cDevice.ReadByteSetupReturns = readBits;
            var powerMode = PowerMode.Forced;
            var cleared = (byte)(readBits & 0b_1111_1100);
            byte expectedBits = (byte)(cleared | (byte)powerMode);
            byte[] expected = new[] { (byte)Register.Ctrl_meas, expectedBits };

            // Act.
            _bme680.SetPowerMode(powerMode);

            // Assert.
            Assert.Equal(expected, _mockI2cDevice.WriteCalledWithValue);
        }

        /// <summary>
        /// It should write the <see cref="Register.Ctrl_meas"/> register so the register value can be read from.
        /// </summary>
        [Fact]
        public void SetTemperatureOversampling_CallsWriteByte_WithCorrectRegister()
        {
            // Arrange.
            var expected = (byte)Register.Ctrl_meas;

            // Act.
            _bme680.SetTemperatureOversampling(Oversampling.Skipped);

            // Assert.
            Assert.Equal(expected, _mockI2cDevice.WriteByteCalledWithValue);
        }

        /// <summary>
        /// Given any state of read bits, the correct value the correct value should be written.
        /// </summary>
        /// <param name="readBits">The read bits to test with.</param>
        [Theory]
        [InlineData(0b_0000_0000)]
        [InlineData(0b_1111_1111)]
        public void SetTemperatureOversampling_CallsWrite_WithCorrectValue(byte readBits)
        {
            // Arrange.
            _mockI2cDevice.ReadByteSetupReturns = readBits;
            var oversampling = Oversampling.x2;
            byte cleared = (byte)(readBits & 0b_0001_1111);
            byte expectedBits = (byte)(cleared | (byte)oversampling << 5);
            byte[] expected = new[] { (byte)Register.Ctrl_meas, expectedBits };

            // Act.
            _bme680.SetTemperatureOversampling(oversampling);

            // Assert.
            Assert.Equal(expected, _mockI2cDevice.WriteCalledWithValue);
        }
    }
}

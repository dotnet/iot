// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.IO;
using Iot.Units;

namespace Iot.Device.Bme680
{
    /// <summary>
    /// Represents a BME680 gas, temperature, humidity and pressure sensor.
    /// </summary>
    public class Bme680 : IDisposable
    {
        /// <summary>
        /// Default I2C bus address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x76;

        /// <summary>
        /// Gets a value indicating whether new data is available.
        /// </summary>
        public bool HasNewData => ReadHasNewData();

        /// <summary>
        /// Gets the humidity in %rH (percentage relative humidity).
        /// </summary>
        public double Humidity => ReadHumidity();

        /// <summary>
        /// Gets the <see cref="PowerMode"/>.
        /// </summary>
        public PowerMode PowerMode => ReadPowerMode();

        /// <summary>
        /// Get the pressure in Pa (Pascal).
        /// </summary>
        public double Pressure => ReadPressure();

        /// <summary>
        /// Secondary I2C bus address.
        /// </summary>
        public const byte SecondaryI2cAddress = 0x77;

        /// <summary>
        /// Gets the <see cref="Temperature"/>.
        /// </summary>
        public Temperature Temperature => ReadTemperature();

        /// <summary>
        /// Calibration data specific to the device.
        /// </summary>
        private readonly CalibrationData _calibrationData = new CalibrationData();

        /// <summary>
        /// The expected chip ID of the BME68x product family.
        /// </summary>
        private readonly byte _expectedChipId = 0x61;

        /// <summary>
        /// The communications channel to a device on an I2C bus.
        /// </summary>
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initialize a new instance of the <see cref="Bme680"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException"></exception>
        public Bme680(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            // Ensure a valid device address has been set.
            int deviceAddress = i2cDevice.ConnectionSettings.DeviceAddress;
            if (deviceAddress < DefaultI2cAddress || deviceAddress > SecondaryI2cAddress)
            {
                throw new ArgumentOutOfRangeException(nameof(i2cDevice),
                    $"Device address {deviceAddress} is out of range. Expected {DefaultI2cAddress} or {SecondaryI2cAddress}");
            }

            // Ensure the device exists on the I2C bus.
            byte readChipId = Read8Bits(Register.CHIPID);
            if (readChipId != _expectedChipId)
            {
                throw new IOException($"Unable to find a chip with id {_expectedChipId}");
            }

            _calibrationData.ReadFromDevice(this);
        }

        /// <summary>
        /// Set the humidity oversampling.
        /// </summary>
        /// <param name="oversampling">The <see cref="Oversampling"/> to set.</param>
        public void SetHumidityOversampling(Oversampling oversampling)
        {
            var register = Register.CONTROL_HUM;
            byte read = Read8Bits(register);

            // Clear first 3 bits.
            var cleared = (byte)(read & 0b_1111_1000);

            _i2cDevice.Write(new[] { (byte)register, (byte)(cleared | (byte)oversampling) });
        }

        /// <summary>
        /// Set the power mode.
        /// </summary>
        /// <param name="powerMode">The <see cref="PowerMode"/> to set.</param>
        public void SetPowerMode(PowerMode powerMode)
        {
            var register = Register.CONTROL;
            byte read = Read8Bits(register);

            // Clear first 2 bits.
            var cleared = (byte)(read & 0b_1111_1100);

            _i2cDevice.Write(new[] { (byte)register, (byte)(cleared | (byte)powerMode) });
        }

        /// <summary>
        /// Set the pressure oversampling.
        /// </summary>
        /// <param name="oversampling">The <see cref="Oversampling"/> value to set.</param>
        public void SetPressureOversampling(Oversampling oversampling)
        {
            var register = Register.CONTROL;
            byte read = Read8Bits(register);

            // Clear bits 4, 3 and 2.
            var cleared = (byte)(read & 0b_1110_0011);

            _i2cDevice.Write(new[] { (byte)register, (byte)(cleared | (byte)oversampling << 2) });
        }

        /// <summary>
        /// Set the temperature oversampling.
        /// </summary>
        /// <param name="oversampling">The <see cref="Oversampling"/> value to set.</param>
        public void SetTemperatureOversampling(Oversampling oversampling)
        {
            var register = Register.CONTROL;
            byte read = Read8Bits(register);

            // Clear last 3 bits.
            var cleared = (byte)(read & 0b_0001_1111);

            _i2cDevice.Write(new[] { (byte)register, (byte)(cleared | (byte)oversampling << 5) });
        }

        /// <summary>
        /// Read 8 bits from a given <see cref="Register"/>.
        /// </summary>
        /// <param name="register">The <see cref="Register"/> to read from.</param>
        /// <returns>Value from register.</returns>
        /// <remarks>
        /// Cast to an <see cref="sbyte"/> if you want to read a signed value.
        /// </remarks>
        internal byte Read8Bits(Register register)
        {
            _i2cDevice.WriteByte((byte)register);

            return _i2cDevice.ReadByte();
        }

        /// <summary>
        /// Read 16 bits from a given <see cref="Register"/> LSB first.
        /// </summary>
        /// <param name="register">The <see cref="Register"/> to read from.</param>
        /// <returns>Value from register.</returns>
        internal short Read16Bits(Register register)
        {
            Span<byte> buffer = stackalloc byte[2];

            _i2cDevice.WriteByte((byte)register);
            _i2cDevice.Read(buffer);

            return BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }

        /// <summary>
        /// Read a value indicating whether or not new sensor data is available.
        /// </summary>
        /// <returns>True if new data is available.</returns>
        private bool ReadHasNewData()
        {
            var register = Register.STATUS;
            byte read = Read8Bits(register);

            // Get only the power mode bit.
            var hasNewData = (byte)(read & 0b_1000_0000);

            return (hasNewData >> 7) == 1;
        }

        /// <summary>
        /// Read the <see cref="PowerMode"/> state.
        /// </summary>
        /// <returns>The current <see cref="PowerMode"/>.</returns>
        private PowerMode ReadPowerMode()
        {
            var register = Register.CONTROL;
            byte read = Read8Bits(register);

            // Get only the power mode bits.
            var powerMode = (byte)(read & 0b_0000_0011);

            return (PowerMode)powerMode;
        }

        /// <summary>
        /// Read the humidity.
        /// </summary>
        /// <returns>Calculated humidity.</returns>
        private double ReadHumidity()
        {
            // Read humidity data.
            byte msb = Read8Bits(Register.HUMIDITYDATA_MSB);
            byte lsb = Read8Bits(Register.HUMIDITYDATA_LSB);
            var temperature = Temperature.Celsius;

            // Convert to a 32bit integer.
            var adcHumidity = (msb << 8) + lsb;

            // Calculate the humidity.
            var var1 = adcHumidity - ((_calibrationData.DigH1 * 16.0) + ((_calibrationData.DigH3 / 2.0) * temperature));
            var var2 = var1 * ((_calibrationData.DigH2 / 262144.0) * (1.0 + ((_calibrationData.DigH4 / 16384.0) * temperature)
                + ((_calibrationData.DigH5 / 1048576.0) * temperature * temperature)));
            var var3 = _calibrationData.DigH6 / 16384.0;
            var var4 = _calibrationData.DigH7 / 2097152.0;
            var calculatedHumidity = var2 + ((var3 + (var4 * temperature)) * var2 * var2);

            if (calculatedHumidity > 100.0)
            {
                calculatedHumidity = 100.0;
            }
            else if (calculatedHumidity < 0.0)
            {
                calculatedHumidity = 0.0;
            }

            return calculatedHumidity;
        }

        /// <summary>
        /// Read the pressure.
        /// </summary>
        /// <returns>Calculated pressure.</returns>
        private double ReadPressure()
        {
            // Read pressure data.
            byte lsb = Read8Bits(Register.PRESSUREDATA_LSB);
            byte msb = Read8Bits(Register.PRESSUREDATA_MSB);
            byte xlsb = Read8Bits(Register.PRESSUREDATA_XLSB);

            // Convert to a 32bit integer.
            var adcPressure = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            // Calculate the pressure.
            var var1 = (Temperature.Celsius * 5120.0 / 2.0) - 64000.0;
            var var2 = var1 * var1 * (_calibrationData.DigP6 / 131072.0);
            var2 += (var1 * _calibrationData.DigP5 * 2.0);
            var2 = (var2 / 4.0) + (_calibrationData.DigP4 * 65536.0);
            var1 = ((_calibrationData.DigP3 * var1 * var1 / 16384.0) + (_calibrationData.DigP2 * var1)) / 524288.0;
            var1 = (1.0 + (var1 / 32768.0)) * _calibrationData.DigP1;
            var calculatedPressure = 1048576.0 - adcPressure;

            // Avoid exception caused by division by zero.
            if (var1 != 0)
            {
                calculatedPressure = (calculatedPressure - (var2 / 4096.0)) * 6250.0 / var1;
                var1 = _calibrationData.DigP9 * calculatedPressure * calculatedPressure / 2147483648.0;
                var2 = calculatedPressure * (_calibrationData.DigP8 / 32768.0);
                var var3 = (calculatedPressure / 256.0) * (calculatedPressure / 256.0) * (calculatedPressure / 256.0)
                    * (_calibrationData.DigP10 / 131072.0);
                calculatedPressure += (var1 + var2 + var3 + (_calibrationData.DigP7 * 128.0)) / 16.0;
            }
            else
            {
                calculatedPressure = 0;
            }

            return calculatedPressure;
        }

        /// <summary>
        /// Read the temperature.
        /// </summary>
        /// <returns>Calculated temperature.</returns>
        private Temperature ReadTemperature()
        {
            // Read temperature data.
            byte lsb = Read8Bits(Register.TEMPDATA_LSB);
            byte msb = Read8Bits(Register.TEMPDATA_MSB);
            byte xlsb = Read8Bits(Register.TEMPDATA_XLSB);

            // Convert to a 32bit integer.
            var adcTemperature = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            // Calculate the temperature.
            var var1 = ((adcTemperature / 16384.0) - (_calibrationData.DigT1 / 1024.0)) * _calibrationData.DigT2;
            var var2 = ((adcTemperature / 131072.0) - (_calibrationData.DigT1 / 8192.0)) * _calibrationData.DigT3;

            return Temperature.FromCelsius((var1 + var2) / 5120.0);
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.IO;
using UnitsNet;

namespace Iot.Device.SensorHub
{
    /// <summary>
    /// SensorHub
    /// </summary>
    public sealed class SensorHub
    {
        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int DefaultI2cAddress = 0x17;

        internal const byte NO_ERROR = 0x0;

        private readonly I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorHub"/> class.
        /// </summary>
        public SensorHub(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            try
            {
                _i2cDevice.WriteByte((byte)_i2cDevice.ConnectionSettings.DeviceAddress);
                _i2cDevice.ReadByte();
            }
            catch (IOException ex)
            {
                throw new IOException($"No response from SensorHub with address {_i2cDevice.ConnectionSettings.DeviceAddress}", ex);
            }
        }

        /// <summary>
        /// Try to read the temperature from the off board thermometer.
        /// </summary>
        /// <remarks>Range is -30 to 127 Celsius.</remarks>
        /// <param name="temperature">The temperature if successful</param>
        /// <returns><c>True</c> on success, <c>False</c> otherwise</returns>
        /// <exception cref="IOException">Thrown when ext. temperature sensor not found.</exception>
        public bool TryReadOffBoardTemperature(out Temperature temperature)
        {
            temperature = default;

            var status = ReadRegister(Register.STATUS_REG);
            if (IsStatusRegError(status, StatusFunctionError.TEMP_NOT_FOUND))
            {
                throw new IOException("No offboard temperature sensor found");
            }
            else if (IsStatusRegError(status, StatusFunctionError.TEMP_OVERFLOW))
            {
                return false;
            }

            temperature = Temperature.FromDegreesCelsius(ReadRegister(Register.TEMP_REG));
            return true;
        }

        /// <summary>
        /// Try to read the temperature from the barometer sensor.
        /// </summary>
        /// <remarks>Range is -40 to 80 Celsius.</remarks>
        /// <param name="temperature">Temperature if successful</param>
        /// <returns><c>True</c> on success, <c>False</c> otherwise</returns>
        public bool TryReadBarometerTemperature(out Temperature temperature)
        {
            temperature = default;
            if (ReadRegister(Register.BMP280_STATUS).Equals(NO_ERROR))
            {
                temperature = Temperature.FromDegreesCelsius(ReadRegister(Register.BMP280_TEMP_REG));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to read pressure from sensor.
        /// </summary>
        /// <remarks>Range is 300Pa to 1100hPa</remarks>
        /// <param name="pressure">Pressure if successful</param>
        /// <returns><c>True</c> on success, <c>False</c> otherwise</returns>
        public bool TryReadBarometerPressure(out Pressure pressure)
        {
            pressure = default;
            if (ReadRegister(Register.BMP280_STATUS).Equals(NO_ERROR))
            {
                Span<byte> bytes = stackalloc byte[4]
                {
                    0,
                    ReadRegister(Register.BMP280_PRESSURE_REG_H),
                    ReadRegister(Register.BMP280_PRESSURE_REG_M),
                    ReadRegister(Register.BMP280_PRESSURE_REG_L)
                };

                pressure = Pressure.FromPascals(BinaryPrimitives.ReadInt32BigEndian(bytes));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to read illuminance from sensor.
        /// </summary>
        /// <remarks>Range is 0 to 1800 Lux.</remarks>
        /// <param name="illuminance">Illuminance if successful</param>
        /// <returns><c>True</c> on success, <c>False</c> otherwise</returns>
        public bool TryReadIlluminance(out Illuminance illuminance)
        {
            illuminance = default;

            var status = ReadRegister(Register.STATUS_REG);
            if (IsStatusRegError(status, StatusFunctionError.LIGHT_BRIGHTNESS_NOT_FOUND)
                || IsStatusRegError(status, StatusFunctionError.LIGHT_BRIGHTNESS_OVERFLOW))
            {
                return false;
            }

            Span<byte> bytes = stackalloc byte[2]
            {
                ReadRegister(Register.LIGHT_REG_H),
                ReadRegister(Register.LIGHT_REG_L)
            };

            illuminance = new(BinaryPrimitives.ReadInt16BigEndian(bytes), UnitsNet.Units.IlluminanceUnit.Lux);
            return true;
        }

        /// <summary>
        /// Try to read relative humidity from sensor.
        /// </summary>
        /// <remarks>Range is 25 to 95 percent</remarks>
        /// <param name="humidity">Relative humidity if successful</param>
        /// <returns><c>True</c> on success, <c>False</c> otherwise</returns>
        public bool TryReadRelativeHumidity(out RelativeHumidity humidity)
        {
            humidity = default;
            if (ReadRegister(Register.ON_BOARD_SENSOR_ERROR).Equals(NO_ERROR))
            {
                humidity = RelativeHumidity.FromPercent(ReadRegister(Register.ON_BOARD_HUMIDITY_REG));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to read temperature from on board sensor
        /// </summary>
        /// <remakrs>Range is -20 to 60 Celsius</remakrs>
        /// <param name="temperature">Temperature if successful</param>
        /// <returns><c>True</c> on success, <c>False</c> otherwise</returns>
        public bool TryReadOnBoardTemperature(out Temperature temperature)
        {
            temperature = default;
            if (ReadRegister(Register.ON_BOARD_SENSOR_ERROR).Equals(NO_ERROR))
            {
                temperature = Temperature.FromDegreesCelsius(ReadRegister(Register.ON_BOARD_TEMP_REG));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Is motion detected by the on board sensor
        /// </summary>
        public bool IsMotionDetected => ReadRegister(Register.MOTION_DETECTED) == 1;

        private static bool IsStatusRegError(byte status, StatusFunctionError error) => (status & (byte)error) != 0;

        private byte ReadRegister(Register register)
        {
            _i2cDevice.WriteByte((byte)register);
            return _i2cDevice.ReadByte();
        }

        internal enum Register : byte
        {
            TEMP_REG = 0x01,                 // Ext. Temperature [Unit: degC]
            LIGHT_REG_L = 0x02,              // Light Brightness Low 8 Bit [Unit: Lux]
            LIGHT_REG_H = 0x03,              // Light Brightness High 8 Bit [Unit: Lux]
            STATUS_REG = 0x04,               // Status Function
            ON_BOARD_TEMP_REG = 0x05,        // OnBoard Temperature [Unit: degC]
            ON_BOARD_HUMIDITY_REG = 0x06,    // OnBoard Humidity [Unit:%]
            ON_BOARD_SENSOR_ERROR = 0x07,    // 0(OK) - 1(Error)
            BMP280_TEMP_REG = 0x08,          // P.Temperature [Unit: degC]
            BMP280_PRESSURE_REG_L = 0x09,    // P. Pressure Low 8 Bit[Unit: Pa]
            BMP280_PRESSURE_REG_M = 0x0A,    // P. Pressure Mid 8 Bit[Unit: Pa]
            BMP280_PRESSURE_REG_H = 0x0B,    // P. Pressure High 8 Bit[Unit: Pa]
            BMP280_STATUS = 0x0C,            // 0(OK) - 1(Error)
            MOTION_DETECTED = 0x0D,          // 0(No Active Body) - 1(Active Body)
        }

        internal enum StatusFunctionError : byte
        {
            TEMP_OVERFLOW = 0x01,               // Ext. Temperature Overflow
            TEMP_NOT_FOUND = 0x02,              // Ext. Temperature Not Found
            LIGHT_BRIGHTNESS_OVERFLOW = 0x04,   // Light Brightness Overflow
            LIGHT_BRIGHTNESS_NOT_FOUND = 0x08,  // Light Brightness Not Found
        }
    }
}

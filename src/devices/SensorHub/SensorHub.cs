// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.SensorHub
{
    /// <summary>
    /// SensorHub
    /// </summary>
    public class SensorHub
    {
        /// <summary>
        /// Default I2C bus id
        /// </summary>
        public const int DefaultI2cBusId = 1;

        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int DefaultI2cAddress = 0x17;

        private readonly I2cDevice _i2cDevice;

        /// <summary>
        /// Creates a SensorHub with the default bus and connection values
        /// </summary>
        /// <returns>SensorHub</returns>
        public static SensorHub Create()
        {
            return new(I2cDevice.Create(new I2cConnectionSettings(DefaultI2cBusId, DefaultI2cAddress)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorHub"/> class.
        /// </summary>
        public SensorHub(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Try to read the temperature from the off board thermometer in the range -30 to 127 Celsius
        /// </summary>
        /// <param name="temperature">The temperature in Celsius if successful</param>
        /// <returns></returns>
        public bool TryReadOffBoardTemperature(out Temperature temperature)
        {
            temperature = Temperature.MaxValue;
            var status = ReadRegister(Register.STATUS_REG);
            if (!((status & 0x1) == 0x1) && !((status & 0x2) == 0x2))
            {
                temperature = Temperature.FromDegreesCelsius(ReadRegister(Register.TEMP_REG));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to read the temperature from the barometer sensor in the range -40 to 80 Celsius
        /// </summary>
        /// <param name="temperature">Temperature in Celsius if successful</param>
        /// <returns></returns>
        public bool TryReadBarometerTemperature(out Temperature temperature)
        {
            temperature = Temperature.MaxValue;
            var status = ReadRegister(Register.BMP280_STATUS);
            if (status == 0)
            {
                temperature = Temperature.FromDegreesCelsius(ReadRegister(Register.BMP280_TEMP_REG));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to read pressure in Pascals from sensor in the range 300 Pa to 1100 hPa
        /// </summary>
        /// <param name="pressure">pressure in Pascals if successful</param>
        /// <returns></returns>
        public bool TryReadBarometerPressure(out Pressure pressure)
        {
            pressure = Pressure.MaxValue;
            var status = ReadRegister(Register.BMP280_STATUS);
            if (status == 0)
            {
                pressure = Pressure.FromPascals(
                       ReadRegister(Register.BMP280_PRESSURE_REG_L)
                    | (ReadRegister(Register.BMP280_PRESSURE_REG_M) << 8)
                    | (ReadRegister(Register.BMP280_PRESSURE_REG_H) << 16));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to read illuminance in Lux from sensor in the range 0lx to 1800lx
        /// </summary>
        /// <param name="illuminance">Illuminance in Lux if successful</param>
        /// <returns></returns>
        public bool TryReadIlluminance(out Illuminance illuminance)
        {
            illuminance = Illuminance.MaxValue;
            var status = ReadRegister(Register.STATUS_REG);
            if (!((status & 0x04) == 1) && !((status & 0x08) == 1))
            {
                illuminance = new((ReadRegister(Register.LIGHT_REG_H) << 8 | ReadRegister(Register.LIGHT_REG_L)), UnitsNet.Units.IlluminanceUnit.Lux);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to read relative humidity from sensor in the range 20 to 95 percent
        /// </summary>
        /// <param name="humidity">Relative humidity if successful</param>
        /// <returns></returns>
        public bool TryReadRelativeHumidity(out int humidity)
        {
            humidity = int.MaxValue;
            var status = ReadRegister(Register.ON_BOARD_SENSOR_ERROR);
            if (status == 0)
            {
                humidity = ReadRegister(Register.ON_BOARD_HUMIDITY_REG);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to read temperature from on board sensor in Celsius in the range -20 to 60 Celsius
        /// </summary>
        /// <param name="temperature">Temperature in Celsius if successful</param>
        /// <returns></returns>
        public bool TryReadOnBoardTemperature(out Temperature temperature)
        {
            temperature = Temperature.MaxValue;
            var status = ReadRegister(Register.ON_BOARD_SENSOR_ERROR);
            if (status == 0)
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

        private byte ReadRegister(Register register)
        {
            _i2cDevice.WriteByte((byte)register);
            return _i2cDevice.ReadByte();
        }

        private enum Register : byte
        {
            TEMP_REG = 0x01,                 // Ext. Temperature [Unit: degC]
            LIGHT_REG_L = 0x02,              // Light Brightness Low 8 Bit [Unit: Lux]
            LIGHT_REG_H = 0x03,              // Light Brightness High 8 Bit [Unit: Lux]
            STATUS_REG = 0x04,               // Status Function
            ON_BOARD_TEMP_REG = 0x05,        // OnBoard Temperature [Unit: degC]
            ON_BOARD_HUMIDITY_REG = 0x06,    // OnBoard Humidity [Uinit:%]
            ON_BOARD_SENSOR_ERROR = 0x07,    // 0(OK) - 1(Error)
            BMP280_TEMP_REG = 0x08,          // P.Temperature [Unit: degC]
            BMP280_PRESSURE_REG_L = 0x09,    // P. Pressure Low 8 Bit[Unit: Pa]
            BMP280_PRESSURE_REG_M = 0x0A,    // P. Pressure Mid 8 Bit[Unit: Pa]
            BMP280_PRESSURE_REG_H = 0x0B,    // P. Pressure High 8 Bit[Unit: Pa]
            BMP280_STATUS = 0x0C,            // 0(OK) - 1(Error)
            MOTION_DETECTED = 0x0D,          // 0(No Active Body) - 1(Active Body)
        }
    }
}

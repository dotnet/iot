// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Ported from https://github.com/adafruit/Adafruit_Python_BMP/blob/master/Adafruit_BMP/BMP085.py
// Formulas and code examples can also be found in the datasheet https://cdn-shop.adafruit.com/datasheets/BST-BMP180-DS000-09.pdf
using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Bmp180
{
    /// <summary>
    /// BMP180 - barometer, altitude and temperature sensor
    /// </summary>
    public class Bmp180 : IDisposable
    {
        private readonly CalibrationData _calibrationData;
        private I2cDevice _i2cDevice;
        private Sampling _mode;

        /// <summary>
        /// Default I2C address
        /// </summary>
        public const byte DefaultI2cAddress = 0x77;

        /// <summary>
        /// Constructs Bmp180 instance
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        public Bmp180(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _calibrationData = new CalibrationData();
            // Read the coefficients table
            _calibrationData.ReadFromDevice(this);
            SetSampling(Sampling.Standard);
        }

        /// <summary>
        /// Sets sampling to the given value
        /// </summary>
        /// <param name="mode">Sampling Mode</param>
        public void SetSampling(Sampling mode) => _mode = mode;

        /// <summary>
        ///  Reads the temperature from the sensor
        /// </summary>
        /// <returns>
        ///  Temperature in degrees celsius
        /// </returns>
        public Temperature ReadTemperature() => Temperature.FromDegreesCelsius((CalculateTrueTemperature() + 8) / 160.0);

        /// <summary>
        ///  Reads the pressure from the sensor
        /// </summary>
        /// <returns>
        ///  Atmospheric pressure
        /// </returns>
        public Pressure ReadPressure()
        {
            // Pressure Calculations
            int b6 = CalculateTrueTemperature() - 4000;
            int b62 = (b6 * b6) / 4096;
            int x3 = (((short)_calibrationData.B2 * b62) + ((short)_calibrationData.AC2 * b6)) / 2048;
            int b3 = ((((short)_calibrationData.AC1 * 4 + x3) << (short)Sampling.Standard) + 2) / 4;
            int x1 = ((short)_calibrationData.AC3 * b6) / 8192;
            int x2 = ((short)_calibrationData.B1 * b62) / 65536;
            x3 = ((x1 + x2) + 2) / 4;
            int b4 = _calibrationData.AC4 * (x3 + 32768) / 32768;
            uint b7 = (uint)(ReadRawPressure() - b3) * (uint)(50000 >> (short)Sampling.Standard);
            int p = (b7 < 0x80000000) ? (int)((b7 * 2) / b4) : (int)((b7 / b4) * 2);
            x1 = (((p * p) / 65536) * 3038) / 65536;

            return Pressure.FromPascals(p + (((((p * p) / 65536) * 3038) / 65536) + ((-7357 * p) / 65536) + 3791) / 8);
        }

        /// <summary>
        ///  Calculates the altitude in meters from the specified sea-level pressure.
        /// </summary>
        /// <param name="seaLevelPressure">
        ///  Sea-level pressure
        /// </param>
        /// <returns>
        ///  Height above sea level
        /// </returns>
        public Length ReadAltitude(Pressure seaLevelPressure) => WeatherHelper.CalculateAltitude(ReadPressure(), seaLevelPressure, ReadTemperature());

        /// <summary>
        ///  Calculates the altitude in meters from the mean sea-level pressure.
        /// </summary>
        /// <returns>
        ///  Height in meters above sea level
        /// </returns>
        public Length ReadAltitude() => ReadAltitude(WeatherHelper.MeanSeaLevel);

        /// <summary>
        ///  Calculates the pressure at sea level when given a known altitude
        /// </summary>
        /// <param name="altitude" >
        ///  Altitude in meters
        /// </param>
        /// <returns>
        ///  Pressure
        /// </returns>
        public Pressure ReadSeaLevelPressure(Length altitude) => WeatherHelper.CalculateSeaLevelPressure(ReadPressure(), altitude, ReadTemperature());

        /// <summary>
        ///  Calculates the pressure at sea level, when the current altitude is 0.
        /// </summary>
        /// <returns>
        ///  Pressure
        /// </returns>
        public Pressure ReadSeaLevelPressure() => ReadSeaLevelPressure(Length.Zero);

        /// <summary>
        ///  Calculate true temperature
        /// </summary>
        /// <returns>
        ///  Coefficient B5
        /// </returns>
        private int CalculateTrueTemperature()
        {
            // Calculations below are taken straight from section 3.5 of the datasheet.
            int x1 = (ReadRawTemperature() - _calibrationData.AC6) * _calibrationData.AC5 / 32768;
            int x2 = _calibrationData.MC * (2048) / (x1 + _calibrationData.MD);

            return x1 + x2;
        }

        /// <summary>
        ///  Reads raw temperatue from the sensor
        /// </summary>
        /// <returns>
        ///  Raw temperature
        /// </returns>
        private short ReadRawTemperature()
        {
            // Reads the raw (uncompensated) temperature from the sensor
            Span<byte> command = stackalloc byte[]
            {
                (byte)Register.CONTROL, (byte)Register.READTEMPCMD
            };
            _i2cDevice.Write(command);
            // Wait 5ms, taken straight from section 3.3 of the datasheet.
            Thread.Sleep(5);

            return (short)Read16BitsFromRegisterBE((byte)Register.TEMPDATA);
        }

        /// <summary>
        ///  Reads raw pressure from the sensor
        ///  Taken from datasheet, Section 3.3.1
        ///  Standard            - 8ms
        ///  UltraLowPower       - 5ms
        ///  HighResolution      - 14ms
        ///  UltraHighResolution - 26ms
        /// </summary>
        /// <returns>
        ///  Raw pressure
        /// </returns>
        private int ReadRawPressure()
        {
            // Reads the raw (uncompensated) pressure level from the sensor.
            _i2cDevice.Write(new[]
            {
                (byte)Register.CONTROL, (byte)(Register.READPRESSURECMD + ((byte)Sampling.Standard << 6))
            });

            if (_mode.Equals(Sampling.UltraLowPower))
            {
                Thread.Sleep(5);
            }
            else if (_mode.Equals(Sampling.HighResolution))
            {
                Thread.Sleep(14);
            }
            else if (_mode.Equals(Sampling.UltraHighResolution))
            {
                Thread.Sleep(26);
            }
            else
            {
                Thread.Sleep(8);
            }

            int msb = Read8BitsFromRegister((byte)Register.PRESSUREDATA);
            int lsb = Read8BitsFromRegister((byte)Register.PRESSUREDATA + 1);
            int xlsb = Read8BitsFromRegister((byte)Register.PRESSUREDATA + 2);

            return ((msb << 16) + (lsb << 8) + xlsb) >> (8 - (byte)Sampling.Standard);
        }

        /// <summary>
        ///  Reads an 8 bit value from a register
        /// </summary>
        /// <param name="register">
        ///  Register to read from
        /// </param>
        /// <returns>
        ///  Value from register
        /// </returns>
        internal byte Read8BitsFromRegister(byte register)
        {
            _i2cDevice.WriteByte(register);
            byte value = _i2cDevice.ReadByte();

            return value;
        }

        /// <summary>
        ///  Reads a 16 bit value over I2C
        /// </summary>
        /// <param name="register">
        ///  Register to read from
        /// </param>
        /// <returns>
        ///  Value from register
        /// </returns>
        internal ushort Read16BitsFromRegister(byte register)
        {
            Span<byte> bytes = stackalloc byte[2];
            _i2cDevice.WriteByte(register);
            _i2cDevice.Read(bytes);

            return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
        }

        /// <summary>
        ///  Reads a 16 bit value over I2C
        /// </summary>
        /// <param name="register">
        ///  Register to read from
        /// </param>
        /// <returns>
        ///  Value (BigEndian) from register
        /// </returns>
        internal ushort Read16BitsFromRegisterBE(byte register)
        {
            Span<byte> bytes = stackalloc byte[2];
            _i2cDevice.WriteByte(register);
            _i2cDevice.Read(bytes);

            return BinaryPrimitives.ReadUInt16BigEndian(bytes);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//ported from https://github.com/adafruit/Adafruit_BMP280_Library/blob/master/Adafruit_BMP280.cpp

using System;
using System.Device.I2c;
using System.Threading.Tasks;

namespace Iot.Device
{
    public class Bmp280 : IDisposable
    {
        private I2cDevice _i2cDevice;
        private readonly CommunicationProtocol _communicationProtocol;
        private bool _initialized = false;
        private readonly Bmp280CalibrationData _calibrationData;
        private const byte Signature = 0x58;
        /// <summary>
        /// The variable _temperatureFine carries a fine resolution temperature value over to the
        /// pressure compensation formula and could be implemented as a global variable.
        /// </summary>
        private int _temperatureFine;

        private enum CommunicationProtocol
        {
            I2c
        }

        public Bmp280(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _calibrationData = new Bmp280CalibrationData();
            _communicationProtocol = CommunicationProtocol.I2c;
        }

        private void Begin()
        {
            _i2cDevice.WriteByte((byte)Registers.REGISTER_CHIPID);
            var readSignature = _i2cDevice.ReadByte();

            if (readSignature != Signature)
            {
                return;
            }
            _initialized = true;

            //Read the coefficients table
            ReadCoefficients();
        }

        /// <summary>
        ///  Reads the factory-set coefficients 
        /// </summary>
        private void ReadCoefficients()
        {
            // Read temperature calibration data
            _calibrationData.DigT1 = Read16((byte)Registers.REGISTER_DIG_T1);
            _calibrationData.DigT2 = (short)Read16((byte)Registers.REGISTER_DIG_T2);
            _calibrationData.DigT3 = (short)Read16((byte)Registers.REGISTER_DIG_T3);

            // Read pressure calibration data
            _calibrationData.DigP1 = Read16((byte)Registers.REGISTER_DIG_P1);
            _calibrationData.DigP2 = (short)Read16((byte)Registers.REGISTER_DIG_P2);
            _calibrationData.DigP3 = (short)Read16((byte)Registers.REGISTER_DIG_P3);
            _calibrationData.DigP4 = (short)Read16((byte)Registers.REGISTER_DIG_P4);
            _calibrationData.DigP5 = (short)Read16((byte)Registers.REGISTER_DIG_P5);
            _calibrationData.DigP6 = (short)Read16((byte)Registers.REGISTER_DIG_P6);
            _calibrationData.DigP7 = (short)Read16((byte)Registers.REGISTER_DIG_P7);
            _calibrationData.DigP8 = (short)Read16((byte)Registers.REGISTER_DIG_P8);
            _calibrationData.DigP9 = (short)Read16((byte)Registers.REGISTER_DIG_P9);

        }

        /// <summary>
        /// Set mode sleep (no measurements)
        /// </summary>
        public void SetModeSleep()
        {
            byte status = Read8((byte)Registers.REGISTER_CONTROL);
            //clear last two bits
            status = (byte)(status & 0b1111_1100);
            _i2cDevice.Write(new[] { (byte)Registers.REGISTER_CONTROL, status });
        }

        /// <summary>
        /// Sets mode to forced (device goes to sleep after one reading)
        /// </summary>
        public void SetModeForced()
        {
            byte status = Read8((byte)Registers.REGISTER_CONTROL);
            //clear last two bits
            status = (byte)(status & 0b1111_1100);
            //set one of the last two bits
            status = (byte)(status | 0b0000_0010);
            _i2cDevice.Write(new[] { (byte)Registers.REGISTER_CONTROL, status });
        }

        /// <summary>
        /// Sets mode to normal (continuous measurements)
        /// </summary>
        public void SetModeNormal()
        {
            byte status = Read8((byte)Registers.REGISTER_CONTROL);
            //set last two bits to 11
            status = (byte)(status | 0b0000_0011);
            _i2cDevice.Write(new[] { (byte)Registers.REGISTER_CONTROL, status });
        }

        public PowerMode ReadPowerMode()
        {
            byte status = Read8((byte)Registers.REGISTER_CONTROL);
            status = (byte)(status & 0b000_00011);
            if (status == (byte)PowerMode.Normal)
            {
                return PowerMode.Normal;
            }
            else if (status == (byte)PowerMode.Sleep)
            {
                return PowerMode.Sleep;
            }
            else
            {
                return PowerMode.Forced;
            }
        }

        /// <summary>
        /// Sets the temperature sampling to the given value
        /// </summary>
        /// <param name="sampling"></param>
        public void SetTemperatureSampling(Sampling sampling)
        {
            byte status = Read8((byte)Registers.REGISTER_CONTROL);
            status = (byte)(status & 0b0001_1111);
            status = (byte)(status | (byte)sampling << 5);
            _i2cDevice.Write(new[] { (byte)Registers.REGISTER_CONTROL, status });
        }

        /// <summary>
        /// Get the sample rate for temperature measurements
        /// </summary>
        /// <returns></returns>
        public Sampling ReadTemperatureSampling()
        {
            byte status = Read8((byte)Registers.REGISTER_CONTROL);
            status = (byte)((status & 0b1110_0000) >> 5);
            return ByteToSampling(status);
        }

        private Sampling ByteToSampling(byte value)
        {
            if (value == (byte)Sampling.Skipped)
            {
                return Sampling.Skipped;
            }
            else if (value == (byte)Sampling.UltraLowPower)
            {
                return Sampling.UltraLowPower;
            }
            else if (value == (byte)Sampling.LowPower)
            {
                return Sampling.LowPower;
            }
            else if (value == (byte)Sampling.Standard)
            {
                return Sampling.Standard;
            }
            else if (value == (byte)Sampling.HighResolution)
            {
                return Sampling.HighResolution;
            }
            else
            {
                return Sampling.UltraHighResolution;
            }
        }

        /// <summary>
        /// Get the current sample rate for pressure measurements
        /// </summary>
        /// <returns></returns>
        public Sampling ReadPressureSampling()
        {
            byte status = Read8((byte)Registers.REGISTER_CONTROL);
            status = (byte)((status & 0b0001_1100) >> 2);
            return ByteToSampling(status);
        }

        /// <summary>
        /// Sets the pressure sampling to the given value
        /// </summary>
        /// <param name="sampling"></param>
        public void SetPressureSampling(Sampling sampling)
        {
            byte status = Read8((byte)Registers.REGISTER_CONTROL);
            status = (byte)(status & 0b1110_0011);
            status = (byte)(status | (byte)sampling << 2);
            _i2cDevice.Write(new[] { (byte)Registers.REGISTER_CONTROL, status });
        }

        /// <summary>
        ///  Reads the temperature from the sensor
        /// </summary>
        /// <returns>
        ///  Temperature in degrees celsius
        /// </returns>
        public async Task<double> ReadTemperatureAsync()
        {
            //Make sure the I2C device is initialized
            if (!_initialized)
            {
                Begin();
            }

            if (ReadPowerMode() == PowerMode.Forced)
            {
                await Task.Delay(GetMeasurementTimeForForcedMode(ReadTemperatureSampling()));
            }

            //Read the MSB, LSB and bits 7:4 (XLSB) of the temperature from the BMP280 registers
            byte tmsb = Read8((byte)Registers.REGISTER_TEMPDATA_MSB);
            byte tlsb = Read8((byte)Registers.REGISTER_TEMPDATA_LSB);
            byte txlsb = Read8((byte)Registers.REGISTER_TEMPDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            int t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);

            //Convert the raw value to the temperature in degC
            double temp = BMP280_compensate_T_double(t);

            //Return the temperature as a float value
            return temp;
        }

        /// <summary>
        /// Recommended sleep timings from the datasheet
        /// </summary>
        /// <param name="sampleMode"></param>
        /// <returns></returns>
        private int GetMeasurementTimeForForcedMode(Sampling sampleMode)
        {
            if (sampleMode == Sampling.UltraLowPower)
            {
                return 7;
            }
            else if (sampleMode == Sampling.LowPower)
            {
                return 9;
            }
            else if (sampleMode == Sampling.Standard)
            {
                return 14;
            }
            else if (sampleMode == Sampling.HighResolution)
            {
                return 23;
            }
            else if (sampleMode == Sampling.UltraHighResolution)
            {
                return 44;
            }
            return 0;
        }

        /// <summary>
        ///  Reads the pressure from the sensor
        /// </summary>
        /// <returns>
        ///  Atmospheric pressure in Pa
        /// </returns>
        public async Task<double> ReadPressureAsync()
        {
            //Make sure the I2C device is initialized
            if (!_initialized)
            {
                Begin();
            }

            if (ReadPowerMode() == PowerMode.Forced)
            {
                await Task.Delay(GetMeasurementTimeForForcedMode(ReadPressureSampling()));
            }

            //Read the temperature first to load the t_fine value for compensation
            if (_temperatureFine == int.MinValue)
            {
                await ReadTemperatureAsync();
            }

            //Read the MSB, LSB and bits 7:4 (XLSB) of the pressure from the BMP280 registers
            byte tmsb = Read8((byte)Registers.REGISTER_PRESSUREDATA_MSB);
            byte tlsb = Read8((byte)Registers.REGISTER_PRESSUREDATA_LSB);
            byte txlsb = Read8((byte)Registers.REGISTER_PRESSUREDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            int t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);

            //Convert the raw value to the pressure in Pa
            long pres = BMP280_compensate_P_Int64(t);

            //Return the temperature as a float value
            return (pres) / 256;
        }

        /// <summary>
        ///  Calculates the altitude in meters from the specified sea-level pressure(in hPa).
        /// </summary>
        /// <param name="seaLevelPressure" > 
        ///  Sea-level pressure in hPa
        /// </param>
        /// <returns>
        ///  Height in meters from the sensor
        /// </returns>
        public async Task<double> ReadAltitudeAsync(double seaLevelPressure)
        {
            //Make sure the I2C device is initialized
            if (!_initialized)
            {
                Begin();
            }

            //Read the pressure first
            var pressure = await ReadPressureAsync();
            //Convert the pressure to Hectopascals(hPa)
            pressure /= 100;

            //Calculate and return the altitude using the international barometric formula
            return 44330.0 * (1.0 - Math.Pow((pressure / seaLevelPressure), 0.1903));
        }

        /// <summary>
        ///  Returns the temperature in DegC. Resolution is 0.01 DegC. Output value of “5123” equals 51.23 DegC.
        /// </summary>
        /// <param name="adcT"></param>
        /// <returns>
        ///  Degrees celsius
        /// </returns>
        private double BMP280_compensate_T_double(int adcT)
        {
            //The temperature is calculated using the compensation formula in the BMP280 datasheet
            var var1 = ((adcT / 16384.0) - (_calibrationData.DigT1 / 1024.0)) * _calibrationData.DigT2;
            var var2 = ((adcT / 131072.0) - (_calibrationData.DigT1 / 8192.0)) * _calibrationData.DigT3;

            _temperatureFine = (int)(var1 + var2);

            var T = (var1 + var2) / 5120.0;
            return T;
        }

        /// <summary>
        ///  Returns the pressure in Pa, in Q24.8 format (24 integer bits and 8 fractional bits).
        ///  Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa
        /// </summary>
        /// <param name="adcP"></param>
        /// <returns>
        ///  Pressure in hPa
        /// </returns>
        private long BMP280_compensate_P_Int64(int adcP)
        {
            //The pressure is calculated using the compensation formula in the BMP280 datasheet
            long var1 = _temperatureFine - 128000;
            var var2 = var1 * var1 * (long)_calibrationData.DigP6;
            var2 = var2 + ((var1 * (long)_calibrationData.DigP5) << 17);
            var2 = var2 + ((long)_calibrationData.DigP4 << 35);
            var1 = ((var1 * var1 * (long)_calibrationData.DigP3) >> 8) + ((var1 * (long)_calibrationData.DigP2) << 12);
            var1 = (((((long)1 << 47) + var1)) * (long)_calibrationData.DigP1) >> 33;
            if (var1 == 0)
            {
                return 0; //Avoid exception caused by division by zero
            }
            //Perform calibration operations as per datasheet: http://www.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf
            long p = 1048576 - adcP;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = ((long)_calibrationData.DigP9 * (p >> 13) * (p >> 13)) >> 25;
            var2 = ((long)_calibrationData.DigP8 * p) >> 19;
            p = ((p + var1 + var2) >> 8) + ((long)_calibrationData.DigP7 << 4);
            return p;
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
        public byte Read8(byte register)
        {
            if (_communicationProtocol == CommunicationProtocol.I2c)
            {
                _i2cDevice.WriteByte(register);
                var value = _i2cDevice.ReadByte();
                return value;
            }
            else
            {
                throw new NotImplementedException();
            }
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
        public ushort Read16(byte register)
        {
            if (_communicationProtocol == CommunicationProtocol.I2c)
            {
                byte[] readBuffer = { 0x00, 0x00 };

                _i2cDevice.WriteByte(register);
                _i2cDevice.Read(readBuffer);
                int h = readBuffer[1] << 8;
                int l = readBuffer[0];
                ushort value = (ushort)(h + l);

                return value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        ///  Reads a 24 bit value over I2C 
        /// </summary>
        /// <param name="register">
        ///  Register to read from
        /// </param>
        /// <returns>
        ///  Value from register
        /// </returns>
        public uint Read24(byte register)
        {
            if (_communicationProtocol == CommunicationProtocol.I2c)
            {
                byte[] readBuffer = { 0x00, 0x00, 0x00 };

                _i2cDevice.WriteByte(register);
                _i2cDevice.Read(readBuffer);
                uint value = readBuffer[2];
                value <<= 8;
                value = readBuffer[1];
                value <<= 8;
                value = readBuffer[0];

                return value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice.Dispose();
                _i2cDevice = null;
            }
        }

        /// <summary>
        /// BMP280s power modes
        /// </summary>
        public enum PowerMode : byte
        {
            /// <summary>
            /// Power saving mode, does not do new measurements
            /// </summary>
            Sleep = 0b00,
            /// <summary>
            /// Device goes to sleep mode after one measurement
            /// </summary>
            Forced = 0b10,
            /// <summary>
            /// Device does continuous measurements
            /// </summary>
            Normal = 0b11
        }

        /// <summary>
        /// Oversampling settings. Maximum of x2 is recommended for temperature
        /// </summary>
        public enum Sampling : byte
        {
            /// <summary>
            /// Skipped (output set to 0x80000)
            /// </summary>
            Skipped = 0b000,
            /// <summary>
            /// oversampling x1
            /// </summary>
            UltraLowPower = 0b001,
            /// <summary>
            /// oversampling x2
            /// </summary>
            LowPower = 0b010,
            /// <summary>
            /// oversampling x4
            /// </summary>
            Standard = 0b011,
            /// <summary>
            /// oversampling x8
            /// </summary>
            HighResolution = 0b100,
            /// <summary>
            /// oversampling x16
            /// </summary>
            UltraHighResolution = 0b101,
        }

        /// <summary>
        ///  Registers
        /// </summary>
        public enum Registers : byte
        {
            REGISTER_DIG_T1 = 0x88,
            REGISTER_DIG_T2 = 0x8A,
            REGISTER_DIG_T3 = 0x8C,

            REGISTER_DIG_P1 = 0x8E,
            REGISTER_DIG_P2 = 0x90,
            REGISTER_DIG_P3 = 0x92,
            REGISTER_DIG_P4 = 0x94,
            REGISTER_DIG_P5 = 0x96,
            REGISTER_DIG_P6 = 0x98,
            REGISTER_DIG_P7 = 0x9A,
            REGISTER_DIG_P8 = 0x9C,
            REGISTER_DIG_P9 = 0x9E,

            REGISTER_CHIPID = 0xD0,
            REGISTER_VERSION = 0xD1,
            REGISTER_SOFTRESET = 0xE0,

            REGISTER_CAL26 = 0xE1,  // R calibration stored in 0xE1-0xF0

            REGISTER_STATUS = 0xF3,
            REGISTER_CONTROL = 0xF4,
            REGISTER_CONFIG = 0xF5,

            REGISTER_PRESSUREDATA_MSB = 0xF7,
            REGISTER_PRESSUREDATA_LSB = 0xF8,
            REGISTER_PRESSUREDATA_XLSB = 0xF9, // bits <7:4>

            REGISTER_TEMPDATA_MSB = 0xFA,
            REGISTER_TEMPDATA_LSB = 0xFB,
            REGISTER_TEMPDATA_XLSB = 0xFC, // bits <7:4>=
        };
    }
}

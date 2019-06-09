// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Ported from https://github.com/adafruit/Adafruit_Python_BMP/blob/master/Adafruit_BMP/BMP085.py  
// Formulas and code examples can also be found in the datasheet https://cdn-shop.adafruit.com/datasheets/BST-BMP180-DS000-09.pdf

using System;
using System.Device.I2c;
using System.Buffers.Binary;
using System.Threading;
using Iot.Units;

namespace Iot.Device.Bmp180
{
    public class Bmp180 : IDisposable
    {
        private I2cDevice _i2cDevice;        
        private readonly CalibrationData _calibrationData;
        private Sampling _mode;
        public const byte DefaultI2cAddress = 0x77;

        public Bmp180(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;            
            _calibrationData = new CalibrationData();            
            //Read the coefficients table
            _calibrationData.ReadFromDevice(this);
            SetSampling(Sampling.Standard);
        }
        
        /// <summary>
        /// Sets sampling to the given value
        /// </summary>
        /// <param name="mode">Sampling Mode</param>
        public void SetSampling(Sampling mode)
        {
            _mode = mode;
        }

        /// <summary>
        ///  Reads the temperature from the sensor
        /// </summary>
        /// <returns>
        ///  Temperature in degrees celsius
        /// </returns>
        public Temperature ReadTemperature()
        {
            return Temperature.FromCelsius((CalculateTrueTemperature() + 8) / 160.0);              
        }

        /// <summary>
        ///  Reads the pressure from the sensor
        /// </summary>
        /// <returns>
        ///  Atmospheric pressure in Pa
        /// </returns>
        public double ReadPressure()
        {
            // Pressure Calculations
            int B6 = CalculateTrueTemperature() - 4000;
            int B62 = (B6 * B6) / 4096;
            int X3 =  (((short)_calibrationData.B2 * B62) + ((short)_calibrationData.AC2 * B6)) / 2048;
            int B3 = ((((short)_calibrationData.AC1 * 4 + X3) << (short)Sampling.Standard) + 2) / 4;
            int X1 = ((short)_calibrationData.AC3 * B6 ) / 8192;
            int X2 = ((short)_calibrationData.B1 * B62) / 65536;
            X3 = ((X1 + X2) + 2) / 4;
            int B4 = _calibrationData.AC4 * (X3 + 32768) / 32768;
            uint B7 = (uint)(ReadRawPressure() - B3) * (uint)(50000 >> (short)Sampling.Standard);
            int p = (B7 < 0x80000000) ? (int)((B7 * 2) / B4) : (int)((B7 / B4) * 2);
            X1 = (((p * p) / 65536 ) * 3038) / 65536;
            
            return p + ( ((((p * p) / 65536 ) * 3038) / 65536) + ((-7357 * p) / 65536) + 3791) / 8;
        }

        /// <summary>
        ///  Calculates the altitude in meters from the specified sea-level pressure(in hPa).
        /// </summary>
        /// <param name="seaLevelPressure"> 
        ///  Sea-level pressure in hPa
        /// </param>
        /// <returns>
        ///  Height in meters from the sensor
        /// </returns>
        public double ReadAltitude(double seaLevelPressure = 101325.0)
        {
            return 44330.0 * (1.0 - Math.Pow(((double)ReadPressure() / seaLevelPressure), (1.0 / 5.255)));
        }

        /// <summary>
        ///  Calculates the pressure at sealevel when given a known altitude in meter
        /// </summary>
        /// <param name="altitude" > 
        ///  altitude in meters
        /// </param>
        /// <returns>
        ///  Pressure in Pascals
        /// </returns>
        public double ReadSeaLevelPressure(double altitude = 0.0)
        {
            return (double)ReadPressure() / Math.Pow((1.0 - (altitude / 44333.0)), 5.255);
        }

        /// <summary>
        ///  Calculate true temperature
        /// </summary>
        /// <returns>
        ///  Coefficient B5
        /// </returns>
        private int CalculateTrueTemperature()
        {
            // Calculations below are taken straight from section 3.5 of the datasheet.
            int X1 = (ReadRawTemperature() - _calibrationData.AC6) * _calibrationData.AC5 / 32768;
            int X2 = _calibrationData.MC * (2048) / (X1 + _calibrationData.MD);
            
            return X1 + X2;
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
            Span<byte> command = stackalloc byte[]  { (byte)Register.CONTROL, (byte)Register.READTEMPCMD };  
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
            _i2cDevice.Write(new[] { (byte)Register.CONTROL, (byte)(Register.READPRESSURECMD + ((byte)Sampling.Standard << 6))});

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
            
            return  ((msb << 16) + (lsb << 8) + xlsb) >> (8 - (byte)Sampling.Standard);            
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
        
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}

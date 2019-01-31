// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//Ported from https://github.com/adafruit/Adafruit_Python_BMP/blob/master/Adafruit_BMP/BMP085.py  
//Formulas and code examples can also be found in the datasheet https://cdn-shop.adafruit.com/datasheets/BST-BMP180-DS000-09.pdf

using System;
using System.Device.I2c;
using System.Buffers.Binary;
using System.Threading;

namespace Iot.Device.Bmp180
{
    public class Bmp180:IDisposable
    {
        private I2cDevice _i2cDevice;
        private readonly CommunicationProtocol _communicationProtocol;
        private bool _initialized = false;
        private readonly CalibrationData _calibrationData;
        private Sampling _mode = Sampling.Standard;

        private enum CommunicationProtocol
        {
            I2c
        }

        public Bmp180(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;            
            _calibrationData = new CalibrationData();
            _communicationProtocol = CommunicationProtocol.I2c;
        }

        private void Begin()
        {
            _initialized = true;

            //Read the coefficients table
            _calibrationData.ReadFromDevice(this);
        }

        /// <summary>
        /// Sets sampling to the given value
        /// </summary>
        /// <param name="sampling"></param>
        public void SetSampling(Sampling mode)
        {
            this._mode = mode;
        }

        /// <summary>
        ///  Reads the temperature from the sensor
        /// </summary>
        /// <returns>
        ///  Temperature in degrees celsius
        /// </returns>
        public double ReadTemperature()
        {
            // Make sure the I2C device is initialized
            if (!_initialized)
            {
                Begin();
            }
            //Gets the compensated temperature in degrees celsius
            var UT = ReadRawTemperature();

            // Calculations below are taken straight from section 3.5 of the datasheet.
            var X1 = (UT - _calibrationData.AC6) * _calibrationData.AC5 / (1 << 15);
            var X2 = _calibrationData.MC * (1 << 11) / (X1 + _calibrationData.MD);
            var B5 = (X1 + X2);
            var T = (B5 + 8) / (1 << 4);
            
            return (double)T/10;
        }

        /// <summary>
        ///  Reads the pressure from the sensor
        /// </summary>
        /// <returns>
        ///  Atmospheric pressure in Pa
        /// </returns>
        public int ReadPressure()
        {
            //Make sure the I2C device is initialized
            if (!_initialized)
            {
                Begin();
            }

            //Gets the compensated pressure in Pascals
            var UT = ReadRawTemperature();
            var UP = ReadRawPressure();

            // Calculations below are taken straight from section 3.5 of the datasheet.
            // Calculate true temperature coefficient B5.
            var X1 = (UT - (short)_calibrationData.AC6) * (short)_calibrationData.AC5 / (1 << 15);
            var X2 = (short)_calibrationData.MC * (1 << 11) / (X1 + (short)_calibrationData.MD);
            var B5 = (X1 + X2);

            // Pressure Calculations
            var B6 = B5 - 4000;
            X1 = ((short)_calibrationData.B2 * (B6 * B6 / (1 << 12))) / (1 << 11);
            X2 = (short)_calibrationData.AC2 * B6 / (1 << 11);
            var X3 = X1 + X2;
            var B3 = ((((short)_calibrationData.AC1 * 4 + X3) << (short)Sampling.Standard) + 2) / 4;
            X1 = (short)_calibrationData.AC3 * B6 / (1 << 13);
            X2 = ((short)_calibrationData.B1 * (B6 * B6 / (1 << 12))) / (1 << 16);
            X3 = ((X1 + X2) + 2) / (1 << 2);
            var B4 = _calibrationData.AC4 * (X3 + 32768) / (1 << 15);
            var B7 = (UP - B3) * (50000 >> (short)Sampling.Standard);

            var p = 0;
            if((uint)B7 < 0x80000000)
            {                
                p = (int)(((long)B7 * 2) / B4);                
            }
            else
            {               
                p = (int)(((long)B7 / B4) * 2);             
            }
            
            X1 = (p / (1 << 8)) * (p / (1 << 8));
            X1 = (X1 * 3038) / (1 << 16);
            X2 = (-7357 * p) / (1 << 16);
            p = p + (X1 + X2 + 3791) / (1 << 4);

            return p;
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
        public double ReadAltitude(double sealevel_pa = 101325.0)
        {
            var pressure = (double)ReadPressure();
            var altitude = 44330.0 * (1.0 - Math.Pow((pressure / sealevel_pa), (1.0 / 5.255)));


            return altitude;
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
            var pressure = (double)ReadPressure();
            var p0 = pressure / Math.Pow((1.0 - (altitude / 44333.0)), 5.255);

            return p0;
        }

        /// <summary>
        ///  Reads raw temperatue from the sensor
        /// </summary>
        /// <returns>
        ///  Raw temperature
        /// </returns>
        short ReadRawTemperature()
        {
            // Reads the raw (uncompensated) temperature from the sensor
            _i2cDevice.Write(new[] { (byte)Register.CONTROL, (byte)Register.READTEMPCMD });            
            // Wait 5ms
            Thread.Sleep(5);
            var raw = (short)Read16BitsFromRegisterBE((byte)Register.TEMPDATA);
            
            return raw;
        }


        /// <summary>
        ///  Reads raw pressure from the sensor
        /// </summary>
        /// <returns>
        ///  Raw pressure
        /// </returns>
        int ReadRawPressure()
        {
            // Reads the raw (uncompensated) pressure level from the sensor.
            _i2cDevice.Write(new[] { (byte)Register.CONTROL, (byte)(Register.READPRESSURECMD + ((byte)Sampling.Standard << 6))});

            if (this._mode.Equals(Sampling.UltraLowPower))
            {
                Thread.Sleep(5);
            }else if (this._mode.Equals(Sampling.HighResolution))
            {
                Thread.Sleep(14);
            }
            else if (this._mode.Equals(Sampling.UltraHighResolution))
            {
                Thread.Sleep(26);
            }
            else
            {
                Thread.Sleep(8);
            }

            var msb = Read8BitsFromRegister((byte)Register.PRESSUREDATA);            
            var lsb = Read8BitsFromRegister((byte)Register.PRESSUREDATA + 1);            
            var xlsb = Read8BitsFromRegister((byte)Register.PRESSUREDATA + 2);

            var raw = ((msb << 16) + (lsb << 8) + xlsb) >> (8 - (byte)Sampling.Standard);
            
            return raw;            
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
            if (_communicationProtocol == CommunicationProtocol.I2c)
            {                
                _i2cDevice.WriteByte(register);
                byte value = _i2cDevice.ReadByte();
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
        internal ushort Read16BitsFromRegister(byte register)
        {
            if (_communicationProtocol == CommunicationProtocol.I2c)
            {
                Span<byte> bytes = stackalloc byte[2];

                _i2cDevice.WriteByte(register);
                _i2cDevice.Read(bytes);

                return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
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
        ///  Value (BigEndian) from register 
        /// </returns>
        internal ushort Read16BitsFromRegisterBE(byte register)
        {
            if (_communicationProtocol == CommunicationProtocol.I2c)
            {
                Span<byte> bytes = stackalloc byte[2];

                _i2cDevice.WriteByte(register);
                _i2cDevice.Read(bytes);

                return BinaryPrimitives.ReadUInt16BigEndian(bytes);
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
    }
}

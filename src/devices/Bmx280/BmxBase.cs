// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//Ported from https://github.com/adafruit/Adafruit_BMP280_Library/blob/master/Adafruit_BMP280.cpp
//Formulas and code examples can also be found in the datasheet http://www.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading.Tasks;
using Iot.Units;

namespace Iot.Device.Bmx280
{
    public abstract class BmxBase : IDisposable
    {
        internal I2cDevice _i2cDevice;
        internal byte _deviceId;
        internal bool _initialized = false;
        internal CommunicationProtocol _communicationProtocol;

        internal CalibrationData CalibrationData { get; private set; }

        /// <summary>
        /// The variable _temperatureFine carries a fine resolution temperature value over to the
        /// pressure compensation formula and could be implemented as a global variable.
        /// </summary>
        protected int TemperatureFine;

        internal enum CommunicationProtocol
        {
            I2c
        }

        internal abstract CalibrationData ReadCalibrationData();

        internal void Begin()
        {
            _i2cDevice.WriteByte((byte)Register.CHIPID);
            byte readSignature = _i2cDevice.ReadByte();

            if (readSignature != _deviceId)
            {
                throw new Exception($"Device ID {readSignature} is not the same as expected {_deviceId}. Please check you are using the right device.");
            }
            _initialized = true;

            //Read the coefficients table
            CalibrationData = ReadCalibrationData();
        }

        /// <summary>
        /// Sets the power mode to the given mode
        /// </summary>
        /// <param name="powerMode"></param>
        public void SetPowerMode(PowerMode powerMode)
        {
            byte status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
            //clear last two bits
            status = (byte)(status & 0b1111_1100);
            status = (byte)(status | (byte)powerMode);
            _i2cDevice.Write(new[] { (byte)Register.CTRL_MEAS, status });
        }

        /// <summary>
        /// Reads the current power mode the device is running in
        /// </summary>
        /// <returns></returns>
        public PowerMode ReadPowerMode()
        {
            byte status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
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
            byte status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
            status = (byte)(status & 0b0001_1111);
            status = (byte)(status | (byte)sampling << 5);
            _i2cDevice.Write(new[] { (byte)Register.CTRL_MEAS, status });
        }

        /// <summary>
        /// Get the sample rate for temperature measurements
        /// </summary>
        /// <returns></returns>
        public Sampling ReadTemperatureSampling()
        {
            byte status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
            status = (byte)((status & 0b1110_0000) >> 5);
            return ByteToSampling(status);
        }

        internal Sampling ByteToSampling(byte value)
        {
            //Values >=5 equals UltraHighResolution (others)
            if (value >= 5)
            {
                return Sampling.UltraHighResolution;
            }
            return (Sampling)value;
        }

        /// <summary>
        /// Get the current sample rate for pressure measurements
        /// </summary>
        /// <returns></returns>
        public Sampling ReadPressureSampling()
        {
            byte status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
            status = (byte)((status & 0b0001_1100) >> 2);
            return ByteToSampling(status);
        }

        /// <summary>
        /// Sets the pressure sampling to the given value
        /// </summary>
        /// <param name="sampling"></param>
        public void SetPressureSampling(Sampling sampling)
        {
            byte status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
            status = (byte)(status & 0b1110_0011);
            status = (byte)(status | (byte)sampling << 2);
            _i2cDevice.Write(new[] { (byte)Register.CTRL_MEAS, status });
        }

        /// <summary>
        ///  Reads the temperature from the sensor
        /// </summary>
        /// <returns>
        ///  Temperature
        /// </returns>
        public async Task<Temperature> ReadTemperatureAsync()
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
            byte msb = Read8BitsFromRegister((byte)Register.TEMPDATA_MSB);
            byte lsb = Read8BitsFromRegister((byte)Register.TEMPDATA_LSB);
            byte xlsb = Read8BitsFromRegister((byte)Register.TEMPDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            int t = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            return CompensateTemperature(t);
        }

        /// <summary>
        /// Recommended wait timings from the datasheet
        /// </summary>
        /// <param name="sampleMode">
        /// </param>
        /// <returns>
        /// The time it takes for the chip to read data in milliseconds rounded up
        /// </returns>
        internal int GetMeasurementTimeForForcedMode(Sampling sampleMode)
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
            if (TemperatureFine == int.MinValue)
            {
                await ReadTemperatureAsync();
            }

            //Read the MSB, LSB and bits 7:4 (XLSB) of the pressure from the BMP280 registers
            byte msb = Read8BitsFromRegister((byte)Register.PRESSUREDATA_MSB);
            byte lsb = Read8BitsFromRegister((byte)Register.PRESSUREDATA_LSB);
            byte xlsb = Read8BitsFromRegister((byte)Register.PRESSUREDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            int t = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            //Convert the raw value to the pressure in Pa
            long pres = CompensatePressure(t);

            //Return the temperature as a float value
            return (double)pres / 256;
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
            double pressure = await ReadPressureAsync();
            //Convert the pressure to hecto pascals (hPa)
            pressure /= 100;

            //Calculate and return the altitude using the international barometric formula
            return 44330.0 * (1.0 - Math.Pow((pressure / seaLevelPressure), 0.1903));
        }

        /// <summary>
        ///  Returns the temperature. Resolution is 0.01 DegC. Output value of “5123” equals 51.23 degrees celsius.
        /// </summary>
        /// <param name="adcTemperature">
        /// The temperature value read from the device
        /// </param>
        /// <returns>
        ///  Temperature
        /// </returns>
        private Temperature CompensateTemperature(int adcTemperature)
        {
            //Formula from the datasheet
            //The temperature is calculated using the compensation formula in the BMP280 datasheet
            double var1 = ((adcTemperature / 16384.0) - (CalibrationData.DigT1 / 1024.0)) * CalibrationData.DigT2;
            double var2 = ((adcTemperature / 131072.0) - (CalibrationData.DigT1 / 8192.0));
            var2 *= var2 * CalibrationData.DigT3;

            TemperatureFine = (int)(var1 + var2);

            double temp = (var1 + var2) / 5120.0;
            return Temperature.FromCelsius(temp);
        }

        /// <summary>
        ///  Returns the pressure in Pa, in Q24.8 format (24 integer bits and 8 fractional bits).
        ///  Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa
        /// </summary>
        /// <param name="adcPressure">
        /// The pressure value read from the device
        /// </param>
        /// <returns>
        ///  Pressure in hPa
        /// </returns>
        private long CompensatePressure(int adcPressure)
        {
            //Formula from the datasheet
            //The pressure is calculated using the compensation formula in the BMP280 datasheet
            long var1 = TemperatureFine - 128000;
            long var2 = var1 * var1 * (long)CalibrationData.DigP6;
            var2 = var2 + ((var1 * (long)CalibrationData.DigP5) << 17);
            var2 = var2 + ((long)CalibrationData.DigP4 << 35);
            var1 = ((var1 * var1 * (long)CalibrationData.DigP3) >> 8) + ((var1 * (long)CalibrationData.DigP2) << 12);
            var1 = (((((long)1 << 47) + var1)) * (long)CalibrationData.DigP1) >> 33;
            if (var1 == 0)
            {
                return 0; //Avoid exception caused by division by zero
            }
            //Perform calibration operations
            long p = 1048576 - adcPressure;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = ((long)CalibrationData.DigP9 * (p >> 13) * (p >> 13)) >> 25;
            var2 = ((long)CalibrationData.DigP8 * p) >> 19;
            p = ((p + var1 + var2) >> 8) + ((long)CalibrationData.DigP7 << 4);
            return p;
        }

        /// <summary>
        /// When called, the device is reset using the complete power-on-reset procedure.
        /// </summary>
        public void Reset()
        {
            const byte resetCommand = 0xb6;
            _i2cDevice.Write(new[] { (byte)Register.RESET, resetCommand });
        }

        /// <summary>
        /// Get the current status of the device.
        /// </summary>
        /// <returns></returns>
        public DeviceStatus ReadStatus()
        {
            var status = Read8BitsFromRegister((byte)Register.STATUS);

            // Bit 3
            var measuring = ((status >> 3) & 1) == 1;
            // Bit 0
            var imUpdate = (status & 1) == 1;

            return new DeviceStatus
            {
                ImageUpdating = imUpdate,
                Measuring = measuring
            };
        }

        /// <summary>
        /// Sets the IIR filter mode.
        /// </summary>
        /// <param name="filteringMode"></param>
        public void SetFilterMode(FilteringMode filteringMode)
        {
            byte current = Read8BitsFromRegister((byte)Register.CONFIG);
            current = (byte)((current & 0b1110_0011) | (byte)filteringMode << 2);
            _i2cDevice.Write(new[] { (byte)Register.CONFIG, current });
        }

        /// <summary>
        /// Reads the current IIR filter mode the device is running in.
        /// </summary>
        /// <returns></returns>
        public FilteringMode ReadFilterMode()
        {
            byte current = Read8BitsFromRegister((byte)Register.CONFIG);
            var mode = (byte)((current & 0b0001_1100) >> 2);
            switch (mode)
            {
                case 0b000:
                    return FilteringMode.Off;
                case 0b001:
                    return FilteringMode.X2;
                case 0b010:
                    return FilteringMode.X4;
                case 0b011:
                    return FilteringMode.X8;
                default:
                    return FilteringMode.X16;
            }
        }

        /// <summary>
        /// Sets the standby time mode the device will used when operating in normal mode.
        /// </summary>
        /// <param name="standbyTime"></param>
        public void SetStandbyTime(StandbyTime standbyTime)
        {
            byte current = Read8BitsFromRegister((byte)Register.CONFIG);
            current = (byte)((current & 0b0001_1111) | (byte)standbyTime << 5);
            _i2cDevice.Write(new[] { (byte)Register.CONFIG, current });
        }

        /// <summary>
        /// Reads the currently configured standby time mode the device will used when operating in normal mode.
        /// </summary>
        /// <returns></returns>
        public StandbyTime ReadStandbyTime()
        {
            byte current = Read8BitsFromRegister((byte)Register.CONFIG);
            var time = (byte)((current & 0b1110_0000) >> 5);
            switch (time)
            {
                case 0b000:
                    return StandbyTime.Ms0_5;
                case 0b001:
                    return StandbyTime.Ms62_5;
                case 0b010:
                    return StandbyTime.Ms125;
                case 0b011:
                    return StandbyTime.Ms250;
                case 0b100:
                    return StandbyTime.Ms500;
                case 0b101:
                    return StandbyTime.Ms1000;
                case 0b110:
                    return StandbyTime.Ms10;
                case 0b111:
                    return StandbyTime.Ms20;
                default:
                    throw new NotImplementedException($"Value read from registers {time:x2} was not defined by specifications.");
            }
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
        ///  Reads a 24 bit value over I2C
        /// </summary>
        /// <param name="register">
        ///  Register to read from
        /// </param>
        /// <returns>
        ///  Value from register
        /// </returns>
        internal uint Read24BitsFromRegister(byte register)
        {
            if (_communicationProtocol == CommunicationProtocol.I2c)
            {
                Span<byte> bytes = stackalloc byte[4];

                _i2cDevice.WriteByte(register);
                _i2cDevice.Read(bytes.Slice(1));

                return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
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
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }
    }
}

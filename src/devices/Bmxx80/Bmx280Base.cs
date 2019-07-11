// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//Ported from https://github.com/adafruit/Adafruit_BMP280_Library/blob/master/Adafruit_BMP280.cpp
//Formulas and code examples can also be found in the datasheet http://www.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf

using System;
using System.Device.I2c;
using System.IO;
using System.Threading.Tasks;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Bmxx80.Register;
using Iot.Units;

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Represents the core functionality of the Bmx280 family.
    /// </summary>
    public abstract class Bmx280Base : Bmxx80Base
    {
        /// <summary>
        /// Default I2C bus address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x77;

        /// <summary>
        /// Secondary I2C bus address.
        /// </summary>
        public const byte SecondaryI2cAddress = 0x76;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmx280Base"/> class.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        protected Bmx280Base(byte deviceId, I2cDevice i2cDevice)
            : base(deviceId, i2cDevice)
        {
            _controlRegister = (byte)Bmx280Register.CTRL_MEAS;
        }

        /// <summary>
        /// Reads the current IIR filter mode the device is running in.
        /// </summary>
        /// <returns>The current <see cref="FilteringMode"/>.</returns>
        public FilteringMode ReadFilterMode()
        {
            byte current = Read8BitsFromRegister((byte)Bmx280Register.CONFIG);
            var mode = (byte)((current & 0b_0001_1100) >> 2);

            return mode switch
            {
                0b000 => FilteringMode.Off,
                0b001 => FilteringMode.X2,
                0b010 => FilteringMode.X4,
                0b011 => FilteringMode.X8,
                _ => FilteringMode.X16
            };
        }

        /// <summary>
        /// Read the temperature.
        /// </summary>
        /// <returns>Calculated temperature.</returns>
        public async Task<Temperature> ReadTemperatureAsync()
        {
            if (ReadPowerMode() == Bmx280PowerMode.Forced)
            {
                await Task.Delay(GetMeasurementTimeForForcedMode(ReadTemperatureSampling()));
            }

            //Read the MSB, LSB and bits 7:4 (XLSB) of the temperature from the BMP280 registers
            byte msb = Read8BitsFromRegister((byte)Bmx280Register.TEMPDATA_MSB);
            byte lsb = Read8BitsFromRegister((byte)Bmx280Register.TEMPDATA_LSB);
            byte xlsb = Read8BitsFromRegister((byte)Bmx280Register.TEMPDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            int t = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            return CompensateTemperature(t);
        }

        /// <summary>
        /// Read the <see cref="Bmx280PowerMode"/> state.
        /// </summary>
        /// <returns>The current <see cref="Bmx280PowerMode"/>.</returns>
        /// <exception cref="NotImplementedException">Thrown when the power mode does not match a defined mode in <see cref="Bmx280PowerMode"/>.</exception>
        public Bmx280PowerMode ReadPowerMode()
        {
            byte read = Read8BitsFromRegister(_controlRegister);

            // Get only the power mode bits.
            var powerMode = (byte)(read & 0b_0000_0011);

            if (Enum.IsDefined(typeof(Bmx280PowerMode), powerMode) == false)
            {
                throw new IOException("Read unexpected power mode");
            }

            return powerMode switch
            {
                0b00 => Bmx280PowerMode.Sleep,
                0b10 => Bmx280PowerMode.Forced,
                0b11 => Bmx280PowerMode.Normal,
                _ => throw new NotImplementedException($"Read power mode not defined by specification.")
            };
        }

        /// <summary>
        /// Reads the pressure from the sensor.
        /// </summary>
        /// <returns>Atmospheric pressure in Pa.</returns>
        public async Task<double> ReadPressureAsync()
        {
            if (ReadPowerMode() == Bmx280PowerMode.Forced)
            {
                await Task.Delay(GetMeasurementTimeForForcedMode(ReadPressureSampling()));
            }

            //Read the temperature first to load the t_fine value for compensation
            if (TemperatureFine == int.MinValue)
            {
                await ReadTemperatureAsync();
            }

            // Read pressure data.
            byte msb = Read8BitsFromRegister((byte)Bmx280Register.PRESSUREDATA_MSB);
            byte lsb = Read8BitsFromRegister((byte)Bmx280Register.PRESSUREDATA_LSB);
            byte xlsb = Read8BitsFromRegister((byte)Bmx280Register.PRESSUREDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer.
            int t = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            //Convert the raw value to the pressure in Pa.
            long pres = CompensatePressure(t);

            //Return the temperature as a float value.
            return (double)pres / 256;
        }

        /// <summary>
        /// Calculates the altitude in meters from the specified sea-level pressure(in hPa).
        /// </summary>
        /// <param name="seaLevelPressure">Sea-level pressure in hPa.</param>
        /// <returns>Height in meters from sea-level.</returns>
        public async Task<double> ReadAltitudeAsync(double seaLevelPressure)
        {
            // Read the pressure first.
            double pressure = await ReadPressureAsync();

            // Convert the pressure to Hectopascals (hPa).
            pressure /= 100;

            // Calculate and return the altitude using the international barometric formula.
            return 44330.0 * (1.0 - Math.Pow((pressure / seaLevelPressure), 0.1903));
        }

        /// <summary>
        /// Get the current status of the device.
        /// </summary>
        /// <returns>The <see cref="DeviceStatus"/>.</returns>
        public DeviceStatus ReadStatus()
        {
            var status = Read8BitsFromRegister((byte)Bmx280Register.STATUS);

            // Bit 3.
            var measuring = ((status >> 3) & 1) == 1;

            // Bit 0.
            var imageUpdating = (status & 1) == 1;

            return new DeviceStatus
            {
                ImageUpdating = imageUpdating,
                Measuring = measuring
            };
        }

        /// <summary>
        /// Sets the IIR filter mode.
        /// </summary>
        /// <param name="filteringMode">The <see cref="FilteringMode"/> to set.</param>
        public void SetFilterMode(FilteringMode filteringMode)
        {
            byte current = Read8BitsFromRegister((byte)Bmx280Register.CONFIG);
            current = (byte)((current & 0b_1110_0011) | (byte)filteringMode << 2);
            _i2cDevice.Write(new[] { (byte)Bmx280Register.CONFIG, current });
        }

        /// <summary>
        /// Sets the power mode to the given mode
        /// </summary>
        /// <param name="powerMode">The <see cref="Bmx280PowerMode"/> to set.</param>
        public void SetPowerMode(Bmx280PowerMode powerMode)
        {
            byte status = Read8BitsFromRegister(_controlRegister);

            // Clear last two bits.
            status = (byte)(status & 0b_1111_1100);

            status = (byte)(status | (byte)powerMode);
            _i2cDevice.Write(new[] { _controlRegister, status });
        }

        /// <summary>
        /// Sets the standby time mode the device will used when operating in normal mode.
        /// </summary>
        /// <param name="standbyTime">The <see cref="StandbyTime"/> to set.</param>
        public void SetStandbyTime(StandbyTime standbyTime)
        {
            byte current = Read8BitsFromRegister((byte)Bmx280Register.CONFIG);
            current = (byte)((current & 0b_0001_1111) | (byte)standbyTime << 5);
            _i2cDevice.Write(new[] { (byte)Bmx280Register.CONFIG, current });
        }

        /// <summary>
        /// Reads the currently configured standby time mode the device will used when operating in normal mode.
        /// </summary>
        /// <returns>The current <see cref="StandbyTime"/>.</returns>
        public StandbyTime ReadStandbyTime()
        {
            byte current = Read8BitsFromRegister((byte)Bmx280Register.CONFIG);
            var time = (byte)((current & 0b_1110_0000) >> 5);

            return time switch
            {
                0b000 => StandbyTime.Ms0_5,
                0b001 => StandbyTime.Ms62_5,
                0b010 => StandbyTime.Ms125,
                0b011 => StandbyTime.Ms250,
                0b100 => StandbyTime.Ms500,
                0b101 => StandbyTime.Ms1000,
                0b110 => StandbyTime.Ms10,
                0b111 => StandbyTime.Ms20,
                _ => throw new NotImplementedException($"Value read from registers {time:x2} was not defined by specifications.")
            };
        }

        /// <summary>
        /// Recommended wait timings from the datasheet.
        /// </summary>
        /// <param name="sampleMode">The <see cref="Sampling"/> to get for.</param>
        /// <returns>The time it takes for the chip to read data in milliseconds rounded up.</returns>
        internal int GetMeasurementTimeForForcedMode(Sampling sampleMode)
        {
            return sampleMode switch
            {
                Sampling.UltraLowPower => 7,
                Sampling.LowPower => 9,
                Sampling.Standard => 14,
                Sampling.HighResolution => 23,
                Sampling.UltraHighResolution => 44,
                _ => 0
            };
        }

        /// <summary>
        /// Compensates the pressure in Pa, in Q24.8 format (24 integer bits and 8 fractional bits).
        /// </summary>
        /// <param name="adcPressure">The pressure value read from the device.</param>
        /// <returns>Pressure in Hectopascals (hPa).</returns>
        /// <remarks>
        /// Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa.
        /// </remarks>
        private long CompensatePressure(int adcPressure)
        {
            // Formula from the datasheet http://www.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf
            // The pressure is calculated using the compensation formula in the BMP280 datasheet
            long var1 = TemperatureFine - 128000;
            long var2 = var1 * var1 * (long)_calibrationData.DigP6;
            var2 = var2 + ((var1 * (long)_calibrationData.DigP5) << 17);
            var2 = var2 + ((long)_calibrationData.DigP4 << 35);
            var1 = ((var1 * var1 * (long)_calibrationData.DigP3) >> 8) + ((var1 * (long)_calibrationData.DigP2) << 12);
            var1 = (((((long)1 << 47) + var1)) * (long)_calibrationData.DigP1) >> 33;
            if (var1 == 0)
            {
                return 0; //Avoid exception caused by division by zero
            }
            //Perform calibration operations
            long p = 1048576 - adcPressure;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = ((long)_calibrationData.DigP9 * (p >> 13) * (p >> 13)) >> 25;
            var2 = ((long)_calibrationData.DigP8 * p) >> 19;
            p = ((p + var1 + var2) >> 8) + ((long)_calibrationData.DigP7 << 4);

            return p;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//Ported from https://github.com/adafruit/Adafruit_BMP280_Library/blob/master/Adafruit_BMP280.cpp
//Formulas and code examples can also be found in the datasheet http://www.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading.Tasks;
using Iot.Device.Bmx280.CalibrationData;
using Iot.Device.Bmx280.Register;
using Iot.Units;

namespace Iot.Device.Bmx280
{
    /// <summary>
    /// Represents the core functionality of the Bmx280 family.
    /// </summary>
    public class Bmx280Base : Bmxx80Base
    {
        internal new Bmx280CalibrationData _calibrationData = new Bmx280CalibrationData();

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmx280Base"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Bmx280Base(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
            _calibrationData.ReadFromDevice(this);
        }

        /// <summary>
        /// Read the temperature.
        /// </summary>
        /// <returns>Calculated temperature.</returns>
        public async Task<Temperature> ReadTemperatureAsync()
        {
            if (ReadPowerMode() == PowerMode.Forced)
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
        /// Recommended wait timings from the datasheet.
        /// </summary>
        /// <param name="sampleMode">The <see cref="Sampling"/> to get for.</param>
        /// <returns>The time it takes for the chip to read data in milliseconds rounded up.</returns>
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
        /// Reads the pressure from the sensor.
        /// </summary>
        /// <returns>Atmospheric pressure in Pa.</returns>
        public async Task<double> ReadPressureAsync()
        {
            if (ReadPowerMode() == PowerMode.Forced)
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
            return (pres) / 256;
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
        /// Compensates the pressure in Pa, in Q24.8 format (24 integer bits and 8 fractional bits).
        /// </summary>
        /// <param name="adcPressure">The pressure value read from the device.</param>
        /// <returns>Pressure in Hectopascals (hPa).</returns>
        /// <remarks>
        /// Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa.
        /// </remarks>
        private long CompensatePressure(int adcPressure)
        {
            //Formula from the datasheet
            //The pressure is calculated using the compensation formula in the BMP280 datasheet
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

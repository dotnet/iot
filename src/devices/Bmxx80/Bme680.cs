// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading.Tasks;
using Iot.Device.Bmxx80.CalibrationData;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Bmxx80.Register;
using Iot.Units;

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Represents a BME680 temperature, pressure, relative humidity and VOC gas sensor.
    /// </summary>
    public class Bme680 : Bmxx80Base
    {
        /// <summary>
        /// Default I2C bus address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x76;

        /// <summary>
        /// Secondary I2C bus address.
        /// </summary>
        public const byte SecondaryI2cAddress = 0x77;

        /// <summary>
        /// The expected chip ID of the BME680.
        /// </summary>
        private const byte DeviceId = 0x61;

        /// <summary>
        /// Calibration data for the <see cref="Bme680"/>.
        /// </summary>
        private readonly Bme680CalibrationData _bme680Calibration;

        /// <summary>
        /// Initialize a new instance of the <see cref="Bme680"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Bme680(I2cDevice i2cDevice)
            : base(DeviceId, i2cDevice)
        {
            var bme680CalibrationData = new Bme680CalibrationData();
            bme680CalibrationData.ReadFromDevice(this);
            _bme680Calibration = bme680CalibrationData;
            _calibrationData = bme680CalibrationData;

            _communicationProtocol = CommunicationProtocol.I2c;
            _controlRegister = (byte)Bme680Register.CTRL_MEAS;
        }

        /// <summary>
        /// Set the humidity sampling.
        /// </summary>
        /// <param name="sampling">The <see cref="Sampling"/> to set.</param>
        public void SetHumiditySampling(Sampling sampling)
        {
            var register = (byte)Bme680Register.CTRL_HUM;
            byte read = Read8BitsFromRegister(register);

            // Clear first 3 bits.
            var cleared = (byte)(read & 0b_1111_1000);

            _i2cDevice.Write(new[] { register, (byte)(cleared | (byte)sampling) });
        }

        /// <summary>
        /// Sets the power mode to the given mode
        /// </summary>
        /// <param name="powerMode">The <see cref="Bme680PowerMode"/> to set.</param>
        public void SetPowerMode(Bme680PowerMode powerMode)
        {
            byte read = Read8BitsFromRegister(_controlRegister);

            // Clear first 2 bits.
            var cleared = (byte)(read & 0b_1111_1100);

            _i2cDevice.Write(new[] { _controlRegister, (byte)(cleared | (byte)powerMode) });
        }

        /// <summary>
        /// Read a value indicating whether or not new sensor data is available.
        /// </summary>
        /// <returns>True if new data is available.</returns>
        public bool ReadHasNewData()
        {
            byte read = Read8BitsFromRegister((byte)Bme680Register.STATUS);

            // Get only the power mode bit.
            var hasNewData = (byte)(read & 0b_1000_0000);

            return (hasNewData >> 7) == 1;
        }

        /// <summary>
        /// Read the humidity.
        /// </summary>
        /// <returns>Calculated humidity.</returns>
        public async Task<double> ReadHumidityAsync()
        {
            // Read humidity data.
            byte msb = Read8BitsFromRegister((byte)Bme680Register.HUMIDITYDATA_MSB);
            byte lsb = Read8BitsFromRegister((byte)Bme680Register.HUMIDITYDATA_LSB);

            // Convert to a 32bit integer.
            var adcHumidity = (msb << 8) + lsb;

            return CompensateHumidity((await ReadTemperatureAsync()).Celsius, adcHumidity);
        }

        /// <summary>
        /// Read the <see cref="Bme680PowerMode"/> state.
        /// </summary>
        /// <returns>The current <see cref="Bme680PowerMode"/>.</returns>
        /// <exception cref="NotImplementedException">Thrown when the power mode does not match a defined mode in <see cref="Bme680PowerMode"/>.</exception>
        public Bme680PowerMode ReadPowerMode()
        {
            byte read = Read8BitsFromRegister(_controlRegister);

            return (Bme680PowerMode)(read & 0b_0000_0011);
        }

        /// <summary>
        /// Read the pressure.
        /// </summary>
        /// <returns>Calculated pressure in Pa.</returns>
        public async Task<double> ReadPressureAsync()
        {
            // Read pressure data.
            byte lsb = Read8BitsFromRegister((byte)Bme680Register.PRESSUREDATA_LSB);
            byte msb = Read8BitsFromRegister((byte)Bme680Register.PRESSUREDATA_MSB);
            byte xlsb = Read8BitsFromRegister((byte)Bme680Register.PRESSUREDATA_XLSB);

            // Convert to a 32bit integer.
            var adcPressure = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            // Read the temperature first to load the t_fine value for compensation.
            await ReadTemperatureAsync();

            return CompensatePressure(adcPressure);
        }

        /// <summary>
        /// Read the temperature.
        /// </summary>
        /// <returns>Calculated temperature.</returns>
        public Task<Temperature> ReadTemperatureAsync()
        {
            // Read temperature data.
            byte lsb = Read8BitsFromRegister((byte)Bme680Register.TEMPDATA_LSB);
            byte msb = Read8BitsFromRegister((byte)Bme680Register.TEMPDATA_MSB);
            byte xlsb = Read8BitsFromRegister((byte)Bme680Register.TEMPDATA_XLSB);

            // Convert to a 32bit integer.
            var adcTemperature = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            return Task.FromResult(CompensateTemperature(adcTemperature));
        }

        /// <summary>
        /// Compensates the humidity.
        /// </summary>
        /// <param name="temperature">The temperature to use.</param>
        /// <param name="adcHumidity">The humidity value read from the device.</param>
        /// <returns>The percentage relative humidity.</returns>
        private double CompensateHumidity(double temperature, int adcHumidity)
        {
            // Calculate the humidity.
            var var1 = adcHumidity - ((_bme680Calibration.DigH1 * 16.0) + ((_bme680Calibration.DigH3 / 2.0) * temperature));
            var var2 = var1 * ((_bme680Calibration.DigH2 / 262144.0) * (1.0 + ((_bme680Calibration.DigH4 / 16384.0) * temperature)
                + ((_bme680Calibration.DigH5 / 1048576.0) * temperature * temperature)));
            var var3 = _bme680Calibration.DigH6 / 16384.0;
            var var4 = _bme680Calibration.DigH7 / 2097152.0;
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
        /// Compensates the pressure.
        /// </summary>
        /// <param name="adcPressure">The pressure value read from the device.</param>
        /// <returns>The pressure in Pa.</returns>
        private double CompensatePressure(int adcPressure)
        {
            // Calculate the pressure.
            var var1 = (TemperatureFine / 2.0) - 64000.0;
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

        protected override int GetTempCalibrationFactor() => 16;
    }
}

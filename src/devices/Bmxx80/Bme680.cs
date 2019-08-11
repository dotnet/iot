// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Threading.Tasks;
using Iot.Device.Bmxx80.CalibrationData;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Bmxx80.Register;
using Iot.Units;

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Represents a BME680 temperature, pressure, relative humidity and VOC gas sensor.
    /// </summary>
    public sealed class Bme680 : Bmxx80Base
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

        private readonly List<Bme680HeaterProfileConfig> _heaterConfigs = new List<Bme680HeaterProfileConfig>();

        protected override int _tempCalibrationFactor => 16;

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

            SetDefaultConfiguration();
        }

        private Sampling _humiditySampling;

        /// <summary>
        /// Gets or sets the humidity sampling.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Sampling"/> is set to an undefined mode.</exception>
        public Sampling HumiditySampling
        {
            get => _humiditySampling;
            set
            {
                if (!Enum.IsDefined(typeof(Sampling), value))
                    throw new ArgumentOutOfRangeException();

                var status = Read8BitsFromRegister((byte)Bme680Register.CTRL_HUM);
                status = (byte)((status & (byte)~Bme680Mask.HUMIDITY_SAMPLING) | (byte)value);

                _i2cDevice.Write(new[] { (byte)Bme680Register.CTRL_HUM, status });
                _humiditySampling = value;
            }
        }

        private Bme680HeaterProfile _currentHeaterProfile;

        /// <summary>
        /// Gets or sets the heater profile to be used for measurements.
        /// Current heater profile is only set if the chosen profile is configured.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Bme680HeaterProfile"/> is set to an undefined profile.</exception>
        public Bme680HeaterProfile CurrentHeaterProfile
        {
            get => _currentHeaterProfile;
            set
            {
                if (_heaterConfigs.Exists(config => config.HeaterProfile == value))
                {
                    if (!Enum.IsDefined(typeof(Bme680HeaterProfile), value))
                        throw new ArgumentOutOfRangeException();

                    var heaterProfile = Read8BitsFromRegister((byte)Bme680Register.CTRL_GAS_1);
                    heaterProfile = (byte)((heaterProfile & (byte)~Bme680Mask.NB_CONV) | (byte)value);

                    _i2cDevice.Write(new[] { (byte)Bme680Register.CTRL_GAS_1, heaterProfile });
                    _currentHeaterProfile = value;
                }
            }
        }

        private Bme680FilteringMode _filterMode;

        /// <summary>
        /// Gets or sets the filtering mode to be used for measurements.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Bme680FilteringMode"/> is set to an undefined mode.</exception>
        public Bme680FilteringMode FilterMode
        {
            get => _filterMode;
            set
            {
                if (!Enum.IsDefined(typeof(Bme680FilteringMode), value))
                    throw new ArgumentOutOfRangeException();

                var filter = Read8BitsFromRegister((byte)Bme680Register.CONFIG);
                filter = (byte)((filter & (byte)~Bme680Mask.FILTER_COEFFICIENT) | (byte)value << 2);

                _i2cDevice.Write(new[] { (byte)Bme680Register.CONFIG, filter });
                _filterMode = value;
            }
        }

        private bool _heaterIsEnabled;

        /// <summary>
        /// Gets or sets whether the heater is enabled.
        /// </summary>
        public bool HeaterIsEnabled
        {
            get => _heaterIsEnabled;
            set
            {
                var heaterStatus = Read8BitsFromRegister((byte)Bme680Register.CTRL_GAS_0);
                heaterStatus = (byte)((heaterStatus & (byte)~Bme680Mask.HEAT_OFF) | Convert.ToByte(!value) << 3);

                _i2cDevice.Write(new[] { (byte)Bme680Register.CTRL_GAS_0, heaterStatus });
                _heaterIsEnabled = value;
            }
        }

        private bool _gasConversionIsEnabled;

        /// <summary>
        /// Gets or sets whether gas conversions are enabled.
        /// </summary>
        public bool GasConversionIsEnabled
        {
            get => _gasConversionIsEnabled;
            set
            {
                var gasConversion = Read8BitsFromRegister((byte)Bme680Register.CTRL_GAS_1);
                gasConversion = (byte)((gasConversion & (byte)~Bme680Mask.RUN_GAS) | Convert.ToByte(value) << 4);

                _i2cDevice.Write(new[] { (byte)Bme680Register.CTRL_GAS_1, gasConversion });
                _gasConversionIsEnabled = value;
            }
        }

        /// <summary>
        /// Reads whether new data is available.
        /// </summary>
        public bool ReadNewDataIsAvailable()
        {
            var newData = Read8BitsFromRegister((byte)Bme680Register.STATUS);
            newData = (byte)(newData >> 7);

            return Convert.ToBoolean(newData);
        }

        /// <summary>
        /// Reads whether a gas measurement is in process.
        /// </summary>
        public bool ReadGasMeasurementInProcess()
        {
            var gasMeasInProcess = Read8BitsFromRegister((byte)Bme680Register.STATUS);
            gasMeasInProcess = (byte)((gasMeasInProcess & (byte)Bme680Mask.GAS_MEASURING) >> 6);

            return Convert.ToBoolean(gasMeasInProcess);
        }

        /// <summary>
        /// Reads whether a measurement of any kind is in process.
        /// </summary>
        public bool ReadMeasurementInProcess()
        {
            var measInProcess = Read8BitsFromRegister((byte)Bme680Register.STATUS);
            measInProcess = (byte)((measInProcess & (byte)Bme680Mask.MEASURING) >> 5);

            return Convert.ToBoolean(measInProcess);
        }

        /// <summary>
        /// Reads whether the target heater temperature is reached.
        /// </summary>
        public bool ReadHeaterIsStable()
        {
                var heaterStable = Read8BitsFromRegister((byte)Bme680Register.GAS_RANGE);
                heaterStable = (byte)((heaterStable & (byte)Bme680Mask.HEAT_STAB) >> 4);

                return Convert.ToBoolean(heaterStable);
        }

        /// <summary>
        /// Sets the power mode to the given mode
        /// </summary>
        /// <param name="powerMode">The <see cref="Bme680PowerMode"/> to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the power mode does not match a defined mode in <see cref="Bme680PowerMode"/>.</exception>
        public void SetPowerMode(Bme680PowerMode powerMode)
        {
            if (!Enum.IsDefined(typeof(Bme680PowerMode), powerMode))
                throw new ArgumentOutOfRangeException();

            var status = Read8BitsFromRegister((byte)Bme680Register.CTRL_MEAS);
            status = (byte)((status & (byte)~Bme680Mask.PWR_MODE) | (byte)powerMode);

            _i2cDevice.Write(new[] { (byte)Bme680Register.CTRL_MEAS, status });
        }

        /// <summary>
        /// Configures a heater profile, making it ready for use.
        /// </summary>
        /// <param name="profile">The <see cref="Bme680HeaterProfile"/> to configure.</param>
        /// <param name="targetTemperature">The target temperature in °C. Ranging from 0-400.</param>
        /// <param name="duration">The duration in ms. Ranging from 0-4032.</param>
        /// <param name="ambientTemperature">The ambient temperature in °C.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the heating profile does not match a defined profile in <see cref="Bme680HeaterProfile"/>.</exception>
        /// <returns></returns>
        public void ConfigureHeatingProfile(Bme680HeaterProfile profile, ushort targetTemperature, ushort duration, double ambientTemperature)
        {
            if (!Enum.IsDefined(typeof(Bme680HeaterProfile), profile))
                throw new ArgumentOutOfRangeException();

            // read ambient temperature for resistance calculation
            var heaterResistance = CalculateHeaterResistance(targetTemperature, (short)ambientTemperature);
            var heaterDuration = CalculateHeaterDuration(duration);

            _i2cDevice.Write(new[] { (byte)((byte)Bme680Register.RES_HEAT_0 + profile), heaterResistance });
            _i2cDevice.Write(new[] { (byte)((byte)Bme680Register.GAS_WAIT_0 + profile), heaterDuration });

            // cache heater configuration
            if (_heaterConfigs.Exists(config => config.HeaterProfile == profile))
                _heaterConfigs.Remove(_heaterConfigs.Single(config => config.HeaterProfile == profile));
            _heaterConfigs.Add(new Bme680HeaterProfileConfig(profile, heaterResistance, duration));
        }

        /// <summary>
        /// Read the <see cref="Bme680PowerMode"/> state.
        /// </summary>
        /// <returns>The current <see cref="Bme680PowerMode"/>.</returns>
        public Bme680PowerMode ReadPowerMode()
        {
            var status = Read8BitsFromRegister((byte)Bme680Register.CTRL_MEAS);

            return (Bme680PowerMode)(status & (byte)Bme680Mask.PWR_MODE);
        }

        /// <summary>
        /// Gets the required time in ms to perform a measurement. The duration of the gas
        /// measurement is not considered if <see cref="GasConversionIsEnabled"/> is set to false
        /// or the chosen <see cref="Bme680HeaterProfile"/> is not configured.
        /// The precision of this duration is within 1ms of the actual measurement time.
        /// </summary>
        /// <param name="profile">The used <see cref="Bme680HeaterProfile"/>. </param>
        /// <returns></returns>
        public int GetMeasurementDuration(Bme680HeaterProfile profile)
        {
            var osToMeasCycles = new byte[] { 0, 1, 2, 4, 8, 16 };
            var osToSwitchCount = new byte[] { 0, 1, 1, 1, 1, 1 };

            var measCycles = osToMeasCycles[(int)TemperatureSampling];
            measCycles += osToMeasCycles[(int)PressureSampling];
            measCycles += osToMeasCycles[(int)HumiditySampling];

            var switchCount = osToSwitchCount[(int)TemperatureSampling];
            switchCount += osToSwitchCount[(int)PressureSampling];
            switchCount += osToSwitchCount[(int)HumiditySampling];

            double measDuration = measCycles * 1963;
            measDuration += 477 * switchCount;      // TPH switching duration

            if (GasConversionIsEnabled)
                measDuration += 477 * 5;            // Gas measurement duration
            measDuration += 500;                    // get it to the closest whole number
            measDuration /= 1000.0;                 // convert to ms
            measDuration += 1;                      // wake up duration of 1ms

            if (GasConversionIsEnabled && _heaterConfigs.Exists(config => config.HeaterProfile == profile))
                measDuration += _heaterConfigs.Single(config => config.HeaterProfile == profile).HeaterDuration;

            return (int)Math.Ceiling(measDuration);
        }

        /// <summary>
        /// Read the humidity.
        /// </summary>
        /// <returns>Calculated humidity.</returns>
        public async Task<double> ReadHumidityAsync()
        {
            if (HumiditySampling == Sampling.Skipped)
                return double.NaN;

            // Read humidity data.
            var msb = Read8BitsFromRegister((byte)Bme680Register.HUMIDITYDATA_MSB);
            var lsb = Read8BitsFromRegister((byte)Bme680Register.HUMIDITYDATA_LSB);

            // Convert to a 32bit integer.
            var adcHumidity = (msb << 8) + lsb;

            return CompensateHumidity((await ReadTemperatureAsync()).Celsius, adcHumidity);
        }

        /// <summary>
        /// Read the pressure.
        /// </summary>
        /// <returns>Calculated pressure in Pa.</returns>
        public async Task<double> ReadPressureAsync()
        {
            if (PressureSampling == Sampling.Skipped)
                return double.NaN;

            // Read pressure data.
            var lsb = Read8BitsFromRegister((byte)Bme680Register.PRESSUREDATA_LSB);
            var msb = Read8BitsFromRegister((byte)Bme680Register.PRESSUREDATA_MSB);
            var xlsb = Read8BitsFromRegister((byte)Bme680Register.PRESSUREDATA_XLSB);

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
            if (TemperatureSampling == Sampling.Skipped)
                return Task.FromResult(Temperature.FromCelsius(double.NaN));

            // Read temperature data.
            var lsb = Read8BitsFromRegister((byte)Bme680Register.TEMPDATA_LSB);
            var msb = Read8BitsFromRegister((byte)Bme680Register.TEMPDATA_MSB);
            var xlsb = Read8BitsFromRegister((byte)Bme680Register.TEMPDATA_XLSB);

            // Convert to a 32bit integer.
            var adcTemperature = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            return Task.FromResult(CompensateTemperature(adcTemperature));
        }

        /// <summary>
        /// Reads the gas resistance.
        /// </summary>
        /// <returns>Gas resistance in Ohm. NaN if last gas measurement is invalid.</returns>
        public Task<double> ReadGasResistanceAsync()
        {
            if (!ReadGasMeasurementIsValid() || !ReadHeaterIsStable())
                return Task.FromResult(double.NaN);

            // Read 10 bit gas resistance value from registers
            var g1 = Read8BitsFromRegister((byte)Bme680Register.GAS_RES);
            var g2 = Read8BitsFromRegister((byte)Bme680Register.GAS_RES + sizeof(byte));
            var gasRange = Read8BitsFromRegister((byte)Bme680Register.GAS_RANGE);

            var gasResistance = (ushort)((ushort)(g1 << 2) + (byte)(g2 >> 6));
            gasRange &= (byte)Bme680Mask.GAS_RANGE;

            return Task.FromResult(CalculateGasResistance(gasResistance, gasRange));
        }

        protected override void SetDefaultConfiguration()
        {
            base.SetDefaultConfiguration();
            HumiditySampling = Sampling.UltraLowPower;
            FilterMode = Bme680FilteringMode.C0;

            var currentTemp = ReadTemperatureAsync().GetAwaiter().GetResult();
            ConfigureHeatingProfile(Bme680HeaterProfile.Profile1, 320, 150, currentTemp.Celsius);
            CurrentHeaterProfile = Bme680HeaterProfile.Profile1;

            HeaterIsEnabled = true;
            GasConversionIsEnabled = true;
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

        private bool ReadGasMeasurementIsValid()
        {
            var gasMeasValid = Read8BitsFromRegister((byte)Bme680Register.GAS_RANGE);
            gasMeasValid = (byte)((gasMeasValid & (byte)Bme680Mask.GAS_VALID) >> 5);

            return Convert.ToBoolean(gasMeasValid);
        }

        private double CalculateGasResistance(ushort adcGasRes, byte gasRange)
        {
            var k1Lookup = new[] { 0.0, 0.0, 0.0, 0.0, 0.0, -1.0, 0.0, -0.8, 0.0, 0.0, -0.2, -0.5, 0.0, -1.0, 0.0, 0.0 };
            var k2Lookup = new[] { 0.0, 0.0, 0.0, 0.0, 0.1, 0.7, 0.0, -0.8, -0.1, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

            var var1 = 1340.0 + 5.0 * _bme680Calibration.RangeSwErr;
            var var2 = var1 * (1.0 + k1Lookup[gasRange] / 100.0);
            var var3 = 1.0 + k2Lookup[gasRange] / 100.0;
            var gasResistance = 1.0 / (var3 * 0.000000125 * (1 << gasRange) * ((adcGasRes - 512.0) / var2 + 1.0));

            return gasResistance;
        }

        private byte CalculateHeaterResistance(ushort setTemp, short ambientTemp)
        {
            // limit maximum temperature to 400°C
            if (setTemp > 400)
                setTemp = 400;

            var var1 = _bme680Calibration.DigGh1 / 16.0 + 49.0;
            var var2 = _bme680Calibration.DigGh2 / 32768.0 * 0.0005 + 0.00235;
            var var3 = _bme680Calibration.DigGh3 / 1024.0;
            var var4 = var1 * (1.0 + var2 * setTemp);
            var var5 = var4 + var3 * ambientTemp;
            var heaterResistance = (byte)(3.4 * (var5 * (4.0 / (4.0 + _bme680Calibration.ResHeatRange)) * (1.0 / (1.0 + _bme680Calibration.ResHeatVal * 0.002)) - 25));

            return heaterResistance;
        }

        // The duration is interpreted as follows:
        // Byte [7:6]: multiplication factor of 1, 4, 16 or 64
        // Byte [5:0]: 64 timer values, 1ms step size
        // Values are rounded down
        private byte CalculateHeaterDuration(ushort duration)
        {
            byte factor = 0;
            byte durationValue;

            // check if value exceeds maximum duration
            if (duration > 0xFC0)
                durationValue = 0xFF;
            else
            {
                while (duration > 0x3F)
                {
                    duration = (ushort)(duration >> 2);
                    factor += 1;
                }

                durationValue = (byte)(duration + factor * 64);
            }

            return durationValue;
        }
    }
}

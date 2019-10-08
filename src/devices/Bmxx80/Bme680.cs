// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
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
        private Bme680CalibrationData _bme680Calibration;

        /// <inheritdoc/>
        protected override int _tempCalibrationFactor => 16;

        private readonly List<Bme680HeaterProfileConfig> _heaterConfigs = new List<Bme680HeaterProfileConfig>();
        private bool _gasConversionIsEnabled;
        private bool _heaterIsEnabled;

        private Bme680HeaterProfile _heaterProfile;
        private Bme680FilteringMode _filterMode;
        private Sampling _humiditySampling;

        private static readonly byte[] s_osToMeasCycles = { 0, 1, 2, 4, 8, 16 };
        private static readonly byte[] s_osToSwitchCount = { 0, 1, 1, 1, 1, 1 };
        private static readonly double[] s_k1Lookup = { 0.0, 0.0, 0.0, 0.0, 0.0, -1.0, 0.0, -0.8, 0.0, 0.0, -0.2, -0.5, 0.0, -1.0, 0.0, 0.0 };
        private static readonly double[] s_k2Lookup = { 0.0, 0.0, 0.0, 0.0, 0.1, 0.7, 0.0, -0.8, -0.1, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

        /// <summary>
        /// Initialize a new instance of the <see cref="Bme680"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Bme680(I2cDevice i2cDevice)
            : base(DeviceId, i2cDevice)
        {
            _communicationProtocol = CommunicationProtocol.I2c;
        }

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

                Span<byte> command = stackalloc[] { (byte)Bme680Register.CTRL_HUM, status };
                _i2cDevice.Write(command);
                _humiditySampling = value;
            }
        }

        /// <summary>
        /// Gets or sets the heater profile to be used for measurements.
        /// Current heater profile is only set if the chosen profile is configured.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Bme680HeaterProfile"/> is set to an undefined profile.</exception>
        public Bme680HeaterProfile HeaterProfile
        {
            get => _heaterProfile;
            set
            {
                if (_heaterConfigs.Exists(config => config.HeaterProfile == value))
                {
                    if (!Enum.IsDefined(typeof(Bme680HeaterProfile), value))
                        throw new ArgumentOutOfRangeException();

                    var heaterProfile = Read8BitsFromRegister((byte)Bme680Register.CTRL_GAS_1);
                    heaterProfile = (byte)((heaterProfile & (byte)~Bme680Mask.NB_CONV) | (byte)value);

                    Span<byte> command = stackalloc[] { (byte)Bme680Register.CTRL_GAS_1, heaterProfile };
                    _i2cDevice.Write(command);
                    _heaterProfile = value;
                }
            }
        }

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

                Span<byte> command = stackalloc[] { (byte)Bme680Register.CONFIG, filter };
                _i2cDevice.Write(command);
                _filterMode = value;
            }
        }

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

                Span<byte> command = stackalloc[] { (byte)Bme680Register.CTRL_GAS_0, heaterStatus };
                _i2cDevice.Write(command);
                _heaterIsEnabled = value;
            }
        }

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

                Span<byte> command = stackalloc[] { (byte)Bme680Register.CTRL_GAS_1, gasConversion };
                _i2cDevice.Write(command);
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

            Span<byte> command = stackalloc[] { (byte)Bme680Register.CTRL_MEAS, status };
            _i2cDevice.Write(command);
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

            Span<byte> resistanceCommand = stackalloc[] { (byte)((byte)Bme680Register.RES_HEAT_0 + profile), heaterResistance };
            Span<byte> durationCommand = stackalloc[] { (byte)((byte)Bme680Register.GAS_WAIT_0 + profile), heaterDuration };
            _i2cDevice.Write(resistanceCommand);
            _i2cDevice.Write(durationCommand);

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
            var measCycles = s_osToMeasCycles[(int)TemperatureSampling];
            measCycles += s_osToMeasCycles[(int)PressureSampling];
            measCycles += s_osToMeasCycles[(int)HumiditySampling];

            var switchCount = s_osToSwitchCount[(int)TemperatureSampling];
            switchCount += s_osToSwitchCount[(int)PressureSampling];
            switchCount += s_osToSwitchCount[(int)HumiditySampling];

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
        /// Reads the humidity. A return value indicates whether the reading succeeded.
        /// </summary>
        /// <param name="humidity">
        /// Contains the measured humidity as %rH if the <see cref="HumiditySampling"/> was not set to <see cref="Sampling.Skipped"/>.
        /// Contains <see cref="double.NaN"/> otherwise.
        /// </param>
        /// <returns><code>true</code> if measurement was not skipped, otherwise <code>false</code>.</returns>
        public bool TryReadHumidity(out double humidity)
        {
            if (HumiditySampling == Sampling.Skipped)
            {
                humidity = double.NaN;
                return false;
            }

            // Read humidity data.
            var hum = Read16BitsFromRegister((byte)Bme680Register.HUMIDITYDATA, Endianness.BigEndian);

            TryReadTemperature(out _);
            humidity = CompensateHumidity(hum);
            return true;
        }

        /// <summary>
        /// Reads the pressure. A return value indicates whether the reading succeeded.
        /// </summary>
        /// <param name="pressure">
        /// Contains the measured pressure if the <see cref="Bmxx80Base.PressureSampling"/> was not set to <see cref="Sampling.Skipped"/>.
        /// Contains <see cref="double.NaN"/> otherwise.
        /// </param>
        /// <returns><code>true</code> if measurement was not skipped, otherwise <code>false</code>.</returns>
        public override bool TryReadPressure(out Pressure pressure)
        {
            if (PressureSampling == Sampling.Skipped)
            {
                pressure = Pressure.FromPascal(double.NaN);
                return false;
            }
                

            // Read pressure data.
            var press = (int)Read24BitsFromRegister((byte)Bme680Register.PRESSUREDATA, Endianness.BigEndian);

            // Read the temperature first to load the t_fine value for compensation.
            TryReadTemperature(out _);

            pressure = CompensatePressure(press >> 4);
            return true;
        }

        /// <summary>
        /// Reads the temperature. A return value indicates whether the reading succeeded.
        /// </summary>
        /// <param name="temperature">
        /// Contains the measured temperature if the <see cref="Bmxx80Base.TemperatureSampling"/> was not set to <see cref="Sampling.Skipped"/>.
        /// Contains <see cref="double.NaN"/> otherwise.
        /// </param>
        /// <returns><code>true</code> if measurement was not skipped, otherwise <code>false</code>.</returns>
        public override bool TryReadTemperature(out Temperature temperature)
        {
            if (TemperatureSampling == Sampling.Skipped)
            {
                temperature = Temperature.FromCelsius(double.NaN);
                return false;
            }
                

            var temp = (int)Read24BitsFromRegister((byte)Bme680Register.TEMPDATA, Endianness.BigEndian);

            temperature = CompensateTemperature(temp >> 4);
            return true;
        }

        /// <summary>
        /// Reads the gas resistance. A return value indicates whether the reading succeeded.
        /// </summary>
        /// <param name="gasResistance">
        /// Contains the measured gas resistance in Ohm if the heater module reached the target temperature and
        /// the measurement was valid. Contains <see cref="double.NaN"/> otherwise.
        /// </param>
        /// <returns><code>true</code> if measurement was not skipped, otherwise <code>false</code>.</returns>
        public bool TryReadGasResistance(out double gasResistance)
        {
            if (!ReadGasMeasurementIsValid() || !ReadHeaterIsStable())
            {
                gasResistance = double.NaN;
                return false;
            }

            // Read 10 bit gas resistance value from registers
            var gasResRaw = Read8BitsFromRegister((byte)Bme680Register.GAS_RES);
            var gasRange = Read8BitsFromRegister((byte)Bme680Register.GAS_RANGE);
            
            var gasRes = (ushort)((ushort)(gasResRaw << 2) + (byte)(gasRange >> 6));
            gasRange &= (byte)Bme680Mask.GAS_RANGE;

            gasResistance = CalculateGasResistance(gasRes, gasRange);
            return true;
        }

        /// <summary>
        /// Sets the default configuration for the sensor.
        /// </summary>
        protected override void SetDefaultConfiguration()
        {
            base.SetDefaultConfiguration();
            HumiditySampling = Sampling.UltraLowPower;
            FilterMode = Bme680FilteringMode.C0;

            _bme680Calibration = (Bme680CalibrationData)_calibrationData;
            TryReadTemperature(out var temp);
            ConfigureHeatingProfile(Bme680HeaterProfile.Profile1, 320, 150, temp.Celsius);
            HeaterProfile = Bme680HeaterProfile.Profile1;

            HeaterIsEnabled = true;
            GasConversionIsEnabled = true;
        }

        /// <summary>
        /// Compensates the humidity.
        /// </summary>
        /// <param name="adcHumidity">The humidity value read from the device.</param>
        /// <returns>The percentage relative humidity.</returns>
        private double CompensateHumidity(int adcHumidity)
        {
            // Calculate the humidity.
            var temperature = TemperatureFine / 5120.0;
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
        private Pressure CompensatePressure(long adcPressure)
        {
            // Calculate the pressure.
            var var1 = (TemperatureFine / 2.0) - 64000.0;
            var var2 = var1 * var1 * (_bme680Calibration.DigP6 / 131072.0);
            var2 += (var1 * _bme680Calibration.DigP5 * 2.0);
            var2 = (var2 / 4.0) + (_bme680Calibration.DigP4 * 65536.0);
            var1 = ((_bme680Calibration.DigP3 * var1 * var1 / 16384.0) + (_bme680Calibration.DigP2 * var1)) / 524288.0;
            var1 = (1.0 + (var1 / 32768.0)) * _bme680Calibration.DigP1;
            var calculatedPressure = 1048576.0 - adcPressure;

            // Avoid exception caused by division by zero.
            if (var1 != 0)
            {
                calculatedPressure = (calculatedPressure - (var2 / 4096.0)) * 6250.0 / var1;
                var1 = _bme680Calibration.DigP9 * calculatedPressure * calculatedPressure / 2147483648.0;
                var2 = calculatedPressure * (_bme680Calibration.DigP8 / 32768.0);
                var var3 = (calculatedPressure / 256.0) * (calculatedPressure / 256.0) * (calculatedPressure / 256.0)
                    * (_bme680Calibration.DigP10 / 131072.0);
                calculatedPressure += (var1 + var2 + var3 + (_bme680Calibration.DigP7 * 128.0)) / 16.0;
            }
            else
            {
                calculatedPressure = 0;
            }

            return Pressure.FromPascal(calculatedPressure);
        }

        private bool ReadGasMeasurementIsValid()
        {
            var gasMeasValid = Read8BitsFromRegister((byte)Bme680Register.GAS_RANGE);
            gasMeasValid = (byte)((gasMeasValid & (byte)Bme680Mask.GAS_VALID) >> 5);

            return Convert.ToBoolean(gasMeasValid);
        }

        private double CalculateGasResistance(ushort adcGasRes, byte gasRange)
        {
            var var1 = 1340.0 + 5.0 * _bme680Calibration.RangeSwErr;
            var var2 = var1 * (1.0 + s_k1Lookup[gasRange] / 100.0);
            var var3 = 1.0 + s_k2Lookup[gasRange] / 100.0;
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

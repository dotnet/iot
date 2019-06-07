// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//The device binding is ported from the [Bosch C library](https://github.com/BoschSensortec/BME680_driver) and adds some new functionality.
// Formulas and code examples can also be found in the datasheet https://ae-bst.resource.bosch.com/media/_tech/media/datasheets/BST-BME680-DS001.pdf

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading.Tasks;
using Iot.Units;

namespace Iot.Device.Bme680
{
    public class Bme680 : IDisposable
    {

        private I2cDevice _i2cDevice;
        private SpiDevice _spiDevice;
        private readonly CommunicationProtocol _protocol;

        private readonly CalibrationData _calibrationData;
        private int _temperatureFine;
        private bool _initialized;

        // The ChipId of the BME680
        private const byte DeviceId = 0x61;

        /// <summary>
        /// Gets the last measured temperature data from the corresponding register.
        /// </summary>
        public Temperature Temperature => ReadTemperature();

        /// <summary>
        /// Gets the last measured relative humidity in percent from the corresponding register.
        /// </summary>
        public double Humidity => ReadHumidity();

        /// <summary>
        /// Gets the last measured pressure in Pa from the corresponding register.
        /// </summary>
        public double Pressure => ReadPressure();

        /// <summary>
        /// Gets the last measured gas resistance in Ohm from the corresponding register.
        /// </summary>
        public double GasResistance => ReadGasResistance();

        /// <summary>
        /// Gets or sets whether the heater is enabled.
        /// </summary>
        public bool HeaterIsEnabled
        {
            get
            {
                var heaterStatus = Read8BitsFromRegister((byte)Register.CTRL_GAS_0);
                heaterStatus = (byte)((heaterStatus & (byte)Mask.HEAT_OFF) >> 3);
                return Convert.ToBoolean(heaterStatus);
            }
            set
            {
                var heaterStatus = Read8BitsFromRegister((byte)Register.CTRL_GAS_0);
                heaterStatus = (byte)(heaterStatus & ((byte)Mask.HEAT_OFF ^ (byte)Mask.CLR));
                heaterStatus = (byte)(heaterStatus | Convert.ToUInt32(value) << 4);

                Write8BitsToRegister((byte)Register.CTRL_GAS_0, heaterStatus);
            }
        }

        /// <summary>
        /// Gets or sets whether gas conversions are enabled.
        /// </summary>
        public bool GasConversionIsEnabled
        {
            get
            {
                var gasConversion = Read8BitsFromRegister((byte)Register.CTRL_GAS_1);
                gasConversion = (byte)((gasConversion & (byte)Mask.RUN_GAS) >> 4);
                return Convert.ToBoolean(gasConversion);
            }
            set
            {
                var gasConversion = Read8BitsFromRegister((byte)Register.CTRL_GAS_1);
                gasConversion = (byte)(gasConversion & ((byte)Mask.RUN_GAS ^ (byte)Mask.CLR));
                gasConversion = (byte)(gasConversion | Convert.ToUInt32(value) << 4);

                Write8BitsToRegister((byte)Register.CTRL_GAS_1, gasConversion);
            }
        }

        /// <summary>
        /// Indicates whether new data is available.
        /// </summary>
        public bool NewDataIsAvailable
        {
            get
            {
                var newData = Read8BitsFromRegister((byte)Register.MEAS_STATUS_0);
                newData = (byte)(newData >> 7);
                return Convert.ToBoolean(newData);
            }
        }

        /// <summary>
        /// Indicates whether a gas measurement is in process.
        /// </summary>
        public bool GasMeasurementInProcess
        {
            get
            {
                var gasMeasInProcess = Read8BitsFromRegister((byte)Register.MEAS_STATUS_0);
                gasMeasInProcess = (byte)((gasMeasInProcess & (byte)Mask.GAS_MEASURING) >> 6);
                return Convert.ToBoolean(gasMeasInProcess);
            }
        }

        /// <summary>
        /// Indicates whether a measurement is in process.
        /// </summary>
        public bool MeasurementInProcess
        {
            get
            {
                var measInProcess = Read8BitsFromRegister((byte)Register.MEAS_STATUS_0);
                measInProcess = (byte)((measInProcess & (byte)Mask.MEASURING) >> 5);
                return Convert.ToBoolean(measInProcess);
            }
        }

        /// <summary>
        /// Indicates whether a real gas conversion was performed (i.e. not a dummy one).
        /// </summary>
        public bool GasMeasurementIsValid
        {
            get
            {
                var gasMeasValid = Read8BitsFromRegister((byte)Register.GAS_R_LSB);
                gasMeasValid = (byte)((gasMeasValid & (byte)Mask.GAS_VALID) >> 5);
                return Convert.ToBoolean(gasMeasValid);
            }
        }

        /// <summary>
        /// Indicates whether the target heater temperature was reached.
        /// </summary>
        public bool HeaterIsStable
        {
            get
            {
                var heaterStable = Read8BitsFromRegister((byte)Register.GAS_R_LSB);
                heaterStable = (byte)((heaterStable & (byte)Mask.HEAT_STAB) >> 4);
                return Convert.ToBoolean(heaterStable);
            }
        }

        /// <summary>
        /// Gets or sets the heater profile to be used for the next measurement.
        /// </summary>
        public HeaterProfile CurrentHeaterProfile
        {
            get
            {
                var heaterProfile = Read8BitsFromRegister((byte)Register.CTRL_GAS_1);
                heaterProfile = (byte)(heaterProfile & (byte)Mask.NB_CONV);
                return (HeaterProfile)heaterProfile;
            }
            set
            {
                var heaterProfile = Read8BitsFromRegister((byte)Register.CTRL_GAS_1);
                heaterProfile = (byte)(heaterProfile & ((byte)Mask.NB_CONV ^ (byte)Mask.CLR));
                heaterProfile = (byte)(heaterProfile | (byte)value);

                Write8BitsToRegister((byte)Register.CTRL_GAS_1, heaterProfile);
            }
        }

        /// <summary>
        /// Gets or sets the IIR filter to the given coefficient.
        /// <para />
        /// The IIR filter affects temperature and pressure measurements but not humidity and gas measurements. 
        /// <para />
        /// An IIR filter can suppress disturbances (e.g. slamming of a door or wind blowing in the sensor).
        /// </summary>
        public FilterCoefficient FilterCoefficient
        {
            get
            {
                var filter = Read8BitsFromRegister((byte)Register.CONFIG);
                filter = (byte)((filter & (byte)Mask.FILTER_COEFFICIENT) >> 2);
                return (FilterCoefficient)filter;
            }
            set
            {
                var filter = Read8BitsFromRegister((byte)Register.CONFIG);
                filter = (byte)(filter & ((byte)Mask.FILTER_COEFFICIENT ^ (byte)Mask.CLR));
                filter = (byte)(filter | (byte)value << 2);

                Write8BitsToRegister((byte)Register.CONFIG, filter);
            }
        }

        /// <summary>
        /// Gets or sets the temperature sampling.
        /// </summary>
        public Sampling TemperatureSampling
        {
            get
            {
                var status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
                status = (byte)((status & (byte)Mask.TEMPERATURE_SAMPLING) >> 5);
                return ByteToSampling(status);
            }
            set
            {
                var status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
                status = (byte)(status & ((byte)Mask.TEMPERATURE_SAMPLING ^ (byte)Mask.CLR));
                status = (byte)(status | (byte)value << 5);

                Write8BitsToRegister((byte)Register.CTRL_MEAS, status);
            }
        }

        /// <summary>
        /// Gets or sets the humidity sampling.
        /// </summary>
        public Sampling HumiditySampling
        {
            get
            {
                var status = Read8BitsFromRegister((byte)Register.CTRL_HUM);
                status = (byte)(status & (byte)Mask.HUMIDITY_SAMPLING);
                return ByteToSampling(status);
            }
            set
            {
                var status = Read8BitsFromRegister((byte)Register.CTRL_HUM);
                status = (byte)(status & ((byte)Mask.HUMIDITY_SAMPLING ^ (byte)Mask.CLR));
                status = (byte)(status | (byte)value);

                Write8BitsToRegister((byte)Register.CTRL_HUM, status);
            }
        }

        /// <summary>
        /// Gets or sets the pressure sampling.
        /// </summary>
        public Sampling PressureSampling
        {
            get
            {
                var status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
                status = (byte)((status & (byte)Mask.PRESSURE_SAMPLING) >> 2);
                return ByteToSampling(status);
            }
            set
            {
                var status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
                status = (byte)(status & ((byte)Mask.PRESSURE_SAMPLING ^ (byte)Mask.CLR));
                status = (byte)(status | (byte)value << 2);

                Write8BitsToRegister((byte)Register.CTRL_MEAS, status);
            }
        }

        private Sampling ByteToSampling(byte value)
        {
            // Values >=5 equals X16
            if (value >= 5)
                return Sampling.X16;

            return (Sampling)value;
        }

        public Bme680(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _calibrationData = new CalibrationData();
            _protocol = CommunicationProtocol.I2C;
        }

        private Bme680(SpiDevice spiDevice)
        {
            // not fully implemented yet, translation of memory addresses and memory page access missing
            throw new NotImplementedException();

            _spiDevice = spiDevice;
            _calibrationData = new CalibrationData();
            _protocol = CommunicationProtocol.Spi;
        }

        public enum CommunicationProtocol
        {
            I2C,
            Spi
        }

        /// <summary>
        /// Initializes the BMP680 sensor, making it ready for use.
        /// </summary>
        public void InitDevice()
        {
            var readSignature = Read8BitsFromRegister((byte)Register.CHIP_ID);

            if (readSignature != DeviceId)
                throw new Exception($"Device ID {readSignature} is not the same as expected {DeviceId}. Please check if you are using the right device.");

            _initialized = true;
            _calibrationData.ReadFromDevice(this);

            // perform a single temperature reading to set the temperature fine and set the default configuration
            TemperatureSampling = Sampling.X1;
            PressureSampling = Sampling.Skipped;
            HumiditySampling = Sampling.Skipped;
            FilterCoefficient = FilterCoefficient.C0;
            GasConversionIsEnabled = false;

            PerformMeasurement().Wait();

            // turn on humidity, pressure and gas conversion for future measurements
            HumiditySampling = Sampling.X1;
            PressureSampling = Sampling.X1;
            GasConversionIsEnabled = true;
            SaveHeaterProfileToDevice(HeaterProfile.Profile1, 320, 150, Temperature.Celsius);
            CurrentHeaterProfile = HeaterProfile.Profile1;
        }

        /// <summary>
        /// Triggers a soft reset on the device which has the same effect as power-on reset.
        /// </summary>
        public void TriggerSoftReset()
        {
            // TODO: do we need a delay after resetting? test read directly after reset
            Write8BitsToRegister((byte)Register.RESET, 0xB6);
            _initialized = false;
        }

        /// <summary>
        /// Performs a measurement by setting the sensor to forced mode, awaits the result.
        /// </summary>
        /// <returns></returns>
        public async Task PerformMeasurement()
        {
            var duration = GetProfileDuration(CurrentHeaterProfile);
            SetPowerMode(PowerMode.Forced);

            if (GasConversionIsEnabled)
                HeaterIsEnabled = true;

            await Task.Delay(duration);
        }

        /// <summary>
        /// Sets the power mode to the given mode.
        /// </summary>
        /// <param name="powerMode"></param>
        public void SetPowerMode(PowerMode powerMode)
        {
            if (!_initialized)
                InitDevice();

            var status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
            status = (byte)(status & ((byte)Mask.PWR_MODE ^ (byte)Mask.CLR));
            status = (byte)(status | (byte)powerMode);

            Write8BitsToRegister((byte)Register.CTRL_MEAS, status);
        }

        /// <summary>
        /// Reads the current power mode the device is running in.
        /// </summary>
        /// <returns></returns>
        public PowerMode ReadPowerMode()
        {
            var status = Read8BitsFromRegister((byte)Register.CTRL_MEAS);
            status = (byte)(status & (byte)Mask.PWR_MODE);

            return (PowerMode)status;
        }

        // TODO: why is time for gas measurement always added, what happens if conversion is disabled to 477*5???
        // TODO: compare with real values (check NewDataIsAvailable for measurement time)
        /// <summary>
        /// Gets the required time in ms to perform a measurement with the given heater profile.
        /// The precision of this duration is within 1ms of the actual measurement time
        /// </summary>
        /// <param name="profile">The heater profile.</param>
        /// <returns></returns>
        public int GetProfileDuration(HeaterProfile profile)
        {
            var osToMeasCycles = new byte[] { 0, 1, 2, 4, 8, 16 };
            var osToSwitchCount = new byte[] {0, 1, 1, 1, 1, 1};

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

            if (GasConversionIsEnabled)
                measDuration += GetHeaterProfileFromDevice(profile).GetHeaterDurationInMilliseconds();

            return (int)Math.Ceiling(measDuration);
        }

        /// <summary>
        /// Sets a gas-sensor-heater profile. Converts the target temperature and duration to internally used format.
        /// </summary>
        /// <param name="profile">The selected profile to save to.</param>
        /// <param name="targetTemperature">The target temperature in °C. Ranging from 0-400.</param>
        /// <param name="duration">The duration in ms. Ranging from 0-4032.</param>
        /// <param name="ambientTemperature">The ambient temperature.</param>
        /// <returns></returns>
        public HeaterProfileConfiguration SaveHeaterProfileToDevice(HeaterProfile profile, ushort targetTemperature, ushort duration, double ambientTemperature)
        {
            // read ambient temperature for resistance calculation
            var heaterResistance = CalculateHeaterResistance(targetTemperature, (short)ambientTemperature);
            var heaterDuration = CalculateHeaterDuration(duration);

            Write8BitsToRegister((byte)((byte)Register.RES_HEAT0 + profile), heaterResistance);
            Write8BitsToRegister((byte)((byte)Register.GAS_WAIT0 + profile), heaterDuration);

            return new HeaterProfileConfiguration(profile, heaterResistance, heaterDuration);
        }

        /// <summary>
        /// Gets the specified gas-sensor-heater configuration.
        /// </summary>
        /// <param name="profile">The chosen heater profile.</param>
        /// <returns>The configuration of the chosen set-point or null if invalid set-point was chosen.</returns>
        public HeaterProfileConfiguration GetHeaterProfileFromDevice(HeaterProfile profile)
        {
            // need to be converted?!
            var heaterTemp = Read8BitsFromRegister((byte)((byte)Register.RES_HEAT0 + profile));
            var heaterDuration = Read8BitsFromRegister((byte)((byte)Register.GAS_WAIT0 + profile));

            return new HeaterProfileConfiguration(profile, heaterTemp, heaterDuration);
        }

        /// <summary>
        /// Reads the temperature from the sensor.
        /// </summary>
        /// <returns>Temperature</returns>
        private Temperature ReadTemperature()
        {
            if (TemperatureSampling == Sampling.Skipped)
                return Temperature.FromCelsius(double.NaN);

            // Read 20 bit uncompensated temperature value from registers
            var t1 = Read16BitsFromRegister((byte)Register.TEMP, Endianness.BigEndian);
            var t2 = Read8BitsFromRegister((byte)Register.TEMP + 2 * sizeof(byte));

            // Combine the two values, t2 is only 4 bit
            var temp = (uint)(t1 << 4) + (uint)(t2 >> 4);

            return CalculateTemperature(temp);
        }

        /// <summary>
        ///  Reads the pressure from the sensor.
        /// </summary>
        /// <returns>Atmospheric pressure in Pa.</returns>
        private double ReadPressure()
        {
            if (_temperatureFine == int.MinValue)
                return double.NaN;

            if (PressureSampling == Sampling.Skipped)
                return double.NaN;

            // Read 20 bit uncompensated pressure value from registers
            var p1 = Read16BitsFromRegister((byte)Register.PRESS, Endianness.BigEndian);
            var p2 = Read8BitsFromRegister((byte)Register.PRESS + 2 * sizeof(byte));

            // Combine the two values, p2 is only 4 bit
            var press = (uint)(p1 << 4) + (uint)(p2 >> 4);

            return CalculatePressure(press);
        }

        /// <summary>
        /// Reads the humidity from the sensor.
        /// </summary>
        /// <returns>Humidity in percent.</returns>
        private double ReadHumidity()
        {
            if (_temperatureFine == int.MinValue)
                return int.MinValue;

            if (HumiditySampling == Sampling.Skipped)
                return double.NaN;

            // Read 16 bit uncompensated humidity value from registers
            var hum = Read16BitsFromRegister((byte)Register.HUM, Endianness.BigEndian);

            return CalculateHumidity(hum);
        }

        /// <summary>
        /// Reads the gas resistance from the sensor.
        /// </summary>
        /// <returns>Gas resistance in Ohm.</returns>
        private double ReadGasResistance()
        {
            if (!GasMeasurementIsValid)
            {
                return double.NaN;
            }

            // Read 10 bit gas resistance value from registers
            var g1 = Read8BitsFromRegister((byte)Register.GAS_RES);
            var g2 = Read8BitsFromRegister((byte)Register.GAS_RES + sizeof(byte));
            var gasRange = Read8BitsFromRegister((byte)Register.GAS_RANGE);

            var gasResistance = (ushort)((ushort)(g1 << 2) + (byte)(g2 >> 6));
            gasRange &= (byte)Mask.GAS_RANGE;

            return CalculateGasResistance(gasResistance, gasRange);
        }

        /// <summary>
        ///  Calculates the temperature in °C.
        /// </summary>
        /// <param name="adcTemperature">The temperature value read from the device.</param>
        /// <returns>Temperature</returns>
        private Temperature CalculateTemperature(uint adcTemperature)
        {
            var var1 = (adcTemperature / 16384.0 - _calibrationData.ParT1 / 1024.0) * _calibrationData.ParT2;
            var var2 = adcTemperature / 131072.0 - _calibrationData.ParT1 / 8192.0;
            var2 = var2 * var2 * (_calibrationData.ParT3 * 16.0);

            _temperatureFine = (int)(var1 + var2);
            var temperature = (var1 + var2) / 5120.0;

            return Temperature.FromCelsius(temperature);
        }

        /// <summary>
        ///  Returns the pressure in Pa.
        /// </summary>
        /// <param name="adcPressure">The pressure value read from the device.</param>
        /// <returns>Pressure in Pa</returns>
        private double CalculatePressure(uint adcPressure)
        {
            var var1 = _temperatureFine / 2.0 - 64000.0;
            var var2 = Math.Pow(var1, 2) * (_calibrationData.ParP6 / 131072.0);
            var2 += var1 * _calibrationData.ParP5 * 2.0;
            var2 = var2 / 4.0 + _calibrationData.ParP4 * 65536.0;
            var1 = (_calibrationData.ParP3 * Math.Pow(var1, 2) / 16384.0 + _calibrationData.ParP2 * var1) / 524288.0;
            var1 = (1.0 + var1 / 32768.0) * _calibrationData.ParP1;
            var pressure = 1048576.0 - adcPressure;

            if (var1 == 0)
                return 0;

            pressure = (pressure - var2 / 4096.0) * 6250.0 / var1;
            var1 = _calibrationData.ParP9 * Math.Pow(pressure, 2) / 2147483648.0;
            var2 = pressure * (_calibrationData.ParP8 / 32768.0);
            var var3 = Math.Pow(pressure / 256.0, 3) * (_calibrationData.ParP10 / 131072.0);
            pressure += (var1 + var2 + var3 + _calibrationData.ParP7 * 128.0) / 16.0;

            return pressure;
        }

        private double CalculateHumidity(ushort adcHumidity)
        {
            var tempComp = _temperatureFine / 5120.0;
            var var1 = adcHumidity - (_calibrationData.ParH1 * 16.0 + _calibrationData.ParH3 / 2.0 * tempComp);
            var var2 = var1 * (float)(_calibrationData.ParH2 / 262144.0 * (1.0 + _calibrationData.ParH4 / 16384.0 * tempComp + _calibrationData.ParH5 / 1048576.0 * Math.Pow(tempComp, 2)));
            var var3 = _calibrationData.ParH6 / 16384.0;
            var var4 = _calibrationData.ParH7 / 2097152.0;
            var humidity = var2 + (var3 + var4 * tempComp) * var2 * var2;

            // limit possible value range
            if (humidity > 100.0)
                humidity = 100.0;
            else if (humidity < 0.0)
                humidity = 0.0;

            return humidity;
        }

        private double CalculateGasResistance(ushort adcGasRes, byte gasRange)
        {
            var k1Lookup = new[] { 0.0, 0.0, 0.0, 0.0, 0.0, -1.0, 0.0, -0.8, 0.0, 0.0, -0.2, -0.5, 0.0, -1.0, 0.0, 0.0 };
            var k2Lookup = new[] { 0.0, 0.0, 0.0, 0.0, 0.1, 0.7, 0.0, -0.8, -0.1, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

            var var1 = 1340.0 + 5.0 * _calibrationData.RangeSwErr;
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

            var var1 = _calibrationData.ParGh1 / 16.0 + 49.0;
            var var2 = _calibrationData.ParGh2 / 32768.0 * 0.0005 + 0.00235;
            var var3 = _calibrationData.ParGh3 / 1024.0;
            var var4 = var1 * (1.0 + var2 * setTemp);
            var var5 = var4 + var3 * ambientTemp;
            var heaterResistance = (byte)(3.4 * (var5 * (4.0 / (4.0 + _calibrationData.ResHeatRange)) * (1.0 / (1.0 + _calibrationData.ResHeatVal * 0.002)) - 25));

            return heaterResistance;
        }


        // The duration is interpreted as follows:
        // Byte [7:6]: multiplication factor of 1 ,4, 16 or 64
        // Byte [5:0] 64 timer values, 1ms step size
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

        internal byte Read8BitsFromRegister(byte register)
        {
            byte value;

            switch (_protocol)
            {
                case CommunicationProtocol.I2C:
                    _i2cDevice.WriteByte(register);
                    value = _i2cDevice.ReadByte();
                    return value;

                case CommunicationProtocol.Spi:
                    _spiDevice.WriteByte(register);
                    value = _spiDevice.ReadByte();
                    return value;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal ushort Read16BitsFromRegister(byte register, Endianness mode = Endianness.LittleEndian)
        {
            Span<byte> bytes = stackalloc byte[2];

            switch (_protocol)
            {
                case CommunicationProtocol.I2C:
                    _i2cDevice.WriteByte(register);
                    _i2cDevice.Read(bytes);
                    break;

                case CommunicationProtocol.Spi:
                    _spiDevice.WriteByte(register);
                    _spiDevice.Read(bytes);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (mode)
            {
                case Endianness.LittleEndian:
                    return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
                case Endianness.BigEndian:
                    return BinaryPrimitives.ReadUInt16BigEndian(bytes);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private void Write8BitsToRegister(byte register, byte data)
        {
            switch (_protocol)
            {
                case CommunicationProtocol.I2C:
                    _i2cDevice.Write(new[] { register, data });
                    break;

                case CommunicationProtocol.Spi:
                    _spiDevice.Write(new[] { register, data });
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal enum Endianness
        {
            LittleEndian,
            BigEndian
        }

        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice.Dispose();
                _i2cDevice = null;
            }

            if (_spiDevice != null)
            {
                _spiDevice.Dispose();
                _spiDevice = null;
            }
        }
    }
}

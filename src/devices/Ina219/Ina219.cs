// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.Diagnostics.CodeAnalysis;
using UnitsNet;

namespace Iot.Device.Adc
{
    /// <summary>
    /// Binding that exposes an INA219 Bidirectional Current/Power monitor.
    ///
    /// The INA219 is a high-side current shunt and power monitor with an I2C interface.
    /// The INA219 monitors both shunt drop and supply voltage, with programmable conversion
    /// times and filtering. A programmable calibration value, combined with an internal multiplier,
    /// enables direct readouts in amperes. An additional multiplying register calculates power in watts.
    /// <see href="http://www.ti.com/lit/ds/symlink/ina219.pdf"/>
    /// </summary>
    public class Ina219 : IDisposable
    {
        private I2cDevice _i2cDevice;
        private bool _disposeI2cDevice = false;
        private ushort _calibrationValue;
        private Ina219AdcResolutionOrSamples _busAdcResSamp;
        private Ina219AdcResolutionOrSamples _shuntAdcResSamp;
        private float _currentLsb;

        // These values are the datasheet defined delays in micro seconds between requesting a Current or Power value from the INA219 and the ADC sampling having completed
        // along with any conversions.
        private static readonly Dictionary<Ina219AdcResolutionOrSamples, int> s_readDelays = new()
        {
            { Ina219AdcResolutionOrSamples.Adc9Bit, 84 },
            { Ina219AdcResolutionOrSamples.Adc10Bit, 148 },
            { Ina219AdcResolutionOrSamples.Adc11Bit, 276 },
            { Ina219AdcResolutionOrSamples.Adc12Bit, 532 },
            { Ina219AdcResolutionOrSamples.Adc2Sample, 1006 },
            { Ina219AdcResolutionOrSamples.Adc4Sample, 2130 },
            { Ina219AdcResolutionOrSamples.Adc8Sample, 4260 },
            { Ina219AdcResolutionOrSamples.Adc16Sample, 8510 },
            { Ina219AdcResolutionOrSamples.Adc32Sample, 17020 },
            { Ina219AdcResolutionOrSamples.Adc64Sample, 34050 },
            { Ina219AdcResolutionOrSamples.Adc128Sample, 68100 }
        };

        /// <summary>
        /// Construct an Ina219 device using an I2cDevice
        /// </summary>
        /// <remarks>
        /// This binding does not dispose the passed in I2cDevice.
        /// </remarks>
        /// <param name="i2cDevice">The I2cDevice initialized to communicate with the INA219.</param>
        public Ina219(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _currentLsb = 1F;
        }

        /// <summary>
        /// Construct an INA219 device using an I2cConnectionSettings.
        /// </summary>
        /// <remarks>
        /// This binding creates an I2cDevice ufor communication with the INA219. The I2cDevice is disposed when then INA219 is disposed.
        /// </remarks>
        /// <param name="settings">The I2cConnectionSettings object initialized with the appropriate settings to communicate with the INA219.</param>
        public Ina219(I2cConnectionSettings settings)
            : this(I2cDevice.Create(settings)) => _disposeI2cDevice = true;

        /// <summary>
        /// Reset the INA219 to default values;
        /// </summary>
        public void Reset()
        {
            // Reset the device by sending a value to the configuration register with the reset but set.
            WriteRegister(Ina219Register.Configuration, (ushort)Ina219ConfigurationFlags.Rst);

            _currentLsb = 1F;
            _calibrationValue = 0;

            // cache the values for the Bus and Shunt ADC settings so that they can be used later without reading them from the INA219 device
            _busAdcResSamp = BusAdcResolutionOrSamples;
            _shuntAdcResSamp = ShuntAdcResolutionOrSamples;
        }

        /// <summary>
        /// Property representing the Operating mode of the INA219
        /// </summary>
        /// <remarks>
        /// This allows the user to selects continuous, triggered, or power-down mode of operation along with which of the shunt and bus voltage measurements are made.
        /// </remarks>
        public Ina219OperatingMode OperatingMode
        {
            get => (Ina219OperatingMode)(ReadRegister(Ina219Register.Configuration) & (ushort)Ina219ConfigurationFlags.ModeMask);
            set
            {
                ushort regValue = ReadRegister(Ina219Register.Configuration);

                regValue &= (ushort)~Ina219ConfigurationFlags.ModeMask;
                regValue |= (ushort)value;

                WriteRegister(Ina219Register.Configuration, regValue);
            }
        }

        /// <summary>
        /// Property representing the Bus voltage range of the INA219
        /// </summary>
        /// <remarks>
        /// This allows the user to selects eiter a 16V range or a 32V range for the ADC reading the bus voltage.
        /// In general the lowest range compatible with the application parameters should be selected.
        /// </remarks>
        public Ina219BusVoltageRange BusVoltageRange
        {
            get => (Ina219BusVoltageRange)(ReadRegister(Ina219Register.Configuration) & (ushort)Ina219ConfigurationFlags.BrngMask);
            set
            {
                ushort regValue = ReadRegister(Ina219Register.Configuration);

                regValue &= (ushort)~Ina219ConfigurationFlags.BrngMask;
                regValue |= (ushort)value;

                WriteRegister(Ina219Register.Configuration, regValue);
            }
        }

        /// <summary>
        /// Property representing the voltage range of the Programable Gain Amplifier used in the INA219 to measure Shunt Voltage.
        /// </summary>
        /// <remarks>
        /// This allows the user to selects a gain for the amplifier reading the shunt voltage before it is applied to the ADC. It can be one of +/-40mV, +/-80mV, +/-160mV or +/-320mV.
        /// In general the lowest range compatible with the application parameters should be selected.
        /// </remarks>
        public Ina219PgaSensitivity PgaSensitivity
        {
            get => (Ina219PgaSensitivity)(ReadRegister(Ina219Register.Configuration) & (ushort)Ina219ConfigurationFlags.PgaMask);
            set
            {
                ushort regValue = ReadRegister(Ina219Register.Configuration);

                regValue &= (ushort)~Ina219ConfigurationFlags.PgaMask;
                regValue |= (ushort)value;

                WriteRegister(Ina219Register.Configuration, regValue);
            }
        }

        /// <summary>
        /// Set the Ina219 ADC resolution or samples to be used when reading the Bus voltage.
        /// </summary>
        /// <remarks>
        /// This can either by the number of bits used for the ADC conversion (9-12 bits) or the number of samples at 12 bits to be averaged for the result.
        /// </remarks>
        public Ina219AdcResolutionOrSamples BusAdcResolutionOrSamples
        {
            get => (Ina219AdcResolutionOrSamples)((ReadRegister(Ina219Register.Configuration) & (ushort)Ina219ConfigurationFlags.BadcMask) >> 4);
            set
            {
                ushort regValue = ReadRegister(Ina219Register.Configuration);

                regValue &= (ushort)~Ina219ConfigurationFlags.BadcMask;
                regValue |= (ushort)((ushort)value << 4);

                WriteRegister(Ina219Register.Configuration, regValue);

                _busAdcResSamp = value;
            }
        }

        /// <summary>
        /// Set the INA219 ADC resolution or samples to be used when reading the Shunt voltage.
        /// </summary>
        /// <remarks>
        /// This can either by the number of bits used for the ADC conversion (9-12 bits) or the number of samples at 12 bits to be averaged for the result.
        /// </remarks>
        public Ina219AdcResolutionOrSamples ShuntAdcResolutionOrSamples
        {
            get => (Ina219AdcResolutionOrSamples)(ReadRegister(Ina219Register.Configuration) & (ushort)Ina219ConfigurationFlags.SadcMask);
            set
            {
                ushort regValue = ReadRegister(Ina219Register.Configuration);

                regValue &= (ushort)~Ina219ConfigurationFlags.SadcMask;
                regValue |= (ushort)((ushort)value << 4);

                WriteRegister(Ina219Register.Configuration, regValue);

                _shuntAdcResSamp = value;
            }
        }

        /// <summary>
        /// Set the INA219 calibration value used to scale the Shunt voltage into a Current reading.
        /// </summary>
        /// <remarks>
        /// This method allows the user to manually specify the value written to the INA219 calibration register which determines how the shunt voltage
        /// reading is scaled into the current register by the INA219. To allow finer control of the scaling the current register does not contain the actual
        /// current value and the currentLsb is used to specify how much current in Amperes is represented by the least significant bit of the current register.
        /// This will allow the ReadCurrent method to return the corrent current value and by implication ReadPower to return the correct power value as it is derived froom
        /// the current value.
        /// <see cref="ReadPower"/><see cref="ReadCurrent"/><seealso href="http://www.ti.com/lit/ds/symlink/ina219.pdf"/>
        /// </remarks>
        /// <param name="calibrationValue">The number of Amperes represented by the LSB of the INA219 current register.</param>
        /// <param name="currentLsb">The current value in Amperes of the least significan bit of the calibration register. Defaults to unity so that the register can be read directly.</param>
        public void SetCalibration(ushort calibrationValue, float currentLsb = 1.0F)
        {
            // cache the values for later use
            _calibrationValue = calibrationValue;
            _currentLsb = currentLsb;

            // set the INA219 calibration value
            WriteRegister(Ina219Register.Calibration, calibrationValue);
        }

        /// <summary>
        /// Dispose instance
        /// </summary>
        public void Dispose()
        {
            if (_disposeI2cDevice)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null!;
            }
        }

        /// <summary>
        /// Read the measured shunt voltage.
        /// </summary>
        /// <returns>The shunt potential difference</returns>
        // read the shunt voltage. LSB = 10uV then convert to Volts
        public ElectricPotential ReadShuntVoltage() => ElectricPotential.FromVolts(ReadRegister(Ina219Register.ShuntVoltage, s_readDelays[(Ina219AdcResolutionOrSamples)_shuntAdcResSamp]) * 10.0 / 1000000.0);

        /// <summary>
        /// Read the measured Bus voltage.
        /// </summary>
        /// <returns>The Bus potential (voltage)</returns>
        // read the bus voltage. LSB = 4mV then convert to Volts
        public ElectricPotential ReadBusVoltage() => ElectricPotential.FromVolts(((short)ReadRegister(Ina219Register.BusVoltage, s_readDelays[_busAdcResSamp]) >> 3) * 4 / 1000.0);

        /// <summary>
        /// Read the calculated current through the INA219.
        /// </summary>
        /// <remarks>
        /// This value is determined by an internal calculation using the calibration register and the read shunt voltage and then scaled.
        /// </remarks>
        /// <returns>The calculated current</returns>
        public ElectricCurrent ReadCurrent()
        {
            // According to Adafruit then large changes in load will reset the cal register
            // meaning that the current and power values will be unavailable.
            // to work around this the calibration value is cached and set
            // whenever needed.
            SetCalibration(_calibrationValue, _currentLsb);

            return ElectricCurrent.FromAmperes(ReadRegister(Ina219Register.Current, s_readDelays[(Ina219AdcResolutionOrSamples)_shuntAdcResSamp]) * _currentLsb);
        }

        /// <summary>
        /// Get the calculated power in the circuit being monitored by the INA219.
        /// </summary>
        /// <remarks>
        /// This value is determined by an internal calculation using the calulated current and the read bus voltage and then scaled.
        /// </remarks>
        /// <returns>The calculated electric power</returns>
        public Power ReadPower()
        {
            // According to Adafruit then large changes in load will reset the cal register
            // meaning that the current and power values will be unavailable.
            // to work around this the calibration value is cached and set
            // whenever needed.
            SetCalibration(_calibrationValue, _currentLsb);

            return Power.FromWatts(ReadRegister(Ina219Register.Power, s_readDelays[(Ina219AdcResolutionOrSamples)_shuntAdcResSamp]) * _currentLsb * 20);
        }

        /// <summary>
        /// Read a register from the INA219 device
        /// </summary>
        /// <param name="register">The register to read.</param>
        /// <param name="delayMicroSeconds">A delay between setting the register to be read and the actual read. Defaults to 0</param>
        /// <returns>Am unsiged short integer representing the regsiter contents.</returns>
        private ushort ReadRegister(Ina219Register register, int delayMicroSeconds = 0)
        {
            Span<byte> buffer = stackalloc byte[2];

            // set a value in the buffer representing the register that we want to read and send it to the INA219
            buffer[0] = (byte)register;
            _i2cDevice.Write(buffer.Slice(0, 1));

            // wait for any sampling, average or conversion.
            if (delayMicroSeconds > 0)
            {
                DelayHelper.DelayMicroseconds(delayMicroSeconds, true);
            }

            // read the register back from the INA219.
            _i2cDevice.Read(buffer);

            // massage the big endian value read from the INA219 unto a ushort.
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        /// <summary>
        /// Write a value to an INA219 register.
        /// </summary>
        /// <param name="register">The register to be written to.</param>
        /// <param name="value">The value to be writtent to the register.</param>
        private void WriteRegister(Ina219Register register, ushort value)
        {
            Span<byte> buffer = stackalloc byte[3];

            // set the first byte of the buffer to the register to be writtent
            buffer[0] = (byte)register;

            // write the value to be written to the second and third bytes in big-endian order.
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(1, 2), value);

            // write the value to the register via the I2c Bus.
            _i2cDevice.Write(buffer);
        }
    }
}

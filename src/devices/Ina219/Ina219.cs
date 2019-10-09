// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;

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
    public partial class Ina219 : IDisposable
    {
        private readonly Dictionary<PgaSensitivity, float> _sensitivityVoltage = new Dictionary<PgaSensitivity, float>() { {PgaSensitivity.PlusOrMinus40mv, 0.04F }, { PgaSensitivity.PlusOrMinus80mv, 0.08F }, { PgaSensitivity.PlusOrMinus160mv, 0.16F }, { PgaSensitivity.PlusOrMinus320mv, 0.32F } };
        private readonly Dictionary<AdcResolutionOrSamples, int> _readDelays = new Dictionary<AdcResolutionOrSamples, int>() { { AdcResolutionOrSamples.Adc9Bit, 84 }, { AdcResolutionOrSamples.Adc10Bit, 148 }, { AdcResolutionOrSamples.Adc11Bit, 276 }, { AdcResolutionOrSamples.Adc12Bit, 532 },
          {AdcResolutionOrSamples.Adc2Sample, 1006}, {AdcResolutionOrSamples.Adc4Sample, 2130}, {AdcResolutionOrSamples.Adc8Sample, 4260}, {AdcResolutionOrSamples.Adc16Sample, 8510}, {AdcResolutionOrSamples.Adc32Sample, 17020}, {AdcResolutionOrSamples.Adc64Sample, 34050}, {AdcResolutionOrSamples.Adc128Sample, 68100}};
        private I2cDevice _i2cDevice;
        private bool _disposeI2cDevice = false;
        private ushort _calibrationValue;
        private AdcResolutionOrSamples _busAdcResSamp;
        private AdcResolutionOrSamples _shuntAdcResSamp;

        /// <summary>
        /// The CurrentLsb property specifies the electrical current in amperes respresented by the least significant bit of the INA219 Current register.
        /// It is used by the GetCurrent method to calculate the current from the value exposed by the current register
        /// <see cref="GetCurrent"/>
        /// </summary>
        /// <remarks>
        /// This property defaults to 1. It is set by calling SetCalibration with values for the ADC sensitivity, current range and optionally shunt resistance.
        /// If the value of the calibration register is set manually then this property will also need to be set for GetCurrent to return the correct value.
        /// <see cref="SetCalibration(PgaSensitivity, float, float)"/> <seealso cref="SetCalibration(ushort)"/>
        /// </remarks>
        public float CurrentLsb { get; set; }

        /// <summary>
        /// The PowerLsb property specifies the electrical power in watts respresented by the least significant bit of the INA219 Power register.
        /// It is used by the GetPower method to calculate the power from the value exposed by the Power register
        /// <see cref="GetCurrent"/>
        /// </summary>
        /// <remarks>
        /// This property defaults to 1. It is set by calling SetCalibration with values for the ADC sensitivity, current range and optionally shunt resistance.
        /// If the value of the calibration register is set manually then this property will also need to be set for GetPower to return the correct value.
        /// <see cref="SetCalibration(PgaSensitivity, float, float)"/> <seealso cref="SetCalibration(ushort)"/>
        /// </remarks>
        public float PowerLsb { get; set; }

        /// <summary>
        /// Construct an Ina219 device using an I2cDevice
        /// </summary>
        /// <remarks>
        /// This binding does not dispose the passed in I2cDevice.
        /// </remarks>
        /// <param name="i2cDevice">The I2cDevice initialized to communicate with the Ina219.</param>
        public Ina219(I2cDevice i2cDevice)
        {
            if (i2cDevice == null)
            {
                throw new System.ArgumentNullException(nameof(i2cDevice));
            }

            PowerLsb = CurrentLsb = 1F;

            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Construct an Ina219 device using an I2cConnectionSettings.
        /// </summary>
        /// <remarks>
        /// This binding creates an I2cDevice ufor communication with the Ina219. The I2cDevice is disposed when then Ina219 is disposed.
        /// </remarks>
        /// <param name="settings">The I2cConnectionSettings object initialized with the appropiate settings to communicate with the Ina219.</param>
        public Ina219(I2cConnectionSettings settings)
        {
            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            PowerLsb = CurrentLsb = 1F;
            _calibrationValue = 0;

            _disposeI2cDevice = true;
            _i2cDevice = I2cDevice.Create(settings);
        }

        /// <summary>
        /// Reset the Ina219 to default values;
        /// </summary>
        public void Reset()
        {
            ushort regValue;

            // Reset the device by sending a value to the configuration register with the reset but set.
            WriteRegister(Register.Configuration, (ushort)ConfigurationFlags.Rst);

            // Read back the values from the configuration register which should now be default.
            regValue = ReadRegister(Register.Configuration, 1000);

            PowerLsb = PowerLsb = 1F;
            _calibrationValue = 0;

            // Get the current values for the Bus and Shunt ADC settings.
            _busAdcResSamp = (AdcResolutionOrSamples)((regValue & (ushort)ConfigurationFlags.BadcMask) >> 4);
            _shuntAdcResSamp = (AdcResolutionOrSamples)(regValue & (ushort)ConfigurationFlags.SadcMask);
        }

        /// <summary>
        /// Set the Ina219 operating mode.
        /// </summary>
        /// <param name="mode">The operating mode to switch to.</param>
        public void SetOperatingMode(OperatingMode mode)
        {
            ushort regValue = ReadRegister(Register.Configuration);

            regValue &= (ushort)ConfigurationFlags.ModeMask ^ 0xFFFF;
            regValue |= (ushort)mode;

            WriteRegister(Register.Configuration, regValue);
        }

        /// <summary>
        /// Set the Ina219 Bus voltage range.
        /// </summary>
        /// <param name="range">The voltage range to switch to.</param>
        public void SetBusVoltageRange(BusVoltageRange range)
        {
            ushort regValue = ReadRegister(Register.Configuration);

            regValue &= (ushort)ConfigurationFlags.BrngMask ^ 0xFFFF;
            regValue |= (ushort)range;

            WriteRegister(Register.Configuration, regValue);
        }

        /// <summary>
        /// Set the Ina219 Programable Gain Amplifier range.
        /// </summary>
        /// <remarks>
        /// This is the voltage range used to read the shunt voltage. It can be one of +/-40mV, +/-80mV, +/-160mV or +/-320mV.
        /// </remarks>
        /// <param name="sensitivity">The voltage range of the Programable Gain Amplifier to switch to.</param>
        public void SetPGARange(PgaSensitivity sensitivity)
        {
            ushort regValue = ReadRegister(Register.Configuration);

            regValue &= (ushort)ConfigurationFlags.PgaMask ^ 0xFFFF;
            regValue |= (ushort)sensitivity;

            WriteRegister(Register.Configuration, regValue);
        }

        /// <summary>
        /// Set the Ina219 ADC resolution or samples to be used when reading the Bus voltage.
        /// </summary>
        /// <remarks>
        /// This can either by the number of bits used for the ADC conversion (9-12 bits) or the number of samples at 12 bits to be averaged for the result.
        /// </remarks>
        /// <param name="resorsamp">The ADC resolution or sample count to switch to.</param>
        public void SetBusAdcResolutionOrSamples(AdcResolutionOrSamples resorsamp)
        {
            ushort regValue = ReadRegister(Register.Configuration);

            regValue &= (ushort)ConfigurationFlags.BadcMask ^ 0xFFFF;
            regValue |= (ushort)((ushort)resorsamp << 4);

            WriteRegister(Register.Configuration, regValue);

            _busAdcResSamp = resorsamp;
        }

        /// <summary>
        /// Set the Ina219 ADC resolution or samples to be used when reading the Shunt voltage.
        /// </summary>
        /// <remarks>
        /// This can either by the number of bits used for the ADC conversion (9-12 bits) or the number of samples at 12 bits to be averaged for the result.
        /// </remarks>
        /// <param name="resorsamp">The ADC resolution or sample count to switch to.</param>
        public void SetShuntAdcResolutionOrSamples(AdcResolutionOrSamples resorsamp)
        {
            ushort regValue = ReadRegister(Register.Configuration);

            regValue &= (ushort)ConfigurationFlags.SadcMask ^ 0xFFFF;
            regValue |= (ushort)resorsamp;

            WriteRegister(Register.Configuration, regValue);

            _shuntAdcResSamp = resorsamp;
        }

        /// <summary>
        /// Set the Ina219 calibration value used to scale the Shunt voltage into a Current reading.
        /// </summary>
        /// <remarks>
        /// If this method is used then the values for the CurrentLsb and PowerMsb will have to be manually determined
        /// to ensure accurate readings from GetPower and GetCurrent Methods.
        /// <see cref="GetPower"/><seealso cref="GetCurrent"/><seealso cref="PowerLsb"/><seealso cref="CurrentLsb"/>
        /// </remarks>
        /// <param name="calibrationValue">The calibration value to switch to.</param>
        public void SetCalibration(ushort calibrationValue)
        {
            _calibrationValue = calibrationValue;
            WriteRegister(Register.Calibration, calibrationValue);
        }

        /// <summary>
        /// Set the Ina219 calibration values used to scale the Shunt voltage into a Current reading based on the sensitivity of the
        /// Progmamable Gain Amplifier, the required full scale current and the value of the shunt resistor.
        /// </summary>
        /// <remarks>
        /// This method sets the calibration register and also the PowerLsb and CurrentLsb properties.
        /// <see cref="PowerLsb"/><seealso cref="CurrentLsb"/>
        /// </remarks>
        /// <param name="sensitivity">The sensitivity of the shunt Programable Gain Amplifier</param>
        /// <param name="fsCurrent">The required full scale current value</param>
        /// <param name="shuntResistance">The value of the shunt resistor. This defaults to 0.1 Ohms as used in the Adafruit breakout board.</param>
        public void SetCalibration(PgaSensitivity sensitivity, float fsCurrent, float shuntResistance = 0.1F)
        {
            int i;
            float minCurrentLsb = fsCurrent / 32767;
            float maxCurrentLsb = fsCurrent / 4096;
            float currentStep;

            if (fsCurrent > _sensitivityVoltage[sensitivity] / shuntResistance)
            {
                throw new ArgumentOutOfRangeException(nameof(fsCurrent), "Current range too large for given shunt resistance and amplifier sensitivity.");
            }

            // find a step by taking a value in the same range as the maxCurrentLSB
            currentStep = (float)Math.Pow(10, (int)Math.Log10(maxCurrentLsb) - 1);

            // step through rounded values of currentLsb to find the best LSB for the current.
            i = 1;
            while (i * currentStep < minCurrentLsb) { i++; };
            CurrentLsb = i * currentStep;
            PowerLsb = CurrentLsb * 20;

            SetPGARange(sensitivity);
            SetCalibration((ushort)(0.04096F / (CurrentLsb * shuntResistance)));
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposeI2cDevice & disposing)
            {
                _i2cDevice?.Dispose();
            }
        }

        /// <summary>
        /// Dispose of managed assets.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Get the measured shunt voltage.
        /// </summary>
        /// <returns>The shunt voltage in Volts</returns>
        public float GetShuntVoltage()
        {
            // read the shunt voltage. LSB = 10uV then convert to Volts
            return (short)ReadRegister(Register.ShuntVoltage, _readDelays[(AdcResolutionOrSamples)_shuntAdcResSamp]) * 10 / 1000000F;
        }

        /// <summary>
        /// Get the measured Bus voltage.
        /// </summary>
        /// <returns>The Bus voltage in Volts</returns>
        public float GetBusVoltage()
        {
            // read the bus voltage. LSB = 4mV then convert to Volts
            return ((short)ReadRegister(Register.BusVoltage, _readDelays[_busAdcResSamp]) >> 3) * 4 / 1000F;
        }

        /// <summary>
        /// Get the calculated current through the INA219.
        /// </summary>
        /// <remarks>
        /// This value is determined by an internal calculation using the calibration register and the read shunt voltage and then scaled
        /// using the CurrentLsb property.
        /// <see cref="CurrentLsb"/>
        /// </remarks>
        /// <returns>The calculated current in Amperes</returns>
        public float GetCurrent()
        {
            // According to Adafruit then large changes in load will reset the cal register
            // meaning that the current and power values will be unavailable.
            // to work around this the calibration value is cached and set
            // whenever needed.
            SetCalibration(_calibrationValue);

            return (float)(short)ReadRegister(Register.Current, _readDelays[(AdcResolutionOrSamples)_shuntAdcResSamp]) * CurrentLsb;
        }

        /// <summary>
        /// Get the calculated power in the circuit being monitored by the INA219.
        /// </summary>
        /// <remarks>
        /// This value is determined by an internal calculation using the calulated current and the read bus voltage and then scaled
        /// using the PowerLsb property.
        /// <see cref="PowerLsb"/>
        /// </remarks>
        /// <returns>The calculated power in Watts</returns>
        public float GetPower()
        {
            // According to Adafruit then large changes in load will reset the cal register
            // meaning that the current and power values will be unavailable.
            // to work around this the calibration value is cached and set
            // whenever needed.
            SetCalibration(_calibrationValue);

            return (float)ReadRegister(Register.Power, _readDelays[(AdcResolutionOrSamples)_shuntAdcResSamp]) * PowerLsb;
        }

        /// <summary>
        /// Read a register from the INA219 device
        /// </summary>
        /// <param name="register">The register to read.</param>
        /// <param name="delayMicroSeconds">A delay between setting the register to be read and the actual read. Defaults to 0</param>
        /// <returns>Am unsiged short integer representing the regsiter contents.</returns>
        protected ushort ReadRegister(Register register, int delayMicroSeconds = 0)
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
        protected void WriteRegister(Register register, ushort value)
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

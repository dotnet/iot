// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device;
using System.Device.I2c;
using System.Device.Model;
using UnitsNet;

namespace Iot.Device.Ina226
{
    /// <summary>
    /// INA226 Bidirectional Current/Power monitor.
    ///
    /// The INA226 is a high-side/low-side current shunt and power monitor with an i2c compatible
    /// interface. The device monitors both a shunt voltage drop and bus supply voltage.
    /// Programmable calibration value, conversion times, and averaging, combined with an
    /// internal multiplier, enable direct readouts of current in ampers an power in watts.
    /// <see href="https://www.ti.com/lit/ds/symlink/ina226.pdf"/>
    /// </summary>
    [Interface("INA226 Bidirectional Current/Power monitor")]
    public class Ina226 : IDisposable
    {
        private I2cDevice _i2cDevice;
        private bool _disposeI2cDevice = false;

        // cache for commonly used configuration values
        private ushort _calibrationValue;
        private Ina226SamplesAveraged _samplesAveraged;
        private Ina226BusVoltageConvTime _busConvTime;
        private Ina226ShuntConvTime _shuntConvTime;
        private int _busConvTimeInt;
        private int _shuntConvTimeInt;
        private int _samplesAveragedInt;
        private double _currentLsb;

        /// <summary>
        /// Construct an Ina226 device using an I2cDevice
        /// </summary>
        /// <remarks>
        /// This binding does not dispose the passed in I2cDevice.
        /// </remarks>
        /// <param name="i2cDevice">The I2cDevice initialized to communicate with the INA226</param>
        public Ina226(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _currentLsb = 1F;
        }

        /// <summary>
        /// Construct an INA226 device useing I2cConnectionSettings passed in.
        /// </summary>
        /// <remarks>
        /// This binding creates an I2cDevice for communication with the INA226. This I2cDevice is disposed when the INA226 is disposed.
        /// </remarks>
        /// <param name="settings">The I2cConnectionSettings object initialized with the appropriate settings to communicate with the INA226</param>
        public Ina226(I2cConnectionSettings settings)
            : this(I2cDevice.Create(settings)) => _disposeI2cDevice = true;

        /// <summary>
        /// Dispose of instance
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
        /// Reset the INA226 to default values.
        /// </summary>
        [Command]
        public void Reset()
        {
            // Reset the device by sending a value to the configuartion register with the reset bit set.
            WriteRegister(Ina226Register.Configuration, (ushort)Ina226ConfigurationFlags.Reset);

            _currentLsb = 1F;
            _calibrationValue = 0;

            // cache the values for the Bus, Shunt, and Samples Averaged settings so that they can be used later without reading them from the INA226 device.
            _samplesAveraged = SamplesAveraged;
            _samplesAveragedInt = int.Parse(_samplesAveraged.ToString().Replace("Quantity_", string.Empty));

            _busConvTime = BusConvTime;
            _busConvTimeInt = int.Parse(_busConvTime.ToString().Replace("Time", string.Empty).Replace("us", string.Empty));

            _shuntConvTime = ShuntConvTime;
            _shuntConvTimeInt = int.Parse(_shuntConvTime.ToString().Replace("Time", string.Empty).Replace("us", string.Empty));
        }

        /// <summary>
        /// Set the INA226 Calibration value used to scale the shunt voltage into a current reading
        /// </summary>
        /// <remarks>
        /// This method allows the user to manually specify the value written to the INA226 calibration register which determines how the shunt voltage
        /// reading is scaled into the current register by the INA226.  To allow finer control of the scaling the current register does not contain the actual
        /// current value and the currentLsb is used to specify how much current in Amperes is represented by the least significant bit of the current register.
        /// </remarks>
        /// <param name="rShunt">Value of shunt in ohms. a 100amp 75mv shunt would be 0.00075 for example.</param>
        /// <param name="maxExpectedCurrent">Maximum current (amps) expected when the voltage difference is 81.92mv, the INA226 full scale voltage.</param>
        [Command]
        public void SetCalibration(double rShunt, double maxExpectedCurrent)
        {
            _currentLsb = (maxExpectedCurrent / 3.2768e+4); // Math.Pow(2, 15) = 3.2768e+4
            _calibrationValue = Convert.ToUInt16(0.00512 / (_currentLsb * rShunt)); // 0.00512 is given in the datasheet
            WriteRegister(Ina226Register.Calibration, _calibrationValue);
        }

        /// <summary>
        /// Function used to determine if a specific bit is set in a ushort register string
        /// </summary>
        /// <param name="bits">ushort read from a register on the INA226.</param>
        /// <param name="pos">What position the bit to check is at.</param>
        /// <returns>true if the bit is set and false if it is not.</returns>
        internal static bool IsBitSet(ushort bits, int pos)
        {
            return (bits & (1 << pos)) != 0;
        }

        /// <summary>
        /// Read the measured shunt voltage
        /// </summary>
        /// <returns>The shunt potential difference</returns>
        [Telemetry("ShuntVoltage")]
        public ElectricPotential ReadShuntVoltage()
        {
            ushort regValue = ReadRegister(Ina226Register.ShuntVoltage, _shuntConvTimeInt * _samplesAveragedInt);

            if ((regValue & 0x8000) > 0)
            {
                regValue &= 0x7FFF;
                regValue ^= 0x7FFF;
                regValue++;

                return ElectricPotential.FromMicrovolts(regValue * -10.0);
            }

            return ElectricPotential.FromMicrovolts(regValue * 10.0);
        }

        /// <summary>
        /// Read the measured Bus Voltage
        /// </summary>
        /// <returns>The Bus potential (voltage)</returns>
        [Telemetry("BusVoltage")]
        public ElectricPotential ReadBusVoltage() => ElectricPotential.FromMillivolts((short)ReadRegister(Ina226Register.BusVoltage, _busConvTimeInt * _samplesAveragedInt) * 1.25);

        /// <summary>
        /// Read the calculated current through the INA226
        /// </summary>
        /// <remarks>
        /// This value is determined by an internal calculation using the calibration register and the read shunt voltage and then scaled.
        /// </remarks>
        /// <returns>The calculated current</returns>
        [Telemetry("Current")]
        public ElectricCurrent ReadCurrent()
        {
            ushort regValue = ReadRegister(Ina226Register.Current, _shuntConvTimeInt * _samplesAveragedInt);
            if ((regValue & 0x8000) > 0)
            {
                regValue &= 0x7FFF;
                regValue ^= 0x7FFF;
                regValue++;

                return ElectricCurrent.FromAmperes(regValue * -_currentLsb);
            }

            return ElectricCurrent.FromAmperes(regValue * _currentLsb);
        }

        /// <summary>
        /// Get the calculated power of the circuit being monitored by the INA226
        /// </summary>
        /// <remarks>
        /// This value is determined by an internal calculation using the calculated current and the read bus voltage and then scaled.
        /// </remarks>
        /// <returns>The calculated electric power</returns>
        [Telemetry("Power")]
        public Power ReadPower()
        {
            return Power.FromWatts(ReadRegister(Ina226Register.Power, _shuntConvTimeInt * _samplesAveragedInt) * _currentLsb * 25);
        }

        /// <summary>
        /// Get the status of the AlertFunctionFlag bit on the INA226
        /// </summary>
        /// <returns>
        /// True if flag is set and false if not
        /// </returns>
        [Telemetry("AlertFunctionFlag")]
        public bool AlertFunctionFlag()
        {
            return IsBitSet((byte)(ReadRegister(Ina226Register.AlertEnable) & (ushort)(Ina226AlertMask.AlertFunctionFlag)), 4);
        }

        /// <summary>
        /// Get the status of the Conversion Ready Flag bit on the INA226
        /// </summary>
        /// <returns>
        /// True if flag is set and false if not
        /// </returns>
        [Telemetry("ConversionReadyFlag")]
        public bool ConversionReadyFlag()
        {
            return IsBitSet((byte)(ReadRegister(Ina226Register.AlertEnable) & (ushort)(Ina226AlertMask.AlertConvReadyFlag)), 3);
        }

        /// <summary>
        /// Get the status of the Math Overflow Flag bit on the INA226
        /// </summary>
        /// <returns>
        /// True if flag is set and false if not
        /// </returns>
        [Telemetry("MathOverflowFlag")]
        public bool MathOverflowFlag()
        {
            return IsBitSet((byte)(ReadRegister(Ina226Register.AlertEnable) & (ushort)(Ina226AlertMask.AlertMathOverflowFlag)), 2);
        }

        /// <summary>
        /// Property representing the Operating mode of the INA226
        /// </summary>
        /// <remarks>
        /// This allows the user to select continuous, triggered, or power-down mode of operation along with which of the shunt and bus voltage measurements are made.
        /// </remarks>
        [Property]
        public Ina226OperatingMode OperatingMode
        {
            get => (Ina226OperatingMode)(ReadRegister(Ina226Register.Configuration) & (ushort)Ina226ConfigurationFlags.ModeMask);
            set
            {
                ushort regValue = ReadRegister(Ina226Register.Configuration);

                regValue &= (ushort)~Ina226ConfigurationFlags.ModeMask;
                regValue |= (ushort)value;

                WriteRegister(Ina226Register.Configuration, regValue);
            }
        }

        /// <summary>
        /// INA226 Number of samples to average property
        /// </summary>
        /// <remarks>
        /// This property is used to select how many samples the INA226 should average before writing to each voltage register.
        /// </remarks>
        [Property]
        public Ina226SamplesAveraged SamplesAveraged
        {
            get => (Ina226SamplesAveraged)(ReadRegister(Ina226Register.Configuration) & (ushort)Ina226ConfigurationFlags.SamplesAvgMask);
            set
            {
                ushort regValue = ReadRegister(Ina226Register.Configuration);

                regValue &= (ushort)~Ina226ConfigurationFlags.SamplesAvgMask;
                regValue |= (ushort)value;

                WriteRegister(Ina226Register.Configuration, regValue);

                _samplesAveraged = value;
                _samplesAveragedInt = int.Parse(value.ToString().Replace("Quantity_", string.Empty));
            }
        }

        /// <summary>
        /// INA226 Conversion time for Bus voltage samples property
        /// </summary>
        /// <remarks>
        /// This property is used to select the conversion time for each sample of the Bus Voltage
        /// </remarks>
        [Property]
        public Ina226BusVoltageConvTime BusConvTime
        {
            get => (Ina226BusVoltageConvTime)(ReadRegister(Ina226Register.Configuration) & (ushort)Ina226ConfigurationFlags.BusConvMask);
            set
            {
                ushort regValue = ReadRegister(Ina226Register.Configuration);

                regValue &= (ushort)~Ina226ConfigurationFlags.BusConvMask;
                regValue |= (ushort)value;

                WriteRegister(Ina226Register.Configuration, regValue);

                _busConvTime = value;
                _busConvTimeInt = int.Parse(value.ToString().Replace("Time", string.Empty).Replace("us", string.Empty));
            }
        }

        /// <summary>
        /// INA226 Conversion time for Shunt voltage samples property
        /// </summary>
        /// <remarks>
        /// This property is used to select the conversion time for each sample of the Shunt Voltage
        /// </remarks>
        [Property]
        public Ina226ShuntConvTime ShuntConvTime
        {
            get => (Ina226ShuntConvTime)(ReadRegister(Ina226Register.Configuration) & (ushort)Ina226ConfigurationFlags.ShuntConvMask);
            set
            {
                ushort regValue = ReadRegister(Ina226Register.Configuration);

                regValue &= (ushort)~Ina226ConfigurationFlags.ShuntConvMask;
                regValue |= (ushort)value;

                WriteRegister(Ina226Register.Configuration, regValue);

                _shuntConvTime = value;
                _shuntConvTimeInt = int.Parse(value.ToString().Replace("Time", string.Empty).Replace("us", string.Empty));
            }
        }

        /// <summary>
        /// INA226 Alert Limit Trigger Register when using bus voltage specific alert modes
        /// </summary>
        /// <remarks>
        /// This property is used to set the alert trigger limit used when alert mode is set to enabled on bus voltage over or under limit modes.
        /// </remarks>
        [Property]
        public ElectricPotential AlertLimitVoltage
        {
            get => ElectricPotential.FromMillivolts((short)ReadRegister(Ina226Register.AlertLimit) * 1.25);
            set
            {
                var regValue = (value.Volts / 0.00125);
                if (regValue > 36)
                {
                    regValue = 36;
                }

                if (regValue < 0)
                {
                    regValue = 0;
                }

                ushort voltage = Convert.ToUInt16(regValue);
                WriteRegister(Ina226Register.AlertLimit, voltage);
            }
        }

        /// <summary>
        /// INA226 Alert Limit Trigger Register when using shunt voltage specific alert modes
        /// </summary>
        [Property]
        public ElectricCurrent AlertLimitCurrent
        {
            get => ElectricCurrent.FromMilliamperes((short)ReadRegister(Ina226Register.AlertLimit) * 2.50);
            set
            {
                var regValue = (value.Amperes / 0.00250);

                // Needs input validation here
                ushort current = Convert.ToUInt16(regValue);
                WriteRegister(Ina226Register.AlertLimit, current);
            }
        }

        /// <summary>
        /// INA226 Alert Limit Trigger Register when using power specific alert modes
        /// </summary>
        [Property]
        public Power AlertLimitPower
        {
            get => Power.FromWatts((short)ReadRegister(Ina226Register.AlertLimit) * _currentLsb * 25);
            set
            {
                var regValue = ((value.Watts / 25) / _currentLsb);

                // Needs input validation here
                ushort current = Convert.ToUInt16(regValue);
                WriteRegister(Ina226Register.AlertLimit, current);
            }
        }

        /// <summary>
        /// INA226 Alert Mode
        /// </summary>
        /// <remarks>
        /// This property is used to set the alert trigger mode.
        /// </remarks>
        [Property]
        public Ina226AlertMode AlertMode
        {
            get => (Ina226AlertMode)(ReadRegister(Ina226Register.AlertEnable) & (ushort)Ina226AlertMask.AlertModeMask);
            set
            {
                var regValue = ReadRegister(Ina226Register.AlertEnable);

                regValue &= (ushort)~Ina226AlertMask.AlertModeMask;
                regValue |= (ushort)value;

                WriteRegister(Ina226Register.AlertEnable, regValue);
            }
        }

        /// <summary>
        /// INA226 Alert Pin Polarity
        /// </summary>
        /// <remarks>
        /// This property is used to set the Alert Pin polarity, true = inverted (active-high open collector) and false = normal (active-low open collector)(default)
        /// </remarks>
        [Property]
        public bool AlertPolarity
        {
            get => IsBitSet((byte)(ReadRegister(Ina226Register.AlertEnable) & (ushort)Ina226AlertMask.AlertPolarityMask), 1);
            set
            {
                var regValue = ReadRegister(Ina226Register.AlertEnable);

                regValue &= Convert.ToUInt16(~Ina226AlertMask.AlertPolarityMask);
                regValue |= Convert.ToUInt16(value);

                WriteRegister(Ina226Register.AlertEnable, regValue);
            }
        }

        /// <summary>
        /// INA226 Alert Latch Enable
        /// </summary>
        /// <remarks>
        /// This property is used to enable alert latching, which will hold the triggered alert until the alert register has been read. Read the datasheet for further details on functionality.
        /// </remarks>
        [Property]
        public bool AlertLatchEnable
        {
            get => IsBitSet((byte)(ReadRegister(Ina226Register.AlertEnable) & (ushort)Ina226AlertMask.AlertLatchMask), 0);
            set
            {
                var regValue = ReadRegister(Ina226Register.AlertEnable);

                regValue &= Convert.ToUInt16(~Ina226AlertMask.AlertLatchMask);
                regValue |= Convert.ToUInt16(value);

                WriteRegister(Ina226Register.AlertEnable, regValue);
            }
        }

        /// <summary>
        /// Read a register from the INA226 Device
        /// </summary>
        /// <param name="register">The register to read.</param>
        /// <param name="delayMicroSeconds">A delay between setting the register to be read and the actual read. Defaults to 0</param>
        /// <returns>A ushort integer representinig the register contents.</returns>
        private ushort ReadRegister(Ina226Register register, int delayMicroSeconds = 0)
        {
            Span<byte> buffer = stackalloc byte[2];

            // Set a value in the buffer representing the register that we want to read and send it to the INA226
            buffer[0] = (byte)register;
            _i2cDevice.Write(buffer.Slice(0, 1));

            // Wait for any sampling, average or conversion.
            if (delayMicroSeconds > 0)
            {
                Ina226DelayHelper.DelayMicroseconds(delayMicroSeconds, true);
            }

            // Read the register back from the INA226.
            _i2cDevice.Read(buffer);

            // Convert and return BigEndian value read from INA226 as a ushort
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        /// <summary>
        /// Write a value to an INA226 register.
        /// </summary>
        /// <param name="register">The register to be written to.</param>
        /// <param name="value">The value to be written to the register.</param>
        private void WriteRegister(Ina226Register register, ushort value)
        {
            Span<byte> buffer = stackalloc byte[3];

            // Set the first byte of the buffer to the register to be written.
            buffer[0] = (byte)register;

            // Write the value to be written to the second and third bytes in big-endian order.
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(1, 2), value);

            // Write the value to the register via the I2c Bus.
            _i2cDevice.Write(buffer);
        }
    }
}

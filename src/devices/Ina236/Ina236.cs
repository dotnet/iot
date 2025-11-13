// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.Device.Model;
using UnitsNet;
using static System.Math;

namespace Iot.Device.Adc
{
    /// <summary>
    /// INA236 Bidirectional Current/Power monitor.
    ///
    /// The INA236 current shunt and power monitor with an I2C interface.
    /// The INA236 monitors both shunt drop and supply voltage, with programmable conversion
    /// times and filtering. A programmable calibration value, combined with an internal multiplier,
    /// enables direct readouts in amperes. An additional multiplying register calculates power in watts.
    /// <see href="http://www.ti.com/lit/ds/symlink/ina236.pdf"/>
    /// </summary>
    [Interface("INA236 Bidirectional Current/Power monitor")]
    public class Ina236 : IDisposable
    {
        /// <summary>
        /// The default I2C Address for this device.
        /// According to the datasheet, the device comes in two variants, A and B.
        /// Type A has addresses 0x80 to 0x83, depending on whether the ADDR pin is connected to GND, VS, SDA or SCL.
        /// Type B has addresses 0x90 to 0x94, depending on the ADDR pin.
        /// </summary>
        public const int DefaultI2cAddress = 0x80;
        private I2cDevice _i2cDevice;
        private ElectricResistance _shuntResistance;
        private ElectricCurrent _currentLsb;
        private ElectricCurrent _maxCurrent;
        private ElectricPotential _voltageLsb;

        /// <summary>
        /// Construct an Ina236 device using an I2cDevice
        /// </summary>
        /// <param name="i2cDevice">The I2cDevice initialized to communicate with the INA236.</param>
        /// <param name="maxCurrent">Maximum expected current. Typical values are 8-10 Amps.</param>
        /// <param name="shuntResistance">The resistance of the shunt between input and output.
        /// Example breakout boards from manufacturers such as Adafruit have 0.008 Ohms, so try this
        /// value if you are unsure.</param>
        /// <remarks>The power dissipation at the resistor is P = I*I*R at the maximum current.</remarks>
        public Ina236(I2cDevice i2cDevice, ElectricResistance shuntResistance, ElectricCurrent maxCurrent)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _shuntResistance = shuntResistance;
            if (_shuntResistance <= ElectricResistance.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(shuntResistance), "The shuntResistance parameter must be greater than zero");
            }

            _maxCurrent = maxCurrent;

            Reset(_shuntResistance, _maxCurrent);
        }

        /// <summary>
        /// Reset the INA236 to default values;
        /// </summary>
        [Command]
        public void Reset(ElectricResistance shuntResistance, ElectricCurrent current)
        {
            // Reset the device by sending a value to the configuration register with the reset bit set.
            WriteRegister(Ina236Register.Configuration, 0x8000);

            ushort deviceId = ReadRegisterUnsigned(Ina236Register.DeviceId);

            if ((deviceId & 0xFFF0) != 0xa080)
            {
                throw new InvalidOperationException($"The device on I2C address {_i2cDevice.ConnectionSettings.DeviceAddress} doesn't seem to be an INA236. Device ID was {deviceId:X4} instead of 0xA080");
            }

            // See datasheet. Use twice the value to allow later rounding
            var currentLsbMinimum = current / Pow(2.0, 15) * 2;
            double exactCalibrationValue = 0.00512 / (currentLsbMinimum.Amperes * shuntResistance.Ohms);
            int valueToSet = (int)(exactCalibrationValue * 1E6); // the value must be set in uA
            if (valueToSet > 0xFFFF)
            {
                throw new InvalidOperationException("Invalid combination of settings - the calibration value is out of spec");
            }

            // Reverse calculation, to get the exact lsb
            double exactLsbMinimum = valueToSet / 0.00512 / shuntResistance.Ohms;

            _currentLsb = ElectricCurrent.FromAmperes(exactLsbMinimum);

            // The LSB of the shunt voltage register is 2.5uV if ADCRANGE==0, otherwise it's 625nV. We currently
            // do not support ADCRANGE=1, to keep things simple.
            _voltageLsb = ElectricPotential.FromMicrovolts(2.5);
            WriteRegister(Ina236Register.Calibration, (ushort)valueToSet);
        }

        /// <summary>
        /// Property representing the Operating mode of the INA236
        /// </summary>
        /// <remarks>
        /// This allows the user to selects continuous, triggered, or power-down mode of operation along with which of the shunt and bus voltage measurements are made.
        /// </remarks>
        [Property]
        public Ina236OperatingMode OperatingMode
        {
            get
            {
                return (Ina236OperatingMode)(ReadRegisterUnsigned(Ina236Register.Configuration) & (ushort)Ina236OperatingMode.ModeMask);
            }
            set
            {
                int regValue = ReadRegisterUnsigned(Ina236Register.Configuration);

                regValue &= ~0b111;
                regValue |= (int)value;

                WriteRegister(Ina236Register.Configuration, (ushort)regValue);
            }
        }

        /// <summary>
        /// Dispose instance
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        /// <summary>
        /// Read the measured shunt voltage.
        /// </summary>
        /// <returns>The shunt potential difference</returns>
        /// <remarks>The LSB is 2.5uV when ADCRANGE=0</remarks>
        [Telemetry("ShuntVoltage")]
        public ElectricPotential ReadShuntVoltage()
        {
            return ReadRegisterUnsigned(Ina236Register.ShuntVoltage) * _voltageLsb;
        }

        /// <summary>
        /// Read the measured Bus voltage.
        /// This is the voltage on the primary side of the shunt.
        /// </summary>
        /// <returns>The Bus potential (voltage)</returns>
        /// <remarks>The LSB is 1.6mV.</remarks>
        [Telemetry("BusVoltage")]
        public ElectricPotential ReadBusVoltage()
        {
            return ReadRegisterUnsigned(Ina236Register.BusVoltage) * ElectricPotential.FromMillivolts(1.6);
        }

        /// <summary>
        /// Read the calculated current through the INA236.
        /// </summary>
        /// <remarks>
        /// This value is determined by an internal calculation using the calibration register and the read shunt voltage and then scaled.
        /// The value can be negative, when power flows to the bus.
        /// </remarks>
        /// <returns>The calculated current</returns>
        [Telemetry("Current")]
        public ElectricCurrent ReadCurrent()
        {
            return ReadRegisterSigned(Ina236Register.Current) * _currentLsb;
        }

        /// <summary>
        /// Reads the current power consumed by the attached device.
        /// </summary>
        /// <returns>The power being used</returns>
        /// <remarks>Clarify whether this register is signed or unsigned. Since it is the product of the current and the bus voltage
        /// registers, it should be possible to get a negative value, but the documentation says it's always positive</remarks>
        [Telemetry("Power")]
        public Power ReadPower()
        {
            return Power.FromWatts(ReadRegisterUnsigned(Ina236Register.Power) * _currentLsb.Amperes * 32);
        }

        /// <summary>
        /// Read a register from the INA236 device
        /// </summary>
        /// <param name="register">The register to read.</param>
        /// <returns>Am unsiged short integer representing the regsiter contents.</returns>
        private ushort ReadRegisterUnsigned(Ina236Register register)
        {
            Span<byte> buffer = stackalloc byte[2];

            byte registerNumber = (byte)register;
            // set a value in the buffer representing the register that we want to read and send it to the INA219
            _i2cDevice.WriteRead(new ReadOnlySpan<byte>(ref registerNumber), buffer);

            // read the register back from the INA219.
            _i2cDevice.Read(buffer);

            // massage the big endian value read from the INA219 unto a ushort.
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        /// <summary>
        /// Read a register from the INA236 device
        /// </summary>
        /// <param name="register">The register to read.</param>
        /// <returns>A signed short integer representing the regsiter contents.</returns>
        private short ReadRegisterSigned(Ina236Register register)
        {
            Span<byte> buffer = stackalloc byte[2];

            byte registerNumber = (byte)register;
            // set a value in the buffer representing the register that we want to read and send it to the INA219
            _i2cDevice.WriteRead(new ReadOnlySpan<byte>(ref registerNumber), buffer);

            // read the register back from the INA219.
            _i2cDevice.Read(buffer);

            // massage the big endian value read from the INA219 unto a ushort.
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }

        /// <summary>
        /// Write a value to an INA236 register.
        /// </summary>
        /// <param name="register">The register to be written to.</param>
        /// <param name="value">The value to be written to the register.</param>
        private void WriteRegister(Ina236Register register, ushort value)
        {
            Span<byte> buffer = stackalloc byte[3];

            // set the first byte of the buffer to the register to be written
            buffer[0] = (byte)register;

            // write the value to be written to the second and third bytes in big-endian order.
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(1, 2), value);

            // write the value to the register via the I2c Bus.
            _i2cDevice.Write(buffer);
        }
    }
}

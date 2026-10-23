// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Diagnostics;

namespace Iot.Device.Bmp3xx.Tests
{
    /// <summary>
    /// A simulated BMP3xx on the I2C bus, modelled on the BMP390 datasheet (BST-BMP390-DS002).
    /// Behaviors marked "unverified" are assumptions the datasheet doesn't state explicitly.
    /// </summary>
    internal sealed class SimulatedBmp3xx : I2cSimulatedDeviceBase
    {
        private const byte ChipIdAddress = 0x00;
        private const byte RevisionIdAddress = 0x01;
        private const byte ErrorAddress = 0x02;
        private const byte StatusAddress = 0x03;
        private const byte DataAddress = 0x04;
        private const byte EventAddress = 0x10;
        private const byte PowerControlAddress = 0x1B;
        private const byte OversamplingAddress = 0x1C;
        private const byte OutputDataRateAddress = 0x1D;
        private const byte ConfigurationAddress = 0x1F;
        private const byte CalibrationAddress = 0x31;
        private const byte CommandAddress = 0x7E;

        private const byte CommandReady = 0x10;
        private const byte PressureDataReady = 0x20;
        private const byte TemperatureDataReady = 0x40;
        private const byte CommandError = 0x02;
        private const byte ConfigurationError = 0x04;
        private const byte SoftResetCommand = 0xB6;

        private readonly byte _chipId;
        private readonly byte[] _calibration;
        private uint _rawPressure;
        private uint _rawTemperature;
        private long _modeChangesIgnoredUntil;
        private long _longestNormalModeMeasurementMicroseconds;

        public SimulatedBmp3xx(byte chipId, byte[] calibration)
            : base(new I2cConnectionSettings(1, Bmp3xxBase.DefaultI2cAddress))
        {
            if (calibration.Length != 21)
            {
                throw new ArgumentException("The calibration data is 21 bytes long.", nameof(calibration));
            }

            _chipId = chipId;
            _calibration = calibration;
            LoadResetValues();

            // After power-on the "power-on reset detected" flag is set (section 3.2).
            SetByte(EventAddress, 0x01);
        }

        /// <summary>Raw 24-bit pressure the next measurement produces.</summary>
        public uint RawPressure
        {
            get => _rawPressure;
            set
            {
                _rawPressure = value;
                MeasureIfNormalMode();
            }
        }

        /// <summary>Raw 24-bit temperature the next measurement produces.</summary>
        public uint RawTemperature
        {
            get => _rawTemperature;
            set
            {
                _rawTemperature = value;
                MeasureIfNormalMode();
            }
        }

        /// <summary>When true, a forced measurement never completes: data and data-ready flags stay unchanged.</summary>
        public bool NeverBecomesReady { get; set; }

        /// <summary>When true, entering normal mode sets the configuration error flag (as for an output data rate that is too fast).</summary>
        public bool ConfigurationErrorOnNormalMode { get; set; }

        /// <summary>When true, the soft reset command fails: the command error flag is set and nothing is reset.</summary>
        public bool SoftResetFails { get; set; }

        /// <summary>Number of successful soft resets.</summary>
        public int ResetCount { get; private set; }

        /// <summary>Every register write, in order, including writes the chip ignores.</summary>
        public List<(byte Register, byte Value)> WriteLog { get; } = new();

        public bool IsDisposed { get; private set; }

        /// <summary>Reads a register without the side effects of a bus read (for assertions).</summary>
        public byte GetRegister(byte address) => RegisterMap.TryGetValue(address, out var register) ? (byte)register.ReadRegister() : (byte)0;

        public override void WriteRead(byte[] inputBuffer, byte[] outputBuffer)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(SimulatedBmp3xx));
            }

            if (inputBuffer.Length == 1)
            {
                // Register address only: sets the read pointer (section 5.2.2).
                CurrentRegister = inputBuffer[0];
            }
            else if (inputBuffer.Length > 1)
            {
                // A write is a sequence of (register address, value) pairs (sections 5 and 5.2.1).
                if (inputBuffer.Length % 2 != 0)
                {
                    throw new InvalidOperationException($"I2C write of {inputBuffer.Length} bytes is not a sequence of (register, value) pairs.");
                }

                for (int i = 0; i < inputBuffer.Length; i += 2)
                {
                    WriteRegister(inputBuffer[i], inputBuffer[i + 1]);
                }

                CurrentRegister = inputBuffer[inputBuffer.Length - 2];
            }

            if (outputBuffer.Length > 0)
            {
                BurstRead(outputBuffer);
            }
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }

        private void BurstRead(byte[] outputBuffer)
        {
            // The register address auto-increments during a read (sections 5 and 5.2.2).
            int start = CurrentRegister;
            for (int i = 0; i < outputBuffer.Length; i++)
            {
                outputBuffer[i] = GetRegister((byte)(start + i));
            }

            // Read side effects happen once the burst is over.
            for (int address = start; address < start + outputBuffer.Length; address++)
            {
                switch (address)
                {
                    case ErrorAddress:
                        // cmd_err and conf_err are cleared on read (table 28).
                        SetByte(ErrorAddress, (byte)(GetRegister(ErrorAddress) & ~(CommandError | ConfigurationError)));
                        break;
                    case EventAddress:
                        // por_detected is cleared on read (table 33).
                        SetByte(EventAddress, 0x00);
                        break;
                    case >= DataAddress and <= DataAddress + 2:
                        // drdy_press is reset when a pressure data register is read (table 29).
                        SetByte(StatusAddress, (byte)(GetRegister(StatusAddress) & ~PressureDataReady));
                        break;
                    case >= DataAddress + 3 and <= DataAddress + 5:
                        SetByte(StatusAddress, (byte)(GetRegister(StatusAddress) & ~TemperatureDataReady));
                        break;
                }
            }
        }

        private void WriteRegister(byte address, byte value)
        {
            WriteLog.Add((address, value));
            switch (address)
            {
                case CommandAddress:
                    ExecuteCommand(value);
                    break;
                case PowerControlAddress:
                    OnPowerControlWritten(value);
                    break;
                case OversamplingAddress:
                    SetByte(address, value);

                    // Which settings a running measurement uses after a change in normal mode is unverified; the
                    // simulation assumes the slowest settings since normal mode was entered are still running.
                    if (((GetRegister(PowerControlAddress) >> 4) & 0b11) == 0b11)
                    {
                        _longestNormalModeMeasurementMicroseconds = Math.Max(_longestNormalModeMeasurementMicroseconds, MeasurementMicroseconds());
                    }

                    ReportConfigurationErrorIfNormalMode();
                    break;
                case OutputDataRateAddress:
                    SetByte(address, value);
                    ReportConfigurationErrorIfNormalMode();
                    break;
                case ConfigurationAddress:
                    SetByte(address, value);
                    break;
                default:
                    // Read-only or unmodelled register: the write has no effect.
                    break;
            }
        }

        private void ExecuteCommand(byte command)
        {
            if (command != SoftResetCommand)
            {
                // Other commands (FIFO flush) aren't modelled.
                return;
            }

            if (SoftResetFails)
            {
                SetByte(ErrorAddress, (byte)(GetRegister(ErrorAddress) | CommandError));
                return;
            }

            // "All user configuration settings are overwritten with their default state" (table 48).
            // The calibration coefficients live in non-volatile memory and survive.
            LoadResetValues();
            SetByte(EventAddress, 0x01);
            ResetCount++;
        }

        private void OnPowerControlWritten(byte value)
        {
            int previousMode = (GetRegister(PowerControlAddress) >> 4) & 0b11;
            int mode = (value >> 4) & 0b11;

            // Verified on a BMP390: when normal mode is left, the running measurement finishes first, and a new mode
            // written before then is ignored, although PWR_CTRL already reads sleep. The simulation assumes the worst
            // case: the longest measurement since normal mode was entered had just started.
            if (mode == 0b00 && previousMode == 0b11)
            {
                _modeChangesIgnoredUntil = Stopwatch.GetTimestamp() + (_longestNormalModeMeasurementMicroseconds * Stopwatch.Frequency / 1_000_000);
            }
            else if (mode != 0b00 && Stopwatch.GetTimestamp() < _modeChangesIgnoredUntil)
            {
                SetByte(PowerControlAddress, (byte)(value & ~0b0011_0000));
                return;
            }

            SetByte(PowerControlAddress, value);
            byte powerControl = value;
            if (mode == 0b01 || mode == 0b10)
            {
                // Forced mode: one measurement, then back to sleep by itself (section 3.3.2).
                if (NeverBecomesReady)
                {
                    return;
                }

                Measure();
                SetByte(PowerControlAddress, (byte)(powerControl & ~0b0011_0000));
            }
            else if (mode == 0b11)
            {
                // A repeated normal-mode write doesn't end a running measurement, so it keeps the longest one.
                if (previousMode != 0b11)
                {
                    _longestNormalModeMeasurementMicroseconds = MeasurementMicroseconds();
                }

                // Normal mode. conf_err is only evaluated in normal mode (table 28). Verified on a BMP390: the sensor
                // also goes back to sleep at once.
                if (ConfigurationErrorOnNormalMode)
                {
                    SetByte(ErrorAddress, (byte)(GetRegister(ErrorAddress) | ConfigurationError));
                    SetByte(PowerControlAddress, (byte)(powerControl & ~0b0011_0000));
                    return;
                }

                Measure();
            }
        }

        private void ReportConfigurationErrorIfNormalMode()
        {
            // Verified on a BMP390 for an oversampling change: in normal mode it is checked like entering normal mode,
            // and a configuration error puts the sensor back to sleep at once. That an output data rate change is
            // checked the same way is unverified.
            if (((GetRegister(PowerControlAddress) >> 4) & 0b11) == 0b11 && ConfigurationErrorOnNormalMode)
            {
                SetByte(ErrorAddress, (byte)(GetRegister(ErrorAddress) | ConfigurationError));
                SetByte(PowerControlAddress, (byte)(GetRegister(PowerControlAddress) & ~0b0011_0000));
            }
        }

        private void MeasureIfNormalMode()
        {
            // Normal mode measures continuously; the simulation measures whenever the scripted values change.
            if (((GetRegister(PowerControlAddress) >> 4) & 0b11) == 0b11 && !ConfigurationErrorOnNormalMode)
            {
                Measure();
            }
        }

        private void Measure()
        {
            byte powerControl = GetRegister(PowerControlAddress);
            byte status = GetRegister(StatusAddress);

            // A disabled sensor skips its measurement (section 3.4); its data registers are left unchanged (unverified).
            if ((powerControl & 0x01) != 0)
            {
                SetData(DataAddress, _rawPressure);
                status |= PressureDataReady;
            }

            if ((powerControl & 0x02) != 0)
            {
                SetData(DataAddress + 3, _rawTemperature);
                status |= TemperatureDataReady;
            }

            SetByte(StatusAddress, status);
        }

        private long MeasurementMicroseconds()
        {
            // BMP390 typical measurement time (datasheet section 3.9.2).
            byte oversampling = GetRegister(OversamplingAddress);
            return 234 + 392 + ((1 << (oversampling & 0b111)) * 2020) + 163 + ((1 << ((oversampling >> 3) & 0b111)) * 2020);
        }

        private void LoadResetValues()
        {
            // Reset values from table 25.
            SetByte(ChipIdAddress, _chipId);
            SetByte(RevisionIdAddress, 0x01);
            SetByte(ErrorAddress, 0x00);

            // Table 25 lists 0x00, but cmd_rdy means "ready to accept a new command" (table 29), and the simulated
            // chip executes commands instantly, so it always reads 1 (unverified).
            SetByte(StatusAddress, CommandReady);
            SetData(DataAddress, 0x800000);
            SetData(DataAddress + 3, 0x800000);
            SetByte(EventAddress, 0x00);
            SetByte(PowerControlAddress, 0x00);
            SetByte(OversamplingAddress, 0x02);
            SetByte(OutputDataRateAddress, 0x00);
            SetByte(ConfigurationAddress, 0x00);
            SetByte(CommandAddress, 0x00);
            for (int i = 0; i < _calibration.Length; i++)
            {
                SetByte((byte)(CalibrationAddress + i), _calibration[i]);
            }
        }

        private void SetData(int address, uint value)
        {
            // 24-bit value, least significant byte first (sections 4.3.5 and 4.3.6).
            SetByte((byte)address, (byte)value);
            SetByte((byte)(address + 1), (byte)(value >> 8));
            SetByte((byte)(address + 2), (byte)(value >> 16));
        }

        private void SetByte(byte address, byte value)
        {
            if (RegisterMap.TryGetValue(address, out var register))
            {
                register.WriteRegister(value);
            }
            else
            {
                RegisterMap.Add(address, new Register<byte>(value));
            }
        }
    }
}

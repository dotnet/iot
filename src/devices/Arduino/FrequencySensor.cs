// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// The frequency extension allows detecting the frequency of a signal at a given pin.
    /// This extension is supported on gpio pins that support interrupts.
    /// </summary>
    public class FrequencySensor : ExtendedCommandHandler
    {
        private int _frequencyReportingPin = -1;

        private int _lastFrequencyUpdateClock = 0;
        private int _lastFrequencyUpdateTicks = 0;

        private Frequency _currentFrequency = Frequency.Zero;

        private object _frequencyLock = new object();

        /// <summary>
        /// Creates a new instance of this class. This supports pins that have the "Frequency" mode enabled.
        /// </summary>
        public FrequencySensor()
            : base(SupportedMode.Frequency)
        {
        }

        /// <summary>
        /// Enable frequency reporting on the given pin.
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <param name="mode">The mode on which to increase the tick count. Typically rising or falling edge.</param>
        /// <param name="reportDelay">How often the value should update, in milliseconds. Must be positive and smaller than 2^14</param>
        public void EnableFrequencyReporting(int pinNumber, FrequencyMode mode, int reportDelay)
        {
            if (_frequencyReportingPin != -1)
            {
                throw new InvalidOperationException($"Frequency reporting is already enabled for pin {_frequencyReportingPin}. Only one pin is allowed in this mode.");
            }

            if (reportDelay <= 0 || reportDelay >= Math.Pow(2, 14))
            {
                throw new ArgumentOutOfRangeException(nameof(reportDelay), "Value must be between 1 and 2^14");
            }

            DisableFrequencyReporting(-1);

            if (!Board.SupportedPinConfigurations[pinNumber].PinModes.Contains(HandlesMode!))
            {
                throw new InvalidOperationException($"Pin {pinNumber} does not support mode {HandlesMode}");
            }

            Board.SetPinMode(pinNumber, SupportedMode.Frequency);

            (int TimeStamp, int NewTicks, bool Success) firstQuery = EnableFrequencyReportingInternal(pinNumber, mode, reportDelay);
            _lastFrequencyUpdateClock = firstQuery.TimeStamp;
            _lastFrequencyUpdateTicks = firstQuery.NewTicks;
            _frequencyReportingPin = pinNumber;
        }

        /// <inheritdoc />
        protected internal override void OnConnected()
        {
            _frequencyReportingPin = -1;
            base.OnConnected();
        }

        /// <summary>
        /// Returns the last measured frequency. Returns 0 if no frequency measurement is active.
        /// </summary>
        /// <returns>The frequency measured during the last interval</returns>
        public Frequency GetMeasuredFrequency()
        {
            if (_frequencyReportingPin == -1)
            {
                return Frequency.Zero;
            }

            lock (_frequencyLock)
            {
                return _currentFrequency;
            }
        }

        /// <inheritdoc />
        protected override void OnSysexData(ReplyType type, byte[] data)
        {
            if (type != ReplyType.SysexCommand)
            {
                return;
            }

            OnFrequencyReport(data);
        }

        private bool OnFrequencyReport(byte[] bytes)
        {
            (int TimeStamp, int NewTicks, bool Success) result = DecodeFrequencyReport(bytes);
            if (!result.Success)
            {
                // Wrong message type, this is not typically an error
                return false;
            }

            lock (_frequencyLock)
            {
                double deltaTime = result.TimeStamp - _lastFrequencyUpdateClock;
                double deltaTicks = result.NewTicks - _lastFrequencyUpdateTicks;
                if (deltaTime > 0) // Otherwise, this just wraps around or no time has passed
                {
                    _currentFrequency = Frequency.FromHertz(deltaTicks / (deltaTime / 1000));
                }

                _lastFrequencyUpdateClock = result.TimeStamp;
                _lastFrequencyUpdateTicks = result.NewTicks;
            }

            return true;
        }

        private (int TimeStamp, int NewTicks, bool Success) EnableFrequencyReportingInternal(int pinNumber, FrequencyMode mode, int reportDelay)
        {
            if (reportDelay >= (1 << 14))
            {
                throw new ArgumentOutOfRangeException(nameof(reportDelay), "The maximum update delay is 16.383ms");
            }

            FirmataCommandSequence sequence = new();
            sequence.WriteByte((byte)FirmataSysexCommand.FREQUENCY_COMMAND);
            sequence.WriteByte(1);
            sequence.WriteByte((byte)pinNumber);
            sequence.WriteByte((byte)mode);
            sequence.WriteByte((byte)(reportDelay & 0x7f)); // lower 7 bits
            sequence.WriteByte((byte)((reportDelay >> 7) & 0x7f));
            sequence.WriteByte((byte)FirmataCommand.END_SYSEX);
            byte[] reply = SendCommandAndWait(sequence);

            return DecodeFrequencyReport(new Span<byte>(reply.ToArray()));
        }

        private (int TimeStamp, int NewTicks, bool Success) DecodeFrequencyReport(Span<byte> reply)
        {
            if (reply.Length < 13 || reply[0] != (byte)FirmataSysexCommand.FREQUENCY_COMMAND)
            {
                // Logger.LogError("Frequency sensor extension: Incorrect answer received");
                return (0, 0, false);
            }

            int timestamp = (int)FirmataCommandSequence.DecodeUInt32(reply, 3);
            int ticks = (int)FirmataCommandSequence.DecodeUInt32(reply, 8);
            return (timestamp, ticks, true);
        }

        /// <summary>
        /// Disables automatic updating of the frequency counter for the given pin
        /// </summary>
        /// <param name="pinNumber">The pin to use</param>
        public void DisableFrequencyReporting(int pinNumber)
        {
            if (!IsRegistered)
            {
                return;
            }

            // Never send a -1!
            if (pinNumber < 0)
            {
                pinNumber = 0x7F;
            }

            FirmataCommandSequence sequence = new();
            sequence.WriteByte((byte)FirmataSysexCommand.FREQUENCY_COMMAND);
            sequence.WriteByte(0);
            sequence.WriteByte((byte)pinNumber);
            sequence.WriteByte((byte)FrequencyMode.NoChange);
            sequence.WriteByte((byte)FirmataCommand.END_SYSEX);
            SendCommand(sequence);
            _frequencyReportingPin = -1;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            DisableFrequencyReporting(-1);
            base.Dispose(disposing);
        }
    }
}

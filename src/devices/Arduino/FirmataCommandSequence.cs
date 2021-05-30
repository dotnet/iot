// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// A firmata command sequence
    /// Intended to be changed to public visibility later
    /// </summary>
    internal class FirmataCommandSequence
    {
        private List<byte> _sequence;

        /// <summary>
        /// Create a new command sequence
        /// </summary>
        /// <param name="command">The first byte of the command</param>
        public FirmataCommandSequence(FirmataCommand command = FirmataCommand.START_SYSEX)
        {
            _sequence = new List<byte>()
            {
                (byte)command
            };
        }

        internal List<byte> Sequence => _sequence;

        public int Length => _sequence.Count;

        public static UInt32 DecodeUInt32(ReadOnlySpan<byte> data, int fromOffset)
        {
            Int32 value = data[fromOffset];
            value |= data[fromOffset + 1] << 7;
            value |= data[fromOffset + 2] << 14;
            value |= data[fromOffset + 3] << 21;
            value |= data[fromOffset + 4] << 28;
            return (UInt32)value;
        }

        public void WriteByte(byte b)
        {
            _sequence.Add(b);
        }

        internal bool Validate()
        {
            if (Length < 2)
            {
                return false;
            }

            if (Sequence[0] == (byte)FirmataCommand.START_SYSEX && Sequence[Sequence.Count - 1] != (byte)FirmataCommand.END_SYSEX)
            {
                return false;
            }

            return true;
        }

        public void AddValuesAsTwo7bitBytes(ReadOnlySpan<byte> values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                _sequence.Add((byte)(values[i] & (uint)sbyte.MaxValue));
                _sequence.Add((byte)(values[i] >> 7 & sbyte.MaxValue));
            }
        }
    }
}

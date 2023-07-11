// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// A firmata command sequence
    /// Intended to be changed to public visibility later
    /// </summary>
    public class FirmataCommandSequence : IEquatable<FirmataCommandSequence>
    {
        private const int InitialCommandLength = 32;

        /// <summary>
        /// Start of sysex command byte. Used as start byte for almost all extended commands.
        /// </summary>
        public const byte StartSysex = (byte)FirmataCommand.START_SYSEX;

        /// <summary>
        /// End of sysex command byte. Must end all sysex commands.
        /// </summary>
        public const byte EndSysex = (byte)FirmataCommand.END_SYSEX;

        private List<byte> _sequence;

        /// <summary>
        /// Create a new command sequence
        /// </summary>
        /// <param name="command">The first byte of the command</param>
        internal FirmataCommandSequence(FirmataCommand command)
        {
            _sequence = new List<byte>()
            {
                (byte)command
            };
        }

        internal FirmataCommandSequence(FirmataCommand command, int pin)
        {
            if (pin > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(pin), "Shorthand commands can only be used with pin numbers <= 15");
            }

            _sequence = new List<byte>()
            {
                (byte)(((byte)command) | pin)
            };
        }

        /// <summary>
        /// Create a new sysex command sequence. The <see cref="StartSysex"/> byte is added automatically.
        /// </summary>
        public FirmataCommandSequence()
            : this(FirmataCommand.START_SYSEX)
        {
        }

        /// <summary>
        /// The current sequence
        /// </summary>
        public IReadOnlyList<byte> Sequence => _sequence;

        /// <summary>
        /// The current length of the sequence
        /// </summary>
        public int Length => _sequence.Count;

        internal byte[] InternalSequence => _sequence.ToArray();

        /// <summary>
        /// Decode an uint from packed 7-bit data.
        /// This way of encoding uints is only used in extension modules.
        /// </summary>
        /// <param name="data">Data. 5 bytes expected</param>
        /// <param name="fromOffset">Start offset in data</param>
        /// <returns>The decoded unsigned integer</returns>
        /// <exception cref="InvalidDataException">The received data is invalid</exception>
        public static UInt32 DecodeUInt32(ReadOnlySpan<byte> data, int fromOffset)
        {
            for (int i = 0; i < 5; i++)
            {
                // Bit 7 of the data must always be 0, or there's either a communication problem or a protocol mismatch
                if ((data[fromOffset + i] & 0x80) != 0)
                {
                    throw new InvalidDataException("An invalid byte was received. The message was probably corrupted");
                }
            }

            Int32 value = data[fromOffset];
            value |= data[fromOffset + 1] << 7;
            value |= data[fromOffset + 2] << 14;
            value |= data[fromOffset + 3] << 21;
            value |= data[fromOffset + 4] << 28;
            return (UInt32)value;
        }

        /// <summary>
        /// Decode an int from packed 7-bit data.
        /// This way of encoding uints is only used in extension modules.
        /// </summary>
        /// <param name="data">Data. 5 bytes expected</param>
        /// <param name="fromOffset">Start offset in data</param>
        /// <returns>The decoded number</returns>
        public static Int32 DecodeInt32(ReadOnlySpan<byte> data, int fromOffset)
        {
            return (Int32)DecodeUInt32(data, fromOffset);
        }

        /// <summary>
        /// Decodes a 14-bit integer into a short
        /// </summary>
        /// <param name="data">Data array</param>
        /// <param name="idx">Start offset</param>
        /// <returns></returns>
        public static short DecodeInt14(byte[] data, int idx)
        {
            return (short)(data[idx] | data[idx + 1] << 7);
        }

        /// <summary>
        /// Send an Uint32 as 5 x 7 bits. This form of transmitting integers is only supported by extension modules
        /// </summary>
        /// <param name="value">The 32-Bit value to transmit</param>
        public void SendUInt32(UInt32 value)
        {
            byte[] data = new byte[5];
            data[0] = (byte)(value & 0x7F);
            data[1] = (byte)((value >> 7) & 0x7F);
            data[2] = (byte)((value >> 14) & 0x7F);
            data[3] = (byte)((value >> 21) & 0x7F);
            data[4] = (byte)((value >> 28) & 0x7F);
            _sequence.AddRange(data);
        }

        /// <summary>
        /// Send an Int32 as 5 x 7 bits. This form of transmitting integers is only supported by extension modules
        /// </summary>
        /// <param name="value">The 32-Bit value to transmit</param>
        public void SendInt32(Int32 value)
        {
            SendUInt32((uint)value);
        }

        /// <summary>
        /// Add a byte to the command sequence
        /// </summary>
        /// <param name="b">The byte to add</param>
        public void WriteByte(byte b)
        {
            _sequence.Add(b);
        }

        /// <summary>
        /// Add a sequence of bytes to the command sequence. The bytes must be encoded already.
        /// </summary>
        /// <param name="bytesToSend">The raw block to send</param>
        public void Write(byte[] bytesToSend)
        {
            _sequence.AddRange(bytesToSend);
        }

        /// <summary>
        /// Add a sequence of bytes to the command sequence. The bytes must be encoded already.
        /// </summary>
        /// <param name="bytesToSend">The raw block to send</param>
        /// <param name="startIndex">Start index</param>
        /// <param name="length">Number of bytes to send</param>
        public void Write(byte[] bytesToSend, int startIndex, int length)
        {
            for (int i = startIndex; i < startIndex + length; i++)
            {
                _sequence.Add(bytesToSend[i]);
            }
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

        /// <summary>
        /// Encodes a set of bytes with 7 bits and adds them to the sequence. Each input byte is encoded in 2 bytes.
        /// </summary>
        /// <param name="values">Binary data to add</param>
        public void WriteBytesAsTwo7bitBytes(ReadOnlySpan<byte> values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                _sequence.Add((byte)(values[i] & (uint)sbyte.MaxValue));
                _sequence.Add((byte)(values[i] >> 7 & sbyte.MaxValue));
            }
        }

        /// <summary>
        /// Write a packed Int14 to the stream. This is used to write an integer of up to 14 bits.
        /// </summary>
        /// <param name="value">The value to write. Only the 14 least significant bits are transmitted</param>
        public void SendInt14(int value)
        {
            WriteByte((byte)(value & 0x7F));
            WriteByte((byte)((value >> 7) & 0x7F));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

            int maxBytes = Math.Min(Length, 32);
            for (int i = 0; i < maxBytes; i++)
            {
                b.Append($"{_sequence[i]:X2} ");
            }

            if (maxBytes < Length)
            {
                b.Append("...");
            }

            return b.ToString();
        }

        /// <inheritdoc />
        public bool Equals(FirmataCommandSequence? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return _sequence.Equals(other._sequence) && Length == other.Length;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((FirmataCommandSequence)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (_sequence.GetHashCode() * 397);
            }
        }
    }
}

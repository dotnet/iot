// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Special NMEA0183 message used to pass NMEA2000 messages over NMEA0183, only supported
    /// by some converters and for some messages. We also use it if we have a raw NMEA2000 input interface.
    /// The messages are usually not fully documented, but the SeaSmart (v1.6.0) protocol
    /// specification may help (and some trying around)
    /// Another great source for NMEA2000 commands is https://canboat.github.io/canboat/canboat.html
    /// </summary>
    public abstract class Nmea2000PackedMessage : NmeaSentence
    {
        /// <summary>
        /// Industry and manufacturer code for Raymarine
        /// </summary>
        public static uint ManufacturerRaymarine => 0x3B9F;

        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("DIN");
        private static bool Matches(SentenceId sentence) => Id == sentence;

        /// <summary>
        /// Checks this message has the correct talker id
        /// </summary>
        /// <param name="sentence">The sentence to check</param>
        /// <returns>True if this input sentence matches this message type (but be careful that this message
        /// type needs further division by arguments)</returns>
        protected static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Creates a default message of this type
        /// </summary>
        protected Nmea2000PackedMessage()
            : base(TalkerId.Proprietary, Id, DateTimeOffset.UtcNow)
        {
            DestinationAddress = 0xff;
        }

        /// <summary>
        /// Used to create a message while decoding, see base class implementation
        /// </summary>
        protected Nmea2000PackedMessage(TalkerId talker, SentenceId id, DateTimeOffset time)
            : base(talker, id, time)
        {
            DestinationAddress = 0xff;
        }

        /// <summary>
        /// The hex identifier of this message type (first field of a PCDIN message).
        /// Note that this member returns a constant, as declared in the message type,
        /// not the actual PGN used to send this message. So this does not include the destination
        /// address for addressed messages nor the priority.
        /// </summary>
        public abstract uint Identifier
        {
            get;
        }

        /// <summary>
        /// The timestamp for the NMEA 2000 message
        /// </summary>
        public int MessageTimeStamp
        {
            get;
            protected set;
        }

        /// <summary>
        /// The source identifier of the device which sent this message
        /// </summary>
        public byte MessageSource
        {
            get;
            set;
        }

        /// <summary>
        /// The message priority as it was received or shall be sent.
        /// Null to use the default. Note that the $PCDIN messages do not include the priority bits
        /// in the PGN.
        /// </summary>
        public uint? Priority
        {
            get;
            set;
        }

        /// <summary>
        /// Destination address, if explicitly specified, otherwise 0xFF for broadcast.
        /// </summary>
        public byte DestinationAddress
        {
            get;
            set;
        }

        /// <summary>
        /// The static PGN declaration information for this type
        /// </summary>
        public virtual Nmea2000PgnDeclaration? PgnDeclaration
        {
            get
            {
                return Nmea2000Declarations.GetByPgn((uint)Identifier);
            }
        }

        /// <summary>
        /// Reverses the endianess of an integer
        /// </summary>
        public static UInt32 InverseEndianness(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        /// <summary>
        /// Reverses the endianess of an integer
        /// </summary>
        public static Int32 InverseEndianness(Int32 value)
        {
            return (int)((value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24);
        }

        /// <summary>
        /// Returns true if the PGNs of the two messages match.
        /// </summary>
        public override bool IsSameMessageAs(NmeaSentence other)
        {
            if (other is Nmea2000PackedMessage other2)
            {
                return base.IsSameMessageAs(other2) && Identifier == other2.Identifier;
            }

            return false;
        }

        /// <summary>
        /// Converts a number to a 16 bit number field
        /// </summary>
        /// <param name="v">Input value</param>
        /// <param name="scaleFactor">Scale Factor</param>
        /// <returns>An uint, meant to be directly converted to hex for output</returns>
        protected static uint DoubleTo16BitField(double? v, double scaleFactor)
        {
            if (v == null)
            {
                return 0xFFFF;
            }

            int val = (int)(v.Value / scaleFactor);
            return Unsafe.BitCast<int, uint>(val);
        }

        /// <summary>
        /// Decodes a value from a longer hex string (PRDIN messages contain one blob of stringly-typed hex numbers)
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="start">Start offset of required number, in nibbles(!)</param>
        /// <param name="length">Length of required number, in nibbles(!). Must be 2, 4 or 8</param>
        /// <param name="inverseEndianness">True to inverse the endianness of the number (reverse the partial string)</param>
        /// <param name="value">The output value</param>
        /// <returns>True on success, false otherwise</returns>
        /// <exception cref="ArgumentException">Length is not 2, 4 or 8</exception>
        /// <remarks>
        /// Other erroneous inputs don't throw an exception but return false, e.g. string shorter than expected or
        /// value is not a hex number. This is to prevent an exception in case of a malformed message.
        /// The offset and length are given in nibbles (half-bytes); as they operate on the input string.
        /// </remarks>
        protected bool ReadSignedFromHexString(string input, int start, int length, bool inverseEndianness, out int value)
        {
            if (length % 2 != 0)
            {
                throw new ArgumentException("Length must be even", nameof(length));
            }

            if (input.Length < start + length)
            {
                value = 0;
                return false;
            }

            // length is given in characters here, not in bytes
            string part = input.Substring(start, length);

            if (!UInt32.TryParse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt32 result))
            {
                value = 0;
                return false;
            }

            bool isUnknown = false;

            if (length > 4 && inverseEndianness)
            {
                result = InverseEndianness(result);
                if (length == 6)
                {
                    result = result >> 8;
                    if (result == 0x7FFFFF)
                    {
                        isUnknown = true;
                    }
                }
                else if (result == int.MaxValue)
                {
                    isUnknown = true;
                }
            }
            else if (length == 4 && inverseEndianness)
            {
                result = result >> 8 | ((result & 0xFF) << 8);
                if (result == short.MaxValue)
                {
                    isUnknown = true;
                }
            }

            value = (int)result;
            return !isUnknown;
        }

        /// <summary>
        /// Decodes a value from a longer hex string (PRDIN messages contain one blob of stringly-typed hex numbers)
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="start">Start offset of required number, in nibbles(!)</param>
        /// <param name="length">Length of required number, in nibbles(!). Must be 2, 4 or 8</param>
        /// <param name="inverseEndianness">True to inverse the endianness of the number (reverse the partial string)</param>
        /// <param name="value">The output value</param>
        /// <returns>True on success, false otherwise</returns>
        /// <exception cref="ArgumentException">Length is not 2, 4 or 8</exception>
        /// <remarks>
        /// Other erroneous inputs don't throw an exception but return false, e.g. string shorter than expected or
        /// value is not a hex number. This is to prevent an exception in case of a malformed message.
        /// The offset and length are given in nibbles (half-bytes); as they operate on the input string.
        /// </remarks>
        protected bool ReadUnsignedFromHexString(string input, int start, int length, bool inverseEndianness, out uint value)
        {
            if (length % 2 != 0)
            {
                throw new ArgumentException("Length must be even", nameof(length));
            }

            if (input.Length < start + length)
            {
                value = 0;
                return false;
            }

            // length is given in characters here, not in bytes
            string part = input.Substring(start, length);

            if (!UInt32.TryParse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt32 result))
            {
                value = 0;
                return false;
            }

            if (length > 4 && inverseEndianness)
            {
                result = InverseEndianness(result);
                if (length == 6)
                {
                    result = result >> 8;
                }
            }
            else if (length == 4 && inverseEndianness)
            {
                result = result >> 8 | ((result & 0xFF) << 8);
            }

            value = result;
            return true;
        }

        protected bool ReadByteFromHexString(string input, int offset, out byte b)
        {
            if (ReadUnsignedFromHexString(input, offset, 2, false, out uint value))
            {
                if (value != 0xFF)
                {
                    b = (byte)value;
                    return true;
                }
            }

            b = 0xFF;
            return false;
        }

        protected bool ReadSbyteFromHexString(string input, int offset, out sbyte b)
        {
            if (ReadSignedFromHexString(input, offset, 2, false, out int value))
            {
                if (value != 0x7F)
                {
                    b = (sbyte)value;
                    return true;
                }
            }

            b = 0x7F;
            return false;
        }

        protected bool ReadUshortFromHexString(string input, int offset, out ushort v)
        {
            if (ReadUnsignedFromHexString(input, offset, 4, true, out uint value))
            {
                if (value != 0xFFFF)
                {
                    v = (ushort)value;
                    return true;
                }
            }

            v = 0xFFFF;
            return false;
        }

        protected bool ReadManufacturerAndIndustryFromHexString(string input, int offset, out ManufacturerCode manufacturer, out IndustryCode industry)
        {
            if (ReadUnsignedFromHexString(input, offset, 4, true, out uint value))
            {
                manufacturer = (ManufacturerCode)(value & 0x7FF); // Uses the 11 lower bits
                // 2 bits here are reserved
                industry = (IndustryCode)(value >> 13); // Uses the three upper bits
                return true;
            }

            manufacturer = ManufacturerCode.Unknown;
            industry = IndustryCode.Global;
            return false;
        }

        protected bool ReadUintFromHexString(string input, int offset, out uint v)
        {
            if (ReadUnsignedFromHexString(input, offset, 8, true, out uint value))
            {
                if (value != uint.MaxValue)
                {
                    v = value;
                    return true;
                }
            }

            v = uint.MaxValue;
            return false;
        }

        protected bool ReadShortFromHexString(string input, int offset, out short s)
        {
            if (ReadSignedFromHexString(input, offset, 4, true, out int value))
            {
                if (value != 0x7FFF)
                {
                    s = (short)value;
                    return true;
                }
            }

            s = 0x7FFF;
            return false;
        }

        /// <summary>
        /// Helper method for parsing the header fields (PGN, timestamp and source address)
        /// </summary>
        /// <param name="field">The enumerator over the arguments</param>
        /// <param name="isAddressedMessage">True if this message is addressed (i.e. the lower byte of the
        /// pgn denote the destination address)</param>
        protected void ParseCommonFields(IEnumerator<string> field, bool isAddressedMessage = false)
        {
            string subMessage = ReadString(field);
            if (!uint.TryParse(subMessage, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result))
            {
                Valid = false;
                return;
            }

            // Only if not set yet
            if (Priority == null)
            {
                Priority = (result >> 18) & 0x7;
            }

            if (isAddressedMessage)
            {
                DestinationAddress = (byte)(result & 0xFF);
            }
            else
            {
                DestinationAddress = 0xff;
            }

            string timeStamp = ReadString(field);

            if (Int32.TryParse(timeStamp, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int time1))
            {
                MessageTimeStamp = time1;
            }

            string source = ReadString(field);
            if (uint.TryParse(source, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint src))
            {
                MessageSource = (byte)src;
            }
        }

        protected string WriteManufacturerAndIndustryToHex(ManufacturerCode manufacturer, IndustryCode industry)
        {
            uint manufacturerAndIndustry = ((uint)manufacturer) | ((uint)industry << 13) | 0x1800;
            return WriteUshortToHex((ushort)manufacturerAndIndustry);
        }

        protected string WriteUshortToHex(ushort? value)
        {
            if (!value.HasValue)
            {
                return "FFFF";
            }

            string data = value.Value.ToString("X4", CultureInfo.InvariantCulture);
            // There's no rotation for shorts available, so we do it the ugly way here
            return data.Substring(2, 2) + data.Substring(0, 2);
        }

        protected string WriteShortToHex(short? value)
        {
            if (!value.HasValue)
            {
                return "FF7F";
            }

            string data = value.Value.ToString("X4", CultureInfo.InvariantCulture);
            // There's no rotation for shorts available, so we do it the ugly way here
            return data.Substring(2, 2) + data.Substring(0, 2);
        }

        protected string WriteByteToHex(byte? value)
        {
            if (!value.HasValue)
            {
                return "FF";
            }

            return value.Value.ToString("X2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// This prepares the header part of the NMEA2000 PCDIN message (first 3 fields)
        /// </summary>
        public override string ToNmeaParameterList()
        {
            uint id = Identifier;
            if (DestinationAddress != 0xFF && PgnDeclaration != null && PgnDeclaration.IsAddressed)
            {
                id = id | DestinationAddress;
            }

            string pgn = id.ToString("X6", CultureInfo.InvariantCulture);
            string timeStampText = MessageTimeStamp.ToString("X8", CultureInfo.InvariantCulture);
            string source = MessageSource.ToString("X2", CultureInfo.InvariantCulture);
            return $"{pgn},{timeStampText},{source},";
        }

        protected ushort? TryReadUShort(string input, int offset)
        {
            return ReadUshortFromHexString(input, offset, out ushort v) ? v : null;
        }

        protected uint? TryReadUInt(string input, int offset)
        {
            return ReadUintFromHexString(input, offset, out uint v) ? v : null;
        }

        protected int? TryReadInt32(string input, int offset)
        {
            return ReadSignedFromHexString(input, offset, 8, true, out int v) ? v : null;
        }

        protected long? TryReadInt64(string input, int offset)
        {
            if (input.Length < offset + 16)
            {
                return null;
            }

            Span<byte> bytes = stackalloc byte[8];
            for (int i = 0; i < 8; i++)
            {
                string b = input.Substring(offset + i * 2, 2);
                if (!byte.TryParse(b, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
                {
                    return null;
                }
            }

            long value = BinaryPrimitives.ReadInt64LittleEndian(bytes);
            return value == long.MaxValue ? null : value;
        }

        protected string WriteUInt32ToHex(uint value)
        {
            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
            return Convert.ToHexString(bytes);
        }

        protected string WriteInt32ToHex(int value)
        {
            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
            return Convert.ToHexString(bytes);
        }

        protected string WriteInt64Scaled(double? value, double scale)
        {
            if (!value.HasValue)
            {
                return "FFFFFFFFFFFFFF7F";
            }

            long raw = (long)Math.Round(value.Value / scale);
            Span<byte> bytes = stackalloc byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(bytes, raw);
            return Convert.ToHexString(bytes);
        }
    }
}

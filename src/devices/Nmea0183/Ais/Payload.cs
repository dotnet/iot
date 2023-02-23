// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Represents the payload part of an AIS message, in form of a bit-string (a string containing only the values "0" or "1")
    /// </summary>
    internal sealed class Payload
    {
        private static readonly SixBitAsciiEncoding _encoding = new SixBitAsciiEncoding();

        public Payload()
        {
            MessageType = AisMessageType.PositionReportClassA;
            RawValue = string.Empty;
        }

        public Payload(string rawValue)
        {
            RawValue = rawValue;
            MessageType = ReadEnum<AisMessageType>(0, 6);
        }

        public string RawValue
        {
            get;
            private set;
        }

        public AisMessageType MessageType { get; internal set; }

        /// <summary>
        /// The length of the payload. This is given in bits!
        /// </summary>
        public int Length => RawValue.Length;

        public T ReadEnum<T>(int startIndex, int length)
            where T : Enum
        {
            var bitValue = Substring(startIndex, length);
            var value = Convert.ToUInt32(bitValue, 2);
            return (T)Enum.ToObject(typeof(T), value);
        }

        public void WriteEnum<T>(T var, int length)
            where T : Enum
        {
            WriteUInt(Convert.ToUInt32(var), length);
        }

        public AisMessageType? ReadNullableMessageType(int startIndex, int length)
        {
            var bitValue = Substring(startIndex, length);
            var value = Convert.ToInt32(bitValue, 2);
            if (Enum.IsDefined(typeof(AisMessageType), value))
            {
                return (AisMessageType)Enum.ToObject(typeof(AisMessageType), value);
            }

            return null;
        }

        public uint ReadUInt(int startIndex, int length)
        {
            var bitValue = Substring(startIndex, length);
            return Convert.ToUInt32(bitValue, 2);
        }

        public void WriteUInt(uint var, int length)
        {
            RawValue += Convert.ToString(var, 2).PadLeft(length, '0');
        }

        public uint? ReadNullableUInt(int startIndex, int length)
        {
            var bitValue = Substring(startIndex, length);
            if (string.IsNullOrWhiteSpace(bitValue))
            {
                return null;
            }

            return Convert.ToUInt32(bitValue, 2);
        }

        public uint? ReadMmsi(int startIndex, int length)
        {
            var bitValue = Substring(startIndex, length);
            if (string.IsNullOrWhiteSpace(bitValue))
            {
                return null;
            }

            var value = Convert.ToUInt32(bitValue, 2);
            if (value == 0)
            {
                return null;
            }

            return value;
        }

        public int ReadInt(int startIndex, int length)
        {
            var bitValue = Substring(startIndex, length);
            var result = Convert.ToInt32(bitValue.Substring(1), 2);
            if (bitValue.StartsWith("1"))
            {
                result = (int)(result - Math.Pow(2, bitValue.Length - 1));
            }

            return result;
        }

        /// <summary>
        /// Writes out an integer
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <param name="length">The minimum number of digits</param>
        public void WriteInt(int value, int length)
        {
            RawValue += Convert.ToString(value, 2).PadLeft(length, '0');
        }

        public double ReadUnsignedDouble(int startIndex, int length)
        {
            var bitValue = Substring(startIndex, length);
            return Convert.ToUInt32(bitValue, 2);
        }

        /// <summary>
        /// Writes out the whole part of a double number
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <param name="length">The minimum number of digits to write</param>
        public void WriteUnsignedDouble(double value, int length)
        {
            RawValue += Convert.ToString((UInt32)value, 2).PadLeft(length, '0');
        }

        public double ReadDouble(int startIndex, int length)
        {
            var bitValue = Substring(startIndex, length);
            var result = (double)Convert.ToInt64(bitValue, 2);

            if (bitValue.StartsWith("1"))
            {
                result = result - Math.Pow(2, bitValue.Length);
            }

            return result;
        }

        /// <summary>
        /// Writes the value as base-two integer. Used to write latitude/longitude values
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <param name="length">The length of the field</param>
        private void WriteDouble(double value, int length)
        {
            if (value < 0)
            {
                value = value + Math.Pow(2, length);
            }

            RawValue += Convert.ToString((UInt32)value, 2).PadLeft(length, '0');
        }

        public int? ReadRateOfTurn(int startIndex, int length)
        {
            var rateOfTurn = ReadInt(startIndex, length);
            return rateOfTurn == -128 ? null : new int?(rateOfTurn);
        }

        public void WriteRateOfTurn(int value, int length)
        {
            WriteInt(value, length);
        }

        public uint? ReadTrueHeading(int startIndex, int length)
        {
            var trueHeading = ReadUInt(startIndex, length);
            return trueHeading == 511 ? null : new uint?(trueHeading);
        }

        public void WriteTrueHeading(uint value, int length)
        {
            WriteUInt(value, length);
        }

        public double ReadLongitude(int startIndex, int length)
        {
            return ReadDouble(startIndex, length) / 600000;
        }

        public void WriteLongitude(double value, int length)
        {
            WriteDouble(value * 600000, length);
        }

        public double ReadLatitude(int startIndex, int length)
        {
            return ReadDouble(startIndex, length) / 600000;
        }

        public void WriteLatitude(double value, int length)
        {
            WriteDouble(value * 600000, length);
        }

        public double ReadSpeedOverGround(int startIndex, int length)
        {
            return ReadUnsignedDouble(startIndex, length) / 10;
        }

        public void WriteSpeedOverGround(double value, int length)
        {
            WriteUnsignedDouble(value * 10, length);
        }

        public double ReadCourseOverGround(int startIndex, int length)
        {
            return ReadUnsignedDouble(startIndex, length) / 10;
        }

        public void WriteCourseOverGround(double value, int length)
        {
            WriteUnsignedDouble(value * 10, length);
        }

        public string ReadString(int startIndex, int length)
        {
            var data = Substring(startIndex, length);

            List<byte> bytes = new List<byte>();
            for (var i = 0; i < data.Length / 6; i++)
            {
                var b = Convert.ToByte(data.Substring(i * 6, 6), 2);

                if (b == 0)
                {
                    break;
                }

                bytes.Add(b);
            }

            return _encoding.GetString(bytes.ToArray()).TrimEnd();
        }

        /// <summary>
        /// Writes the given string as 6-bit encoded ascii to the payload
        /// </summary>
        /// <param name="data">The string to write. Only a subset of the ascii characters is allowed, other letters will be replaced</param>
        /// <param name="length">The maximum length to write, in bits. Must be a multiple of 6</param>
        /// <param name="padToLength">True to pad the payload to the given length, false otherwise</param>
        /// <returns>The number of bits written</returns>
        /// <exception cref="ArgumentException">The length is not a multiple of 6</exception>
        public int WriteString(string data, int length, bool padToLength)
        {
            string encoded = string.Empty;
            if (length % 6 != 0)
            {
                throw new ArgumentException("The length of the expected bit string must be a multiple of 6", nameof(length));
            }

            int maxChars = length / 6;

            int bitsWritten = 0;

            byte[] bytes = new byte[_encoding.GetByteCount(data)];

            int encodedBytes = _encoding.GetBytes(data, 0, data.Length, bytes, 0);

            for (int i = 0; i < encodedBytes; i++)
            {
                if (i > maxChars)
                {
                    break;
                }

                WriteUInt(bytes[i], 6);
                bitsWritten += 6;
            }

            while (padToLength && bitsWritten < length)
            {
                WriteUInt(0, 6);
                bitsWritten += 6;
            }

            return bitsWritten;
        }

        public double ReadDraught(int startIndex, int length)
        {
            return ReadUnsignedDouble(startIndex, length) / 10;
        }

        public void WriteDraught(double var, int length)
        {
            WriteUnsignedDouble(var * 10, length);
        }

        public bool ReadDataTerminalReady(int startIndex, int length)
        {
            var value = ReadUInt(startIndex, length);
            return value == 0;
        }

        public bool ReadBoolean(int startIndex, int length)
        {
            var bitValue = Substring(startIndex, length);
            return Convert.ToInt32(bitValue) == 1;
        }

        private string Substring(int startIndex, int length)
        {
            if (startIndex > RawValue.Length)
            {
                return "0";
            }

            string ret = startIndex + length > RawValue.Length
                ? RawValue.Substring(startIndex)
                : RawValue.Substring(startIndex, length);

            // The value will typically be fed to Convert.ToInt()
            if (string.IsNullOrWhiteSpace(ret))
            {
                return "0";
            }

            return ret;
        }
    }
}

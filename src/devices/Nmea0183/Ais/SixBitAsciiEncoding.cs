// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Converts an input character string to six-bit ascii, as used in AIS string fields.
    /// Only supports uppercase letters, numbers and a few symbols.
    /// </summary>
    internal class SixBitAsciiEncoding : Encoding
    {
        /// <summary>
        /// From 6-bit to unicode
        /// </summary>
        private static readonly Dictionary<byte, char> _decodingDictionary;
        private static readonly Dictionary<char, byte> _encodingDictionary;

        static SixBitAsciiEncoding()
        {
            _decodingDictionary = new Dictionary<byte, char>();
            // It's unclear why the documentation https://gpsd.gitlab.io/gpsd/AIVDM.html says this is an @ character. It it uses as binary 0 terminator, much like in C strings.
            _decodingDictionary.Add(0, '\0');
            for (int i = 1; i <= 26; i++)
            {
                _decodingDictionary.Add((byte)i, (char)(i + 64));
            }

            _decodingDictionary.Add(0x1b, '[');
            _decodingDictionary.Add(0x1c, '\\');
            _decodingDictionary.Add(0x1d, ']');
            _decodingDictionary.Add(0x1e, '^');
            _decodingDictionary.Add(0x1f, '_');
            for (int i = 0x20; i <= 63; i++)
            {
                _decodingDictionary.Add((byte)i, (char)i); // This range maps to the identity
            }

            _encodingDictionary = new Dictionary<char, byte>();

            foreach (var e in _decodingDictionary)
            {
                _encodingDictionary.Add(e.Value, e.Key);
            }

            // The encoding is not exactly reversible, so add some improved replacements
            _encodingDictionary.Add('°', 42); // * instead of ° for the degree sign
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }

        /// <summary>
        /// Encode chars to bytes.
        /// This will return 8-bit bytes, which will later typically be compressed to 6 bit.
        /// The method always returns <paramref name="charCount"/> bytes.
        /// </summary>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (charIndex < 0 || charIndex > chars.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(charIndex));
            }

            if (charCount > chars.Length + charIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(charCount));
            }

            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            if (bytes.Length + byteIndex < charCount)
            {
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "Not enough bytes available in target buffer");
            }

            int byteOffset = byteIndex;
            for (int i = charIndex; i < charIndex + charCount; i++)
            {
                char c = chars[i];
                c = Char.ToUpperInvariant(c);
                if (_encodingDictionary.TryGetValue(c, out byte value))
                {
                    bytes[byteOffset++] = value;
                }
                else
                {
                    bytes[byteOffset++] = 31; // underscore as replacement character
                }
            }

            return charCount;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (byteIndex < 0 || byteCount > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(charIndex));
            }

            if (byteCount > chars.Length + byteCount)
            {
                throw new ArgumentOutOfRangeException(nameof(byteCount));
            }

            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            if (chars.Length + charIndex < byteCount)
            {
                throw new ArgumentOutOfRangeException(nameof(byteIndex), "Not enough chars available in target buffer");
            }

            int charOffset = charIndex;
            for (int i = byteIndex; i < byteIndex + byteCount; i++)
            {
                byte b = bytes[i];
                if (_decodingDictionary.TryGetValue(b, out char value))
                {
                    chars[charOffset++] = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(bytes),
                        $"Invalid input stream. Callot decode value {b}");
                }
            }

            return byteCount;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
    }
}

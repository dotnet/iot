// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// An encoding that converts 8-Bit data to unicode, by just interpreting the input values as the lower part of a
    /// char. This works for all ASCII characters as well as if the input and output devices use the same code page.
    /// </summary>
    public class Raw8BitEncoding : Encoding
    {
        /// <inheritdoc/>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }

        /// <inheritdoc/>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (int i = charIndex; i < charIndex + charCount; i++)
            {
                bytes[byteIndex++] = (byte)chars[i];
            }

            return charCount;
        }

        /// <inheritdoc/>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }

        /// <inheritdoc/>
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (int i = byteIndex; i < byteIndex + byteCount; i++)
            {
                chars[charIndex++] = (char)bytes[i];
            }

            return byteCount;
        }

        /// <inheritdoc/>
        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        /// <inheritdoc/>
        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
    }
}

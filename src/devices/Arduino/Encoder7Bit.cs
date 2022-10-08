// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// This class is used to encode larger chunks of data for transmission using the Firmata protocol.
    /// It converts each block of 7 bytes into a block of 8 bytes, keeping the top bit 0.
    /// </summary>
    public static class Encoder7Bit
    {
        /// <summary>
        /// Calculates the number of bytes generated during decode (the result is smaller than the input)
        /// </summary>
        public static int Num8BitOutBytes(int inputBytes)
        {
            // Equals * 7 / 8
            return (int)Math.Floor(((inputBytes) * 7) / 8.0);
        }

        /// <summary>
        /// Calculates the number of bytes required for the 7-byte encoding
        /// </summary>
        public static int Num7BitOutBytes(int inputBytes)
        {
            return (int)Math.Ceiling(((inputBytes) * 8.0) / 7);
        }

        /// <summary>
        /// Encode a sequence of bytes
        /// </summary>
        /// <param name="data">The data to encode</param>
        /// <returns>The encoded data</returns>
        public static byte[] Encode(ReadOnlySpan<byte> data)
        {
            return Encode(data, 0, data.Length);
        }

        /// <summary>
        /// Encodes a sequence of bytes
        /// </summary>
        /// <param name="data">The data to encode</param>
        /// <param name="startIndex">The start index in the data</param>
        /// <param name="length">The length of the data</param>
        /// <returns>The encoded data</returns>
        public static byte[] Encode(ReadOnlySpan<byte> data, int startIndex, int length)
        {
            int shift = 0;
            byte[] retBytes = new byte[Num7BitOutBytes(length)];
            int index = 0;
            int previous = 0;
            for (int i = startIndex; i < startIndex + length; i++)
            {
                if (shift == 0)
                {
                    retBytes[index] = (byte)(data[i] & 0x7f);
                    index++;
                    shift++;
                    previous = data[i] >> 7;
                }
                else
                {
                    retBytes[index] = (byte)(((data[i] << shift) & 0x7f) | previous);
                    index++;
                    if (shift == 6)
                    {
                        retBytes[index] = (byte)(data[i] >> 1);
                        index++;
                        shift = 0;
                    }
                    else
                    {
                        shift++;
                        previous = data[i] >> (8 - shift);
                    }
                }
            }

            if (shift > 0)
            {
                // Write remainder
                retBytes[index] = (byte)previous;
            }

            return retBytes;
        }

        /// <summary>
        /// Decodes the given data sequence
        /// </summary>
        /// <param name="inData">The data to decode</param>
        /// <returns>The decoded data</returns>
        public static byte[] Decode(ReadOnlySpan<byte> inData)
        {
            byte[] outBytes = new byte[Num8BitOutBytes(inData.Length)];
            for (int i = 0; i < outBytes.Length; i++)
            {
                int j = i << 3;
                int pos = j / 7;
                int shift = j % 7;
                outBytes[i] = (byte)((inData[pos] >> shift) | ((inData[pos + 1] << (7 - shift)) & 0xFF));
            }

            return outBytes;
        }
}
}

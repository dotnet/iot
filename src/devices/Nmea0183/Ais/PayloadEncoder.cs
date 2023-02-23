// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using System;

namespace Iot.Device.Nmea0183.Ais
{
    internal class PayloadEncoder
    {
        public string EncodeSixBitAis(Payload payload, out int paddedBits)
        {
            // Consume each 6 bits and making them 8 bits, with some value shifts
            var value = payload.RawValue;
            if (String.IsNullOrEmpty(value))
            {
                paddedBits = 0;
                return value;
            }

            string ret = string.Empty;

            int numCharsToEncode = (int)Math.Ceiling(value.Length / 6.0);

            paddedBits = 0;

            for (int i = 0; i < numCharsToEncode; ++i)
            {
                string b;
                if (value.Length >= 6 * i + 6)
                {
                    b = value.Substring(6 * i, 6).PadLeft(8, '0');
                }
                else
                {
                    // Last character, only partially filled
                    int remainingBits = value.Length - (6 * i);
                    b = value.Substring(6 * i, remainingBits);
                    b += new string('0', 6 - remainingBits);
                    b = b.PadLeft(8, '0'); // Now 8 bits, the relevant data is in the middle
                    paddedBits = 6 - remainingBits;
                }

                var c = (byte)ConvertBitsToChar(b);

                if (c < 40)
                {
                    c += 48;
                }
                else
                {
                    c += 56;
                }

                ret += Convert.ToChar(c);
            }

            return ret;
        }

        private static Char ConvertBitsToChar(String value)
        {
            int result = 0;

            foreach (Char ch in value)
            {
                result = result * 2 + ch - '0';
            }

            return (Char)result;
        }

    }
}

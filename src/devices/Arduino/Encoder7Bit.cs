using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
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

        public static byte[] Encode(byte[] data)
        {
            return Encode(data, 0, data.Length);
        }

        public static byte[] Encode(byte[] data, int startIndex, int length)
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

        public static byte[] Decode(byte[] inData)
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

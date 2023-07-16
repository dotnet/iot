// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Ili934x
{
    /// <summary>
    /// This is the image format used by the Ili934X internally. It's similar to the (meanwhile otherwise rather obsolete) 16-bit RGB format with
    /// 5 bits for red, 6 bits for green and 5 bits for blue.
    /// </summary>
    internal struct Rgb565 : IEquatable<Rgb565>
    {
        private ushort _value;

        public Rgb565(ushort packedValue)
        {
            _value = packedValue;
        }

        public Rgb565(ushort r, ushort g, ushort b)
        {
            _value = 0;
            InitFrom(r, g, b);
        }

        public int R
        {
            get
            {
                // Ensure full black is a possible result
                int lowByte = _value & 0xF8;
                if (lowByte == 0)
                {
                    return 0;
                }

                return lowByte | 0x7;
            }
        }

        public int G
        {
            get
            {
                int gbyte = ((Swap(_value) & 0x7E0) >> 3);
                if (gbyte == 0)
                {
                    return 0;
                }

                return gbyte | 0x3;
            }
        }

        public int B
        {
            get
            {
                int bbyte = (_value >> 5) & 0xF8;
                if (bbyte == 0)
                {
                    return 0;
                }

                return bbyte | 0x7;
            }
        }

        private void InitFrom(ushort r, ushort g, ushort b)
        {
            // get the top 5 MSB of the blue or red value
            UInt16 retval = (UInt16)(r >> 3);
            // shift right to make room for the green Value
            retval <<= 6;
            // combine with the 6 MSB if the green value
            retval |= (UInt16)(g >> 2);
            // shift right to make room for the red or blue Value
            retval <<= 5;
            // combine with the 6 MSB if the red or blue value
            retval |= (UInt16)(b >> 3);

            _value = Swap(retval);
        }

        /// <summary>
        /// Convert a color structure to a byte tuple representing the colour in 565 format.
        /// </summary>
        /// <param name="color">The color to be converted.</param>
        /// <returns>
        /// This method returns the low byte and the high byte of the 16bit value representing RGB565 or BGR565 value
        ///
        /// byte    11111111 00000000
        /// bit     76543210 76543210
        ///
        /// For ColorSequence.RGB (inversed!, the LSB is the top byte)
        ///         GGGBBBBB RRRRRGGG
        ///         43210543 21043210
        /// </returns>
        public static Rgb565 FromRgba32(Color color)
        {
            // get the top 5 MSB of the blue or red value
            UInt16 retval = (UInt16)(color.R >> 3);
            // shift right to make room for the green Value
            retval <<= 6;
            // combine with the 6 MSB if the green value
            retval |= (UInt16)(color.G >> 2);
            // shift right to make room for the red or blue Value
            retval <<= 5;
            // combine with the 6 MSB if the red or blue value
            retval |= (UInt16)(color.B >> 3);

            return new Rgb565(Swap(retval));
        }

        private static ushort Swap(ushort val) => (ushort)((val >> 8) | (val << 8));

        public ushort PackedValue
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public bool Equals(Rgb565 other)
        {
            return _value == other._value;
        }

        public override bool Equals(object? obj)
        {
            return obj is Rgb565 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public static bool operator ==(Rgb565 left, Rgb565 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rgb565 left, Rgb565 right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns true if the two colors are almost equal
        /// </summary>
        /// <param name="a">First color</param>
        /// <param name="b">Second color</param>
        /// <param name="delta">The allowed delta, in visible bits</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool AlmostEqual(Rgb565 a, Rgb565 b, int delta)
        {
            if (a.PackedValue == b.PackedValue)
            {
                return true;
            }

            if (Math.Abs(a.R - b.R) > (delta << 3))
            {
                return false;
            }

            if (Math.Abs(a.G - b.G) > (delta << 2))
            {
                return false;
            }

            if (Math.Abs(a.B - b.B) > (delta << 3))
            {
                return false;
            }

            return true;
        }

        public Color ToColor()
        {
            return Color.FromArgb(255, R, G, B);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Display
{
    /// <summary>
    /// Fonts for 7-Segment displays
    /// </summary>
    public enum Font : byte
    {
        /// <summary>
        /// Digit 0
        /// </summary>
        Digit_0 = Segment.Top | Segment.TopRight | Segment.BottomRight | Segment.Bottom | Segment.BottomLeft | Segment.TopLeft,

        /// <summary>
        /// Digit 1
        /// </summary>
        Digit_1 = Segment.TopRight | Segment.BottomRight,

        /// <summary>
        /// Digit 2
        /// </summary>
        Digit_2 = Segment.Top | Segment.TopRight | Segment.Bottom | Segment.BottomLeft | Segment.Middle,

        /// <summary>
        /// Digit 3
        /// </summary>
        Digit_3 = Segment.Top | Segment.TopRight | Segment.BottomRight | Segment.Bottom | Segment.Middle,

        /// <summary>
        /// Digit 4
        /// </summary>
        Digit_4 = Segment.TopRight | Segment.BottomRight | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Digit 5
        /// </summary>
        Digit_5 = Segment.Top | Segment.BottomRight | Segment.Bottom | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Digit 6
        /// </summary>
        Digit_6 = Segment.Top | Segment.BottomRight | Segment.Bottom | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Digit 7
        /// </summary>
        Digit_7 = Segment.Top | Segment.TopRight | Segment.BottomRight,

        /// <summary>
        /// Digit 8
        /// </summary>
        Digit_8 = Segment.Top | Segment.TopRight | Segment.BottomRight | Segment.Bottom | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Digit 9
        /// </summary>
        Digit_9 = Segment.Top | Segment.TopRight | Segment.BottomRight | Segment.Bottom | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Hexadecimal digit A (upper case letter A)
        /// </summary>
        Digit_A = Letter_A,

        /// <summary>
        /// Hexadecimal digit B (letter b)
        /// </summary>
        Digit_B = Letter_b,

        /// <summary>
        /// Hexadecimal digit C (upper case letter C)
        /// </summary>
        Digit_C = Letter_C,

        /// <summary>
        /// Hexadecimal digit D (letter d)
        /// </summary>
        Digit_D = Letter_d,

        /// <summary>
        /// Hexadecimal digit E (upper case letter E)
        /// </summary>
        Digit_E = Letter_E,

        /// <summary>
        /// Hexadecimal digit F (upper case letter F)
        /// </summary>
        Digit_F = Letter_F,

        /// <summary>
        /// Lower case letter a
        /// </summary>
        Letter_a = Segment.Top | Segment.TopRight | Segment.BottomRight | Segment.Bottom | Segment.BottomLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter A
        /// </summary>
        Letter_A = Segment.Top | Segment.TopRight | Segment.BottomRight | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Lower case letter b
        /// </summary>
        Letter_b = Segment.BottomRight | Segment.Bottom | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter B (same as digit 8)
        /// </summary>
        Letter_B = Digit_8,

        /// <summary>
        /// Lower case letter b
        /// </summary>
        Letter_c = Segment.Bottom | Segment.BottomLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter C
        /// </summary>
        Letter_C = Segment.Top | Segment.Bottom | Segment.BottomLeft | Segment.TopLeft,

        /// <summary>
        /// Lower case letter b
        /// </summary>
        Letter_d = Segment.TopRight | Segment.BottomRight | Segment.Bottom | Segment.BottomLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter D (same as digit 0)
        /// </summary>
        Letter_D = Digit_0,

        /// <summary>
        /// Lower case letter e
        /// </summary>
        Letter_e = Segment.Top | Segment.TopRight | Segment.Bottom | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter E
        /// </summary>
        Letter_E = Segment.Top | Segment.Bottom | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Lower case letter f (same as upper case letter F)
        /// </summary>
        Letter_f = Segment.Top | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter F (same as lower case letter f)
        /// </summary>
        Letter_F = Letter_f,

        /// <summary>
        /// Lower case letter g (same as digit 9)
        /// </summary>
        Letter_g = Digit_9,

        /// <summary>
        /// Upper case letter G
        /// </summary>
        Letter_G = Segment.Top | Segment.BottomRight | Segment.Bottom | Segment.BottomLeft | Segment.TopLeft,

        /// <summary>
        /// Lower case letter h
        /// </summary>
        Letter_h = Segment.BottomRight | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter H
        /// </summary>
        Letter_H = Segment.TopRight | Segment.BottomRight | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Lower case letter i
        /// </summary>
        Letter_i = Segment.BottomLeft,

        /// Upper case letter I  (same as | /pipe/ symbol)
        Letter_I = Segment.BottomLeft | Segment.TopLeft,

        /// <summary>
        /// Lower case letter j
        /// </summary>
        Letter_j = Segment.BottomRight | Segment.Bottom,

        /// <summary>
        /// Upper case letter J
        /// </summary>
        Letter_J = Segment.TopRight | Segment.BottomRight | Segment.Bottom,

        /// <summary>
        /// Lower case letter l
        /// </summary>
        Letter_l = Segment.Bottom | Segment.BottomLeft,

        /// <summary>
        /// Upper case letter L
        /// </summary>
        Letter_L = Segment.Bottom | Segment.BottomLeft | Segment.TopLeft,

        /// <summary>
        /// Lower case letter n
        /// </summary>
        Letter_n = Segment.BottomRight | Segment.BottomLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter N
        /// </summary>
        Letter_N = Segment.Top | Segment.TopRight | Segment.BottomRight | Segment.BottomLeft | Segment.TopLeft,

        /// <summary>
        /// Lower case letter o
        /// </summary>
        Letter_o = Segment.BottomRight | Segment.Bottom | Segment.BottomLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter O (same as digit 0)
        /// </summary>
        Letter_O = Digit_0,

        /// <summary>
        /// Lower case letter p (same as upper case letter P)
        /// </summary>
        Letter_p = Segment.Top | Segment.TopRight | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter P (same as lower case letter p)
        /// </summary>
        Letter_P = Segment.Top | Segment.TopRight | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Lower case letter r
        /// </summary>
        Letter_r = Segment.BottomLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter R
        /// </summary>
        Letter_R = Segment.Top | Segment.BottomLeft | Segment.TopLeft,

        /// <summary>
        /// Lower case letter s (same as digit 5 and upper case letter S)
        /// </summary>
        Letter_s = Digit_5,

        /// <summary>
        /// Upper case letter S (same as digit 5 and lower case letter s)
        /// </summary>
        Letter_S = Digit_5,

        /// <summary>
        /// Lower case letter t
        /// </summary>
        Letter_t = Segment.Bottom | Segment.BottomLeft | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Lower case letter u
        /// </summary>
        Letter_u = Segment.BottomRight | Segment.Bottom | Segment.BottomLeft,

        /// <summary>
        /// Upper case letter U
        /// </summary>
        Letter_U = Segment.TopRight | Segment.BottomRight | Segment.Bottom | Segment.BottomLeft | Segment.TopLeft,

        /// <summary>
        /// Lower case letter y (same as upper case letter Y)
        /// </summary>
        Letter_y = Segment.TopRight | Segment.BottomRight | Segment.Bottom | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter Y (same as lower case letter y)
        /// </summary>
        Letter_Y = Letter_y,

        /// <summary>
        /// Upper case letter z (same as upper case letter Z)
        /// </summary>
        Letter_z = Segment.Top | Segment.TopRight | Segment.Bottom | Segment.BottomLeft | Segment.Middle,

        /// <summary>
        /// Upper case letter Z (same as lower case letter z)
        /// </summary>
        Letter_Z = Segment.Top | Segment.TopRight | Segment.Bottom | Segment.BottomLeft | Segment.Middle,

        /// <summary>
        /// - symbol
        /// </summary>
        Symbol_Minus = Segment.Middle,

        /// <summary>
        /// = symbol
        /// </summary>
        Symbol_Equals = Segment.Bottom | Segment.Middle,

        /// <summary>
        /// _ symbol
        /// </summary>
        Symbol_Underscore = Segment.Bottom,

        /// <summary>
        /// | symbol (same as upper case letter I)
        /// </summary>
        Symbol_Pipe = Letter_I,

        /// <summary>
        /// ° symbol
        /// </summary>
        Symbol_Degree = Segment.Top | Segment.TopRight | Segment.TopLeft | Segment.Middle,

        /// <summary>
        /// [ symbol (same as upper case letter C)
        /// </summary>
        Symbol_LeftSquareBracket = Letter_C,

        /// <summary>
        /// ] symbol
        /// </summary>
        Symbol_RightSquareBracket = Segment.Top | Segment.Bottom | Segment.BottomRight | Segment.TopRight,

        /// <summary>
        /// Whitespace
        /// </summary>
        Whitespace = Segment.None
    }
}

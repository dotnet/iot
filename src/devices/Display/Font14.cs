// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display
{
    /// <summary>
    /// Fonts for 14-Segment displays
    /// </summary>
    /// <remarks>
    /// Sources:
    /// Derived from /src/devices/Display/Font.cs
    /// Using https://upload.wikimedia.org/wikipedia/commons/7/7c/14-segment_ASCII.svg as reference.
    /// Leds inside outer segments are referenced as
    /// points of the compass.
    /// </remarks>
    public enum Font14 : ushort
    {
        /// <summary>
        /// Digit 0
        /// </summary>
        Digit_0 = Segment14.Top | Segment14.Right | Segment14.Bottom | Segment14.Left | Segment14.NorthEast | Segment14.SouthWest,

        /// <summary>
        /// Digit 1
        /// </summary>
        Digit_1 = Segment14.Right | Segment14.NorthEast,

        /// <summary>
        /// Digit 2
        /// </summary>
        Digit_2 = Segment14.Top | Segment14.TopRight | Segment14.East | Segment14.SouthWest | Segment14.Bottom,

        /// <summary>
        /// Digit 3
        /// </summary>
        Digit_3 = Segment14.Top | Segment14.Right | Segment14.East | Segment14.Bottom,

        /// <summary>
        /// Digit 4
        /// </summary>
        Digit_4 = Segment14.TopLeft | Segment14.Middle | Segment14.Center,

        /// <summary>
        /// Digit 5
        /// </summary>
        Digit_5 = Segment14.Top | Segment14.TopLeft | Segment14.West | Segment14.SouthEast | Segment14.Bottom,

        /// <summary>
        /// Digit 6
        /// </summary>
        Digit_6 = Segment14.Top | Segment14.Left | Segment14.Bottom | Segment14.BottomRight | Segment14.Middle,

        /// <summary>
        /// Digit 7
        /// </summary>
        Digit_7 = Segment14.Top | Segment14.ForwardSlash,

        /// <summary>
        /// Digit 8
        /// </summary>
        Digit_8 = Segment14.Top | Segment14.Right | Segment14.Bottom | Segment14.Left | Segment14.TopLeft | Segment14.Middle,

        /// <summary>
        /// Digit 9
        /// </summary>
        Digit_9 = Segment14.Top | Segment14.Right | Segment14.Bottom | Segment14.TopLeft | Segment14.Middle,

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
        Letter_a = Segment14.BottomLeft | Segment14.Bottom | Segment14.South | Segment14.West,

        /// <summary>
        /// Upper case letter A
        /// </summary>
        Letter_A = Segment14.Top | Segment14.Right | Segment14.Left | Segment14.Middle,

        /// <summary>
        /// Lower case letter b
        /// </summary>
        Letter_b = Segment14.Left | Segment14.Bottom | Segment14.SouthEast | Segment14.West,

        /// <summary>
        /// Upper case letter B (same as digit 8)
        /// </summary>
        Letter_B = Segment14.Top | Segment14.East | Segment14.Bottom | Segment14.Center | Segment14.Right,

        /// <summary>
        /// Lower case letter b
        /// </summary>
        Letter_c = Segment14.Bottom | Segment14.BottomLeft | Segment14.Middle,

        /// <summary>
        /// Upper case letter C
        /// </summary>
        Letter_C = Segment14.Top | Segment14.Bottom | Segment14.Left,

        /// <summary>
        /// Lower case letter b
        /// </summary>
        Letter_d = Segment14.Right | Segment14.Bottom | Segment14.East | Segment14.SouthWest,

        /// <summary>
        /// Upper case letter D (same as digit 0)
        /// </summary>
        Letter_D = Segment14.Top | Segment14.Bottom | Segment14.Right | Segment14.Center,

        /// <summary>
        /// Lower case letter e
        /// </summary>
        Letter_e = Segment14.BottomLeft | Segment14.Bottom | Segment14.Bottom | Segment14.West | Segment14.SouthWest,

        /// <summary>
        /// Upper case letter E
        /// </summary>
        Letter_E = Segment14.Top | Segment14.Bottom | Segment14.Left | Segment14.Middle,

        /// <summary>
        /// Lower case letter f (same as upper case letter F)
        /// </summary>
        Letter_f = Segment14.Middle | Segment14.South | Segment14.NorthEast,

        /// <summary>
        /// Upper case letter F (same as lower case letter f)
        /// </summary>
        Letter_F = Segment14.Top | Segment14.Left | Segment14.Middle,

        /// <summary>
        /// Lower case letter g (same as digit 9)
        /// </summary>
        Letter_g = Segment14.Top | Segment14.Right | Segment14.Bottom | Segment14.East | Segment14.NorthWest,

        /// <summary>
        /// Upper case letter G
        /// </summary>
        Letter_G = Segment14.Top | Segment14.Left | Segment14.Bottom | Segment14.BottomRight | Segment14.East,

        /// <summary>
        /// Lower case letter h
        /// </summary>
        Letter_h = Segment14.BottomRight | Segment14.Left | Segment14.Middle,

        /// <summary>
        /// Upper case letter H
        /// </summary>
        Letter_H = Segment14.Left | Segment14.Right | Segment14.Middle,

        /// <summary>
        /// Lower case letter i
        /// </summary>
        Letter_i = Segment14.BottomRight,

        /// Upper case letter I  (same as | /pipe/ symbol)
        Letter_I = Segment14.Top | Segment14.Bottom | Segment14.Center,

        /// <summary>
        /// Lower case letter j
        /// </summary>
        Letter_j = Segment14.Right | Segment14.Bottom,

        /// <summary>
        /// Upper case letter J
        /// </summary>
        Letter_J = Segment14.Right | Segment14.BottomLeft | Segment14.Bottom,

        /// <summary>
        /// Lower case letter k
        /// </summary>
        Letter_k = Segment14.Center | Segment14.East | Segment14.SouthEast,

        /// <summary>
        /// Upper case letter K
        /// </summary>
        Letter_K = Segment14.TopLeft | Segment14.BottomLeft | Segment14.West | Segment14.NorthEast | Segment14.SouthEast,

        /// <summary>
        /// Lower case letter l
        /// </summary>
        Letter_l = Segment14.Left | Segment14.SouthWest,

        /// <summary>
        /// Upper case letter L
        /// </summary>
        Letter_L = Segment14.Left | Segment14.Bottom,

        /// <summary>
        /// Lower case letter l
        /// </summary>
        Letter_m = Segment14.BottomLeft | Segment14.Middle | Segment14.South | Segment14.BottomRight,

        /// <summary>
        /// Upper case letter L
        /// </summary>
        Letter_M = Segment14.Left | Segment14.NorthWest | Segment14.NorthEast | Segment14.Right,

        /// <summary>
        /// Lower case letter n
        /// </summary>
        Letter_n = Segment14.BottomRight | Segment14.BottomLeft | Segment14.Middle,

        /// <summary>
        /// Upper case letter N
        /// </summary>
        Letter_N = Segment14.Left | Segment14.Right | Segment14.BackSlash,

        /// <summary>
        /// Lower case letter o
        /// </summary>
        Letter_o = Segment14.BottomRight | Segment14.Bottom | Segment14.BottomLeft | Segment14.Middle,

        /// <summary>
        /// Upper case letter O (same as digit 0)
        /// </summary>
        Letter_O = Segment14.Top | Segment14.Left | Segment14.Bottom | Segment14.Right,

        /// <summary>
        /// Lower case letter p (same as upper case letter P)
        /// </summary>
        Letter_p = Segment14.Top | Segment14.Left | Segment14.West | Segment14.NorthEast,

        /// <summary>
        /// Upper case letter P (same as lower case letter p)
        /// </summary>
        Letter_P = Segment14.Top | Segment14.TopRight | Segment14.Middle | Segment14.Left,

        /// <summary>
        /// Lower case letter q
        /// </summary>
        Letter_q = Segment14.Top | Segment14.Right | Segment14.East | Segment14.NorthWest,

        /// <summary>
        /// Upper case letter Q
        /// </summary>
        Letter_Q = Segment14.Top | Segment14.Right | Segment14.Left | Segment14.Bottom | Segment14.SouthEast,

        /// <summary>
        /// Lower case letter r
        /// </summary>
        Letter_r = Segment14.BottomLeft | Segment14.Middle,

        /// <summary>
        /// Upper case letter R
        /// </summary>
        Letter_R = Segment14.Top | Segment14.Left | Segment14.TopRight | Segment14.Middle | Segment14.SouthEast,

        /// <summary>
        /// Lower case letter s (same as digit 5 and upper case letter S)
        /// </summary>
        Letter_s = Segment14.Bottom | Segment14.SouthEast | Segment14.East,

        /// <summary>
        /// Upper case letter S (same as digit 5 and lower case letter s)
        /// </summary>
        Letter_S = Segment14.Top | Segment14.Middle | Segment14.Bottom | Segment14.TopLeft | Segment14.BottomRight,

        /// <summary>
        /// Lower case letter t
        /// </summary>
        Letter_t = Segment14.Left | Segment14.Bottom | Segment14.West,

        /// <summary>
        /// Upper case letter T
        /// </summary>
        Letter_T = Segment14.Top | Segment14.Center,

        /// <summary>
        /// Lower case letter u
        /// </summary>
        Letter_u = Segment14.BottomRight | Segment14.Bottom | Segment14.BottomLeft,

        /// <summary>
        /// Upper case letter U
        /// </summary>
        Letter_U = Segment14.Right | Segment14.Bottom | Segment14.Left,

        /// <summary>
        /// Lower case letter v
        /// </summary>
        Letter_v = Segment14.BottomLeft | Segment14.SouthWest,

        /// <summary>
        /// Upper case letter V
        /// </summary>
        Letter_V = Segment14.Left | Segment14.ForwardSlash,

        /// <summary>
        /// Lower case letter w
        /// </summary>
        Letter_w = Segment14.BottomRight | Segment14.Bottom | Segment14.South | Segment14.BottomLeft,

        /// <summary>
        /// Upper case letter W
        /// </summary>
        Letter_W = Segment14.Left | Segment14.Right | Segment14.Bottom | Segment14.South,

        /// <summary>
        /// Lower case letter x
        /// </summary>
        Letter_x = Segment14.Middle | Segment14.SouthWest | Segment14.SouthEast,

        /// <summary>
        /// Upper case letter X
        /// </summary>
        Letter_X = Segment14.BackSlash | Segment14.ForwardSlash,

        /// <summary>
        /// Lower case letter y (same as upper case letter Y)
        /// </summary>
        Letter_y = Segment14.Right | Segment14.Bottom | Segment14.NorthWest | Segment14.East,

        /// <summary>
        /// Upper case letter Y (same as lower case letter y)
        /// </summary>
        Letter_Y = Segment14.NorthWest | Segment14.NorthEast | Segment14.South,

        /// <summary>
        /// Upper case letter z (same as upper case letter Z)
        /// </summary>
        Letter_z = Segment14.Bottom | Segment14.West | Segment14.SouthWest,

        /// <summary>
        /// Upper case letter Z (same as lower case letter z)
        /// </summary>
        Letter_Z = Segment14.Top | Segment14.ForwardSlash | Segment14.Bottom,

        /// <summary>
        /// - symbol
        /// </summary>
        Symbol_Minus = Segment14.Middle,

        /// <summary>
        /// - symbol
        /// </summary>
        Symbol_Plus = Segment14.Middle | Segment14.Center,

        /// <summary>
        /// / symbol
        /// </summary>
        Symbol_ForwardSlash = Segment14.ForwardSlash,

        /// <summary>
        /// \ symbol
        /// </summary>
        Symbol_BackSlash = Segment14.BackSlash,

        /// <summary>
        /// = symbol
        /// </summary>
        Symbol_Equals = Segment14.Bottom | Segment14.Middle,

        /// <summary>
        /// _ symbol
        /// </summary>
        Symbol_Underscore = Segment14.Bottom,

        /// <summary>
        /// | symbol (same as upper case letter I)
        /// </summary>
        Symbol_Pipe = Segment14.Center,

        /// <summary>
        /// ° symbol
        /// </summary>
        Symbol_Degree = Segment14.Top | Segment14.TopRight | Segment14.TopLeft | Segment14.Middle,

        /// <summary>
        /// [ symbol (same as upper case letter C)
        /// </summary>
        Symbol_LeftSquareBracket = Segment14.Top | Segment14.Left | Segment14.Bottom,

        /// <summary>
        /// ] symbol
        /// </summary>
        Symbol_RightSquareBracket = Segment14.Top | Segment14.Bottom | Segment14.Right,

        /// <summary>
        /// . symbol
        /// </summary>
        Symbol_FullStop = Segment14.FullStop,

        /// <summary>
        /// * symbol
        /// </summary>
        Symbol_Asterisk = Segment14.BackSlash | Segment14.ForwardSlash | Segment14.Middle | Segment14.Center,

        /// <summary>
        /// % symbol
        /// </summary>
        Symbol_Percent = Segment14.ForwardSlash | Segment14.TopLeft | Segment14.BottomRight,

        /// <summary>
        /// ? symbol
        /// </summary>
        Symbol_QuestionMark = Segment14.TopLeft | Segment14.Top | Segment14.TopRight | Segment14.East | Segment14.South,

        /// <summary>
        /// Whitespace
        /// </summary>
        Whitespace = Segment14.None
    }
}

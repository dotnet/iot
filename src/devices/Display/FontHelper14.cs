// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Display
{
    /// <summary>
    /// Provides segment mappings for hexadecimal digits and certain ASCII characters
    /// </summary>
    /// <remarks>
    /// Sources:
    /// Derived from /src/devices/Display/FontHelper.cs
    /// </remarks>
    internal static class FontHelper14
    {
        #region Private members

        /// <summary>
        /// Hexadecimal digit (0..F) fonts
        /// </summary>
        private static readonly Font14[] s_hexDigits =
        {
            Font14.Digit_0,
            Font14.Digit_1,
            Font14.Digit_2,
            Font14.Digit_3,
            Font14.Digit_4,
            Font14.Digit_5,
            Font14.Digit_6,
            Font14.Digit_7,
            Font14.Digit_8,
            Font14.Digit_9,
            Font14.Digit_A,
            Font14.Digit_B,
            Font14.Digit_C,
            Font14.Digit_D,
            Font14.Digit_E,
            Font14.Digit_F
        };

        /// <summary>
        /// Upper case letter fonts
        /// </summary>
        private static readonly Font14[] s_upperCaseLetters =
        {
            Font14.Letter_A,
            Font14.Letter_B,
            Font14.Letter_C,
            Font14.Letter_D,
            Font14.Letter_E,
            Font14.Letter_F,
            Font14.Letter_G,
            Font14.Letter_H,
            Font14.Letter_I,
            Font14.Letter_J,
            Font14.Letter_K,
            Font14.Letter_L,
            Font14.Letter_M,
            Font14.Letter_N,
            Font14.Letter_O,
            Font14.Letter_P,
            Font14.Letter_Q,
            Font14.Letter_R,
            Font14.Letter_S,
            Font14.Letter_T,
            Font14.Letter_U,
            Font14.Letter_V,
            Font14.Letter_W,
            Font14.Letter_X,
            Font14.Letter_Y,
            Font14.Letter_Z
        };

        /// <summary>
        /// Lower case letter fonts
        /// </summary>
        private static readonly Font14[] s_lowerCaseLetters =
        {
            Font14.Letter_a,
            Font14.Letter_b,
            Font14.Letter_c,
            Font14.Letter_d,
            Font14.Letter_e,
            Font14.Letter_f,
            Font14.Letter_g,
            Font14.Letter_h,
            Font14.Letter_i,
            Font14.Letter_j,
            Font14.Letter_k,
            Font14.Letter_l,
            Font14.Letter_m,
            Font14.Letter_n,
            Font14.Letter_o,
            Font14.Letter_p,
            Font14.Letter_q,
            Font14.Letter_r,
            Font14.Letter_s,
            Font14.Letter_t,
            Font14.Letter_u,
            Font14.Letter_v,
            Font14.Letter_w,
            Font14.Letter_x,
            Font14.Letter_y,
            Font14.Letter_z
        };
        #endregion

        #region Public members
        #region Constants

        /// <summary>
        /// Used to mask upper 4 bits of a byte for a single hexadecimal value
        /// </summary>
        public const byte HexadecimalMask = 0b0000_1111;
        #endregion

        /// <summary>
        /// Convert byte value hexadecimal digit (0..F) to corresponding font bits
        /// </summary>
        /// <param name="digit">hexadecimal digit (0..F)</param>
        /// <returns>corresponding font</returns>
        public static Font14 GetHexDigit(byte digit) => s_hexDigits[digit & HexadecimalMask];

        /// <summary>
        /// Converts a span of bytes to their corresponding hexadecimal digits' font representation
        /// </summary>
        /// <param name="digits">list of hexadecimal digits (will be converted in place!)</param>
        public static void ConvertHexDigits(Span<byte> digits)
        {
            for (int i = 0, l = digits.Length; i < l; i++)
            {
                digits[i] = (byte)GetHexDigit(digits[i]);
            }
        }

        /// <summary>
        /// Converts a span of bytes to their corresponding hexadecimal digits' font representation
        /// </summary>
        /// <param name="digits">list of hexadecimal digits (will be converted in place!)</param>
        /// <returns>list of corresponding digit fonts</returns>
        public static Font14[] GetHexDigits(ReadOnlySpan<byte> digits)
        {
            var fonts = new Font14[digits.Length];
            if (digits.Length > 0)
            {
                for (int i = 0, l = fonts.Length; i < l; i++)
                {
                    fonts[i] = GetHexDigit(digits[i]);
                }
            }

            return fonts;
        }

        /// <summary>
        /// Convert character value to corresponding font segments
        /// </summary>
        /// <param name="value">input character</param>
        /// <returns>corresponding font bits</returns>
        public static Font14 GetCharacter(char value) => value switch
        {
            _ when value >= 'A' && value <= 'Z' => s_upperCaseLetters[value - 'A'],
            _ when value >= 'a' && value <= 'z' => s_lowerCaseLetters[value - 'a'],
            _ when value >= '0' && value <= '9' => s_hexDigits[value - '0'],
            '+' => Font14.Symbol_Plus,
            '-' => Font14.Symbol_Minus,
            '\\' => Font14.Symbol_BackSlash,
            '/' => Font14.Symbol_ForwardSlash,
            '=' => Font14.Symbol_Equals,
            '_' => Font14.Symbol_Underscore,
            '|' => Font14.Symbol_Pipe,
            '°' => Font14.Symbol_Degree,
            '[' => Font14.Symbol_LeftSquareBracket,
            ']' => Font14.Symbol_RightSquareBracket,
            '.' => Font14.Symbol_FullStop,
            '?' => Font14.Symbol_QuestionMark,
            '%' => Font14.Symbol_Percent,
            '*' => Font14.Symbol_Asterisk,
            _ => Font14.Whitespace
        };

        /// <summary>
        /// Convert a string of characters to corresponding fonts
        /// </summary>
        /// <param name="input">input string</param>
        /// <param name="output">list of corresponding character fonts</param>
        public static void ConvertString(ReadOnlySpan<char> input, Span<Font14> output)
        {
            var fsOffset = 0;
            for (int i = 0, l = input.Length; i < l; i++)
            {
                output[i - fsOffset] = GetCharacter(input[i]);
                // read ahead next char might be a decimal point
                if ((i + 1) < l)
                {
                    if (GetCharacter(input[i + 1]) == Font14.Symbol_FullStop)
                    {
                        // add full stop to current digit.
                        output[i - fsOffset] = output[i - fsOffset] | Font14.Symbol_FullStop;
                        i++;
                        fsOffset++;
                    }
                }
            }
        }

        /// <summary>
        /// Convert a string of characters to corresponding font segments
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>list of corresponding character fonts</returns>
        public static Font14[] GetString(string input)
        {
            var decimalCount = input.Split('.').Length - 1;
            decimalCount = decimalCount > 0 ? decimalCount - (input.Substring(0, 1) == "." ? 1 : 0) : 0;
            var fonts = new Font14[input.Length - decimalCount];
            if (fonts.Length > 0)
            {
                ConvertString(input.AsSpan(), fonts.AsSpan());
            }

            return fonts;
        }
        #endregion
    }
}

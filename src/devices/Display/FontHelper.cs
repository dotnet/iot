// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Display
{
    /// <summary>
    /// Provides segment mappings for hexadecimal digits and certain ASCII characters
    /// </summary>
    public static class FontHelper
    {
        #region Private members
        /// <summary>
        /// Hexadecimal digit (0..F) fonts
        /// </summary>
        private static readonly Font[] s_hexDigits =
        {
            Font.Digit_0,
            Font.Digit_1,
            Font.Digit_2,
            Font.Digit_3,
            Font.Digit_4,
            Font.Digit_5,
            Font.Digit_6,
            Font.Digit_7,
            Font.Digit_8,
            Font.Digit_9,
            Font.Digit_A,
            Font.Digit_B,
            Font.Digit_C,
            Font.Digit_D,
            Font.Digit_E,
            Font.Digit_F
        };

        /// <summary>
        /// Upper case letter fonts
        /// </summary>
        private static readonly Font[] s_upperCaseLetters =
        {
            Font.Letter_A,
            Font.Letter_B,
            Font.Letter_C,
            Font.Letter_D,
            Font.Letter_E,
            Font.Letter_F,
            Font.Letter_G,
            Font.Letter_H,
            Font.Letter_I,
            Font.Letter_J,
            Font.Whitespace, // letter K is not supported
            Font.Letter_L,
            Font.Whitespace, // letter M is not supported
            Font.Letter_N,
            Font.Letter_O,
            Font.Letter_P,
            Font.Whitespace, // letter Q is not supported
            Font.Letter_R,
            Font.Letter_S,
            Font.Whitespace, // letter T is not supported
            Font.Letter_U,
            Font.Whitespace, // letter V is not supported
            Font.Whitespace, // letter W is not supported
            Font.Whitespace, // letter X is not supported
            Font.Letter_Y,
            Font.Letter_Z
        };

        /// <summary>
        /// Lower case letter fonts
        /// </summary>
        private static readonly Font[] s_lowerCaseLetters =
        {
            Font.Letter_a,
            Font.Letter_b,
            Font.Letter_c,
            Font.Letter_d,
            Font.Letter_e,
            Font.Letter_f,
            Font.Letter_g,
            Font.Letter_h,
            Font.Letter_i,
            Font.Letter_j,
            Font.Whitespace, // letter k is not supported
            Font.Letter_l,
            Font.Whitespace, // letter m is not supported
            Font.Letter_n,
            Font.Letter_o,
            Font.Letter_p,
            Font.Whitespace, // letter q is not supported
            Font.Letter_r,
            Font.Letter_s,
            Font.Letter_t,
            Font.Letter_u,
            Font.Whitespace, // letter v is not supported
            Font.Whitespace, // letter w is not supported
            Font.Whitespace, // letter x is not supported
            Font.Letter_y,
            Font.Letter_z
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
        public static Font GetHexDigit(byte digit) => s_hexDigits[digit & HexadecimalMask];

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
        public static Font[] GetHexDigits(ReadOnlySpan<byte> digits)
        {
            var fonts = new Font[digits.Length];
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
        public static Font GetCharacter(char value) => value switch
        {
            _ when value >= 'A' && value <= 'Z' => s_upperCaseLetters[value - 'A'],
            _ when value >= 'a' && value <= 'z' => s_lowerCaseLetters[value - 'a'],
            _ when value >= '0' && value <= '9' => s_hexDigits[value - '0'],
            '-' => Font.Symbol_Minus,
            '=' => Font.Symbol_Equals,
            '_' => Font.Symbol_Underscore,
            '|' => Font.Symbol_Pipe,
            '°' => Font.Symbol_Degree,
            '[' => Font.Symbol_LeftSquareBracket,
            ']' => Font.Symbol_RightSquareBracket,
            _ => Font.Whitespace
        };

        /// <summary>
        /// Convert a string of characters to corresponding fonts
        /// </summary>
        /// <param name="input">input string</param>
        /// <param name="output">list of corresponding character fonts</param>
        public static void ConvertString(ReadOnlySpan<char> input, Span<Font> output)
        {
            if (input.Length != output.Length)
            {
                throw new InvalidOperationException($"{nameof(input)} and {nameof(output)} length must be the same");
            }

            for (int i = 0, l = input.Length; i < l; i++)
            {
                output[i] = GetCharacter(input[i]);
            }
        }

        /// <summary>
        /// Convert a string of characters to corresponding font segments
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>list of corresponding character fonts</returns>
        public static Font[] GetString(string input)
        {
            var fonts = new Font[input.Length];
            if (fonts.Length > 0)
            {
                ConvertString(input.AsSpan(), fonts.AsSpan());
            }
            return fonts;
        }
        #endregion
    }
}

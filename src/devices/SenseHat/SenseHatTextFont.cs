// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// 5x8 font adaptor to render font glyphs into the text render matrix.
    /// </summary>
    public class SenseHatTextFont
    {
        private const int CharGap = 2;
        private const int WordGap = 4; // Reduced space between words
        private const int CharHeight = 8;
        // Limit length of text when rendering
        private const int MaxTextLength = 128;
        // Using Font5x8
        private Graphics.Font5x8 _font = new Graphics.Font5x8();

        /// <summary>
        /// Generates a byte matrix containing the bit pattern for the rendered text.
        /// </summary>
        /// <param name="text">The text to render</param>
        /// <returns>The initial renderMatrix including matrix dimension.</returns>
        public SenseHatTextRenderMatrix RenderText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                // nothing to render
                return new SenseHatTextRenderMatrix(text, new byte[0], 0);
            }

            if (text.Length > MaxTextLength)
            {
                text = "Text is too long";
            }

            // Calculate "bitmap" width for mono space font
            var renderWidth = 0;
            foreach (var c in text)
            {
                if (c == ' ')
                {
                    renderWidth += WordGap;
                }
                else
                {
                    renderWidth += _font.Width + CharGap;
                }
            }

            // remove last gap
            renderWidth -= CharGap;

            // Reserve space for the rendered bitmap
            var matrix = new byte[renderWidth * CharHeight];

            var x = 0;
            foreach (var c in text)
            {
                if (c == ' ')
                {
                    x += WordGap;
                    continue;
                }

                _font.GetCharData(c, out var glyph);
                for (var cy = 0; cy < _font.Height; cy++)
                {
                    var bitPattern = glyph[cy];
                    // The letter is right-aligned within the 8x8 matrix; evaluate from bit 3 onward
                    byte flag = 0x80 >> 3;
                    for (var cx = 3; cx < 8; cx++)
                    {
                        if ((bitPattern & flag) != 0)
                        {
                            // Set value to 1 to indicate that a pixel should be "on".
                            matrix[x + cx - 3 + cy * renderWidth] = 1;
                        }

                        flag >>= 1;
                    }
                }

                x += _font.Width + CharGap;
            }

            return new SenseHatTextRenderMatrix(text, matrix, renderWidth);
        }
    }
}

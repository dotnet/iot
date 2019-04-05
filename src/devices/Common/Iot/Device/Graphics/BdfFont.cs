// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Graphics
{
    public class BdfFont
    {
        public int Width  { private set; get; }
        public int Height { private set; get; }
        public int XDisplacement { private set; get; }
        public int YDisplacement { private set; get; }
        public int DefaultChar  { private set; get; }
        public int CharsCount   { private set; get; }

        // GlyphMapper is mapping from the character number to the index of the character bitmap data in the buffer GlyphUshortData.
        private Dictionary<int, int> GlyphMapper { set; get; }
        private int BytesPerGlyph { set; get; }
        private ushort[] GlyphUshortData { set; get; }

        private static readonly string s_fontBoundingBox    = "FONTBOUNDINGBOX ";
        private static readonly string s_charSet            = "CHARSET_REGISTRY ";
        private static readonly string s_isoCharset         = "\"ISO10646\"";
        private static readonly string s_defaultChar        = "DEFAULT_CHAR ";
        private static readonly string s_Chars              = "CHARS ";
        private static readonly string s_startChar          = "STARTCHAR ";
        private static readonly string s_encoding           = "ENCODING ";
        // private static readonly string s_sWidth             = "SWIDTH";
        // private static readonly string s_dWidth             = "DWIDTH";
        private static readonly string s_bbx                = "BBX ";
        // private static readonly string s_vVector            = "VVECTOR";
        private static readonly string s_endChar            = "ENDCHAR";
        private static readonly string s_bitmap             = "BITMAP";

        private BdfFont() { }

        public static BdfFont Load(string fontFilePath)
        {
            using (StreamReader sr = new StreamReader(fontFilePath))
            {
                BdfFont font = new BdfFont();
                while (!sr.EndOfStream)
                {
                    ReadOnlySpan<char> span = sr.ReadLine().AsSpan().Trim();
                    if (span.StartsWith(s_fontBoundingBox, StringComparison.Ordinal))
                    {
                        span = span.Slice(s_fontBoundingBox.Length).Trim();
                        font.Width = ReadNextDecimalNumber(ref span);
                        font.Height = ReadNextDecimalNumber(ref span);
                        font.XDisplacement = ReadNextDecimalNumber(ref span);
                        font.YDisplacement = ReadNextDecimalNumber(ref span);
                        font.BytesPerGlyph = (int)Math.Ceiling(((double)font.Width) / 8);
                    }
                    else if (span.StartsWith(s_charSet, StringComparison.Ordinal))
                    {
                        span = span.Slice(s_charSet.Length).Trim();
                        if (span.CompareTo(s_isoCharset, StringComparison.Ordinal) != 0)
                        {
                            throw new NotSupportedException("We only support ISO10646 for now.");
                        }
                    }
                    else if (span.StartsWith(s_defaultChar, StringComparison.Ordinal))
                    {
                        span = span.Slice(s_defaultChar.Length).Trim();
                        font.DefaultChar = ReadNextDecimalNumber(ref span);
                    }
                    else if (span.StartsWith(s_Chars, StringComparison.Ordinal))
                    {
                        span = span.Slice(s_Chars.Length).Trim();
                        font.CharsCount = ReadNextDecimalNumber(ref span);

                        if (font.Width == 0 || font.Height == 0 || font.CharsCount <= 0)
                        {
                            throw new InvalidDataException("The font data is not well formed.");
                        }

                        font.ReadGlyphsData(sr);
                    }
                }

                return font;
            }
        }

        public void GetCharData(char c, out ReadOnlySpan<ushort> charData)
        {
            if (!GlyphMapper.TryGetValue((int)c, out int index))
            {
                if (!GlyphMapper.TryGetValue((int)DefaultChar, out index))
                {
                    throw new InvalidDataException("Couldn't get the glyph data");
                }
            }

            charData = GlyphUshortData.AsSpan().Slice(index, Height);
        }

        public int[] SupportedChars
        {
            get
            {
                Dictionary<int, int>.KeyCollection collection = GlyphMapper.Keys;
                int[] values = new int[collection.Count];
                collection.CopyTo(values, 0);
                return values;
            }
        }

        public bool GetCharData(int charOrdinal, ref Span<int> data, bool useDefaultChar = true)
        {
            if (data.Length < Height)
            {
                return false;
            }

            if (!GlyphMapper.TryGetValue(charOrdinal, out int index))
            {
                if (useDefaultChar)
                {
                    if (!GlyphMapper.TryGetValue(DefaultChar, out index))
                    {
                        return false;
                    }
                }
            }

            for (int i = 0; i < Height; i++)
            {
                data[i] = GlyphUshortData[index + i];
            }

            return true;
        }

        private void ReadGlyphsData(StreamReader sr)
        {
            GlyphMapper = new Dictionary<int, int>();
            if (BytesPerGlyph <= 2)
            {
                GlyphUshortData = new ushort[CharsCount * Height];
            }
            else
            {
                throw new NotSupportedException("Fonts with width more than 16 pixels is not supported.");
            }

            int index = 0;
            for (int i = 0; i < CharsCount; i++)
            {
                ReadOnlySpan<char> span = sr.ReadLine().AsSpan().Trim();
                if (!span.StartsWith(s_startChar, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The font data is not well formed. expected STARTCHAR tag in the beginning of glyoh data.");
                }

                span = sr.ReadLine().AsSpan().Trim();
                if (!span.StartsWith(s_encoding, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The font data is not well formed. expected ENCODING tag.");
                }
                span = span.Slice(s_encoding.Length).Trim();
                int charNumber = ReadNextDecimalNumber(ref span);
                GlyphMapper.Add(charNumber, index);

                do
                {
                    span = sr.ReadLine().AsSpan().Trim();
                } while (!span.StartsWith(s_bbx, StringComparison.Ordinal));

                span = span.Slice(s_bbx.Length).Trim();
                if (ReadNextDecimalNumber(ref span) != Width ||
                    ReadNextDecimalNumber(ref span) != Height ||
                    ReadNextDecimalNumber(ref span) != XDisplacement ||
                    ReadNextDecimalNumber(ref span) != YDisplacement )
                {
                    throw new NotSupportedException("We don't support fonts have BBX values different than FONTBOUNDINGBOX values.");
                }

                span = sr.ReadLine().AsSpan().Trim();
                if (span.CompareTo(s_bitmap, StringComparison.Ordinal) != 0)
                {
                    throw new InvalidDataException("The font data is not well formed. expected BITMAP tag.");
                }

                span = sr.ReadLine().AsSpan().Trim();
                int heightData = 0;
                while (heightData < Height)
                {
                    if (span.Length > 0)
                    {
                        int data = ReadNextHexaDecimalNumber(ref span);
                        GlyphUshortData[index] = (ushort)data;

                        heightData++;
                        index++;
                    }
                    else
                    {
                        span = sr.ReadLine().AsSpan().Trim();
                    }
                }

                span = sr.ReadLine().AsSpan().Trim();
                if (!span.StartsWith(s_endChar, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The font data is not well formed. expected ENDCHAR tag in the beginning of glyoh data.");
                }
            }
        }

        private static int ReadNextDecimalNumber(ref ReadOnlySpan<char> span)
        {
            span = span.Trim();

            int sign = 1;
            if (span.Length > 0 && span[0] == '-')
            {
                sign = -1;
                span = span.Slice(1);
            }

            int number = 0;
            while (span.Length > 0 && ((uint)(span[0] - '0')) <= 9)
            {
                number = number * 10 + (span[0] - '0');
                span = span.Slice(1);
            }

            return number * sign;
        }

        private static int ReadNextHexaDecimalNumber(ref ReadOnlySpan<char> span)
        {
            span = span.Trim();

            int number = 0;
            while (span.Length > 0)
            {
                if ((uint)(span[0] - '0') <= 9)
                {
                    number = number * 16 + (span[0] - '0');
                    span = span.Slice(1);
                    continue;
                }
                else if ((uint)(Char.ToLowerInvariant(span[0]) - 'a') <= ((uint)('f' - 'a')))
                {
                    number = number * 16 + (Char.ToLowerInvariant(span[0]) - 'a') + 10;
                    span = span.Slice(1);
                    continue;
                }
                else
                {
                    break;
                }
            }

            return number;
        }
    }
}

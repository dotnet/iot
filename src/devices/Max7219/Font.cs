// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Max7219
{
    public class Font
    {
        readonly byte[][] _bitmap;

        public Font(params byte[][] bitmap)
        {
            _bitmap = bitmap;
        }

        public byte[] GetItem(char chr)
        {
            var bitmap = _bitmap[chr];
            if (chr == ' ')
            {
                return new byte[Trim(_bitmap['l']).Length];
            }
            return Trim(bitmap);
        }

        byte[] Trim(byte[] bitmap)
        {
            var start = 0;
            var end = bitmap.Length;
            while (start < end && bitmap[start] == 0)
                start++;
            while (end > start && bitmap[end - 1] == 0)
                end--;
            if (start > 0 || end < bitmap.Length)
                return new Span<byte>(bitmap).Slice(start, end - start).ToArray();
            return bitmap;
        }
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Graphics;
using System.Drawing;

namespace Iot.Device.Ws28xx
{
    /// <summary>
    /// Special 24bit RGB format for Neo pixel LEDs where each bit is converted to 3 bits.
    /// A one is converted to 110, a zero is converted to 100.
    /// </summary>
    public class BitmapImageNeo3 : BitmapImage
    {
        private const int BytesPerComponent = 3;
        private const int BytesPerPixel = BytesPerComponent * 3;
        // The Neo Pixels require a 50us delay (all zeros) after. Since Spi freq is not exactly
        // as requested 100us is used here with good practical results. 100us @ 2.4Mbps and 8bit
        // data means we have to add 30 bytes of zero padding.
        private const int ResetDelayInBytes = 30;

        public BitmapImageNeo3(int width, int height)
            : base(new byte[width * height * BytesPerPixel + ResetDelayInBytes], width, height, width * BytesPerPixel)
        {
        }

        public override void SetPixel(int x, int y, Color c)
        {
            var offset = y * Stride + x * BytesPerPixel;
            Data[offset++] = _lookup[c.G * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.G * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.G * BytesPerComponent + 2];
            Data[offset++] = _lookup[c.R * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.R * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.R * BytesPerComponent + 2];
            Data[offset++] = _lookup[c.B * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.B * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.B * BytesPerComponent + 2];
        }

        private static readonly byte[] _lookup = new byte[256 * BytesPerComponent];
        static BitmapImageNeo3()
        {
            for (int i = 0; i < 256; i++)
            {
                int data = 0;
                for (int j = 7; j >= 0; j--)
                {
                    data = (data << 3) | 0b100 | ((i >> j) << 1) & 2;
                }
                _lookup[i * BytesPerComponent + 0] = unchecked((byte)(data >> 16));
                _lookup[i * BytesPerComponent + 1] = unchecked((byte)(data >> 8));
                _lookup[i * BytesPerComponent + 2] = unchecked((byte)(data >> 0));
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using Iot.Device.Graphics;
namespace Iot.Device.Ws28xx
{
    internal class BitmapImageNeo4 : RawPixelContainer
    {
        private const int BytesPerComponent = 3;
        private const int BytesPerPixel = BytesPerComponent * 4;

        // The Neo Pixels require a 50us delay (all zeros) after. Since Spi freq is not exactly
        // as requested 100us is used here with good practical results. 100us @ 2.4Mbps and 8bit
        // data means we have to add 30 bytes of zero padding.
        private const int ResetDelayInBytes = 30;

        // This field defines the count within the lookup table. The length correlates to the possible values of a single byte.
        private const int LookupCount = 256;

        private static readonly byte[] _lookup = new byte[LookupCount * BytesPerComponent];

        static BitmapImageNeo4()
        {
            for (int i = 0; i < LookupCount; i++)
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

        public BitmapImageNeo4(int width, int height)
                            : base(new byte[width * height * BytesPerPixel + ResetDelayInBytes], width, height, width * BytesPerPixel)
        {
        }

        public override void SetPixel(int x, int y, Color c)
        {
            // Alpha is used as white.
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
            Data[offset++] = _lookup[c.A * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.A * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.A * BytesPerComponent + 2];
        }
    }
}

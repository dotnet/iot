// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;

namespace Iot.Device.Ws28xx
{
    /// <summary>
    /// Special 24bit RGB format for Neo pixel LEDs where each bit is converted to 3 bits.
    /// A one is converted to 110, a zero is converted to 100.
    /// </summary>
    /// <seealso cref="Iot.Device.Ws28xx.BitmapImageNeo3" />
    internal class BitmapImageNeo3Rgb : BitmapImageNeo3
    {
        public BitmapImageNeo3Rgb(int width, int height)
            : base(width, height)
        {
        }

        public override void SetPixel(int x, int y, Color c)
        {
            var offset = y * Stride + x * BytesPerPixel;
            Data[offset++] = _lookup[c.R * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.R * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.R * BytesPerComponent + 2];
            Data[offset++] = _lookup[c.G * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.G * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.G * BytesPerComponent + 2];
            Data[offset++] = _lookup[c.B * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.B * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.B * BytesPerComponent + 2];
        }
    }
}

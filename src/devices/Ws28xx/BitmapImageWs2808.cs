// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Graphics;
using System.Drawing;

namespace Iot.Device.Ws28xx
{
    internal class BitmapImageWs2808 : BitmapImage
    {
        private const int BytesPerPixel = 3;
        
        public BitmapImageWs2808(int width, int height)
            : base(new byte[width * height * BytesPerPixel], width, height, width * BytesPerPixel)
        {
        }

        public override void SetPixel(int x, int y, Color c)
        {
            var offset = y * Stride + x * BytesPerPixel;
            Data[offset++] = c.R;
            Data[offset++] = c.G;
            Data[offset++] = c.B;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;

namespace Iot.Device.Graphics
{
    public abstract class BitmapImage
    {
        protected BitmapImage(byte[] data, int width, int height, int stride)
        {
            _data = data;
            Width = width;
            Height = height;
            Stride = stride;
        }

        private readonly byte[] _data;
        public Span<byte> Data => _data;
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }

        public abstract void SetPixel(int x, int y, Color c);

        public virtual void Clear(Color c = default)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetPixel(x, y, c);
                }
            }
        }
    }
}

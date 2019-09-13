// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// Represents bitmap image
    /// </summary>
    public abstract class BitmapImage
    {
        /// <summary>
        /// Initializes a <see cref="T:Iot.Device.Graphics.BitmapImage" /> instance with the specified data, width, height and stride.
        /// </summary>
        /// <param name="data">Data representing the image (derived class defines a specific format)</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="stride">Number of bytes per row</param>
        protected BitmapImage(byte[] data, int width, int height, int stride)
        {
            _data = data;
            Width = width;
            Height = height;
            Stride = stride;
        }

        private readonly byte[] _data;

        /// <summary>
        /// Data related to the image (derived class defines a specific format)
        /// </summary>
        public Span<byte> Data => _data;

        /// <summary>
        /// Width of the image
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the image
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Number of bytes per row
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// Sets pixel at specific position
        /// </summary>
        /// <param name="x">X coordinate of the pixel</param>
        /// <param name="y">Y coordinate of the pixel</param>
        /// <param name="color">Color to set the pixel to</param>
        public abstract void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Clears the image to specific color
        /// </summary>
        /// <param name="color">Color to clear the image. Defaults to black.</param>
        public virtual void Clear(Color color = default)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetPixel(x, y, color);
                }
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;

namespace Iot.Device.Ws28xx
{
    /// <summary>
    /// An abstract class that acts as a data container for device-dependent pixel arrays
    /// </summary>
    public abstract class RawPixelContainer
    {
        private byte[] _data;

        /// <summary>
        /// Constructs a container with the given values
        /// </summary>
        /// <param name="data">The data array. Should be of size <paramref name="width"/> * <paramref name="height"/> * bytesPerPixel</param>
        /// <param name="width">Width of the image, in pixels</param>
        /// <param name="height">Height of the image, in pixels</param>
        /// <param name="stride">Image width * bytes per pixel (number of bytes per image scanline)</param>
        protected RawPixelContainer(byte[] data, int width, int height, int stride)
        {
            _data = data;
            Width = width;
            Height = height;
            Stride = stride;
        }

        /// <summary>
        /// Image width
        /// </summary>
        public int Width
        {
            get;
        }

        /// <summary>
        /// Image Height
        /// </summary>
        public int Height
        {
            get;
        }

        /// <summary>
        /// Image Stride (number of bytes per scanline)
        /// </summary>
        public int Stride
        {
            get;
        }

        /// <summary>
        /// Retrieves the raw pixels. Addressing and color format is device-dependent!
        /// </summary>
        public Span<byte> Data => _data;

        /// <summary>
        /// Sets a pixel to a specific color
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="c">Color to set, will be adjusted to the color format of the target hardware</param>
        public abstract void SetPixel(int x, int y, Color c);

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

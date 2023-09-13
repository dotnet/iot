// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// Factory interface for creating bitmaps.
    /// An instance of this interface shall be provided by library-specific adapter classes.
    /// The class <see cref="BitmapImage"/> requires an instance of this interface to work properly.
    /// </summary>
    public interface IImageFactory
    {
        /// <summary>
        /// Creates a bitmap with the given size and format
        /// </summary>
        /// <param name="width">Width of the bitmap, in pixels</param>
        /// <param name="height">Height of the bitmap in pixels</param>
        /// <param name="pixelFormat">The desired pixel format</param>
        /// <returns>An empty bitmap of the given size</returns>
        BitmapImage CreateBitmap(int width, int height, PixelFormat pixelFormat);

        /// <summary>
        /// Creates a bitmap from a stream (e.g. an image file)
        /// </summary>
        /// <param name="file">The stream to open</param>
        /// <returns>A bitmap from the given stream</returns>
        BitmapImage CreateFromStream(Stream file);
    }
}

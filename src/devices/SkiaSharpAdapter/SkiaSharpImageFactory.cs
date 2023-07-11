// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Iot.Device.Graphics.SkiaSharpAdapter
{
    /// <summary>
    /// This image factory uses SkiaSharp as backend
    /// </summary>
    public class SkiaSharpImageFactory : IImageFactory
    {
        /// <inheritdoc />
        public BitmapImage CreateBitmap(int width, int height, PixelFormat pixelFormat)
        {
            return new SkiaSharpBitmap(width, height, pixelFormat);
        }

        /// <inheritdoc />
        public BitmapImage CreateFromStream(Stream file)
        {
            using var image = SKImage.FromEncodedData(file);
            PixelFormat pf;
            if (image.ColorType == SKColorType.Rgb888x)
            {
                pf = PixelFormat.Format32bppXrgb;
            }
            else if (image.ColorType == SKColorType.Bgra8888)
            {
                pf = PixelFormat.Format32bppArgb;
            }
            else
            {
                throw new NotSupportedException($"The stream contains an image with color type {image.ColorType}, which is currently not supported");
            }

            return new SkiaSharpBitmap(SKBitmap.FromImage(image), pf);
        }
    }
}

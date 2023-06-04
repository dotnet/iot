// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Iot.Device.Graphics.SkiaSharpAdapter
{
    internal class SkiaSharpBitmap : BitmapImage
    {
        private SKBitmap _bitmap;

        public SkiaSharpBitmap(int width, int height, PixelFormat pixelFormat)
            : base(width, height, width * 4, pixelFormat)
        {
            if (pixelFormat == PixelFormat.Format32bppArgb)
            {
                _bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            }
            else if (pixelFormat == PixelFormat.Format32bppXrgb)
            {
                _bitmap = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);
            }
            else
            {
                throw new NotSupportedException($"Pixelformat {pixelFormat} is not currently supported");
            }
        }

        internal SKBitmap WrappedBitmap => _bitmap;

        public SkiaSharpBitmap(SKBitmap bitmap, PixelFormat pixelFormat)
            : base(bitmap.Width, bitmap.Height, bitmap.Width * 4, pixelFormat)
        {
            _bitmap = bitmap;
        }

        public override void SetPixel(int x, int y, Color color)
        {
            _bitmap.SetPixel(x, y, ConvertColor(color));
        }

        public override Color GetPixel(int x, int y)
        {
            return ConvertColor(_bitmap.GetPixel(x, y));
        }

        public override unsafe Span<byte> AsByteSpan()
        {
            // GetPixelSpan() returns a ReadonlySpan for some reason, therefore work around this.
            IntPtr ptr = IntPtr.Zero;
            ptr = _bitmap.GetPixels(out IntPtr length);
            return new Span<byte>(ptr.ToPointer(), length.ToInt32());
        }

        public override void SaveToStream(Stream stream, ImageFileType fileType)
        {
            using var img = SKImage.FromBitmap(WrappedBitmap);

            SKEncodedImageFormat encodingFormat = fileType switch
            {
                ImageFileType.Bmp => SKEncodedImageFormat.Bmp,
                ImageFileType.Png => SKEncodedImageFormat.Png,
                ImageFileType.Jpg => SKEncodedImageFormat.Jpeg,
                _ => throw new NotSupportedException($"File type {fileType} is not supported")
            };

            var data = img.Encode(encodingFormat, 100);
            data.SaveTo(stream);
        }

        private SKColor ConvertColor(Color c)
        {
            return new SKColor(c.R, c.G, c.B, c.A);
        }

        private Color ConvertColor(SKColor color)
        {
            return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        }

        protected override void Dispose(bool disposing)
        {
            _bitmap?.Dispose();
            _bitmap = null!;
        }

        public override IGraphics GetDrawingApi()
        {
            return new BitmapCanvas(_bitmap);
        }

        internal class BitmapCanvas : IGraphics
        {
            private SKCanvas _canvas;
            private SKBitmap _bitmap;

            public BitmapCanvas(SKBitmap bitmap)
            {
                _bitmap = bitmap;
                _canvas = new SKCanvas(_bitmap);
            }

            public SKCanvas Canvas => _canvas;

            public void Dispose()
            {
                // Do not dispose the bitmap, it's not ours
                _canvas.Dispose();
            }
        }
    }
}

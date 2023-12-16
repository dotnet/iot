// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// Contains a set of converters from <see cref="System.Drawing.Bitmap"/> to <see cref="BitmapImage"/>, for
    /// easy conversion of legacy code that still uses the obsolete System.Drawing library
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Convert a <see cref="System.Drawing.Bitmap"/> to a <see cref="BitmapImage"/>
        /// </summary>
        /// <param name="bmp">Input bitmap</param>
        /// <returns>An image</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bmp"/> is null</exception>
        /// <exception cref="NotSupportedException">The input format is not supported</exception>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public static unsafe BitmapImage ToBitmapImage(Bitmap bmp)
        {
            if (bmp == null)
            {
                throw new ArgumentNullException(nameof(bmp));
            }

            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new PlatformNotSupportedException("This operation is only supported on Windows");
            }

            if (bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                var target = BitmapImage.CreateBitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);

                var bmd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                for (int i = 0; i < bmp.Height; i++)
                {
                    IntPtr offsetSource = bmd.Scan0 + (bmd.Stride * i);
                    void* sourcePtr = offsetSource.ToPointer();
                    Span<byte> source = new Span<byte>(sourcePtr, bmd.Stride);
                    Span<byte> dest = target.AsByteSpan().Slice(bmd.Stride * i, bmd.Stride);
                    source.CopyTo(dest);
                }

                bmp.UnlockBits(bmd);

                return target;
            }

            throw new NotSupportedException($"Converting images of type {bmp.PixelFormat} is not supported");
        }

        /// <summary>
        /// Adjusts the target position and size so that a given image can be copied to a target image without scaling and without further cropping.
        /// This ensures the destination rectangle, starting at the given point lies within the image.
        /// </summary>
        /// <param name="image">The input image size</param>
        /// <param name="leftTop">[in, out] The top left corner of the input image to show. If at the bottom or right edge of the destination, this will be
        /// reset so that the right edge of the input image is at the right edge of the destination</param>
        /// <param name="destination">The destination rectangle. If this is larger than the input image, the size will be cropped</param>
        public static void AdjustImageDestination(BitmapImage image, ref Point leftTop, ref Rectangle destination)
        {
            int left = leftTop.X;
            int top = leftTop.Y;

            if (destination.Width > image.Width)
            {
                // Rectangle is a struct, so this has no effect on the caller (yet)
                destination.Width = image.Width;
            }

            if (destination.Height > image.Height)
            {
                destination.Height = image.Height;
            }

            if (left < 0)
            {
                left = 0;
            }

            if (left > image.Width - destination.Width)
            {
                left = image.Width - destination.Width;
            }

            if (top < 0)
            {
                top = 0;
            }

            if (top > image.Height - destination.Height)
            {
                top = image.Height - destination.Height;
            }

            leftTop = new Point(left, top);
            destination = new Rectangle(destination.X, destination.Y, destination.Width, destination.Height);
        }
    }
}

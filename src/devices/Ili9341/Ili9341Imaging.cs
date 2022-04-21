// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Iot.Device.Ili9341
{
    public partial class Ili9341
    {
        /// <summary>
        /// Send a bitmap to the Ili9341 display specifying the starting position and destination clipping rectangle.
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller note that only Pixel Format Format32bppArgb is supported.</param>
        public void SendBitmap(Bitmap bm)
        {
            SendBitmap(bm, new Point(0, 0), new Rectangle(0, 0, ScreenWidthPx, ScreenHeightPx));
        }

        /// <summary>
        /// Send a bitmap to the Ili9341 display specifying the starting position and destination clipping rectangle.
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller note that only Pixel Format Format32bppArgb is supported.</param>
        /// <param name="updateRect">A rectangle that defines where in the display the bitmap is written. Note that no scaling is done.</param>
        public void SendBitmap(Bitmap bm, Rectangle updateRect)
        {
            SendBitmap(bm, new Point(updateRect.X, updateRect.Y), updateRect);
        }

        /// <summary>
        /// Send a bitmap to the Ili9341 display specifying the starting position and destination clipping rectangle.
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller note that only Pixel Format Format32bppArgb is supported.</param>
        /// <param name="sourcePoint">A coordinate point in the source bitmap where copying starts from.</param>
        /// <param name="destinationRect">A rectangle that defines where in the display the bitmap is written. Note that no scaling is done.</param>
        public void SendBitmap(Bitmap bm, Point sourcePoint, Rectangle destinationRect)
        {
            if (bm is null)
            {
                throw new ArgumentNullException(nameof(bm));
            }

            if (bm.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArgumentException($"Pixel format {bm.PixelFormat.ToString()} not supported.", nameof(bm));
            }

            // get the pixel data and send it to the display
            SendBitmapPixelData(GetBitmapPixelData(bm, new Rectangle(sourcePoint.X, sourcePoint.Y, destinationRect.Width, destinationRect.Height)), destinationRect);
        }

        /// <summary>
        /// Convert a bitmap into an array of pixel data suitable for sending to the display
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller note that only Pixel Format Format32bppArgb is supported.</param>
        /// <param name="sourceRect">A rectangle that defines where in the bitmap data is to be converted from.</param>
        public Span<byte> GetBitmapPixelData(Bitmap bm, Rectangle sourceRect)
        {
            BitmapData bmd;
            byte[] bitmapData; // array that takes the raw bytes of the bitmap
            byte[] outputBuffer; // array used to form the data to be written out to the SPI interface

            if (bm is null)
            {
                throw new ArgumentNullException(nameof(bm));
            }

            if (bm.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArgumentException($"Pixel format {bm.PixelFormat.ToString()} not supported.", nameof(bm));
            }

            // allocate the working arrays.
            bitmapData = new byte[sourceRect.Width * sourceRect.Height * 4];
            outputBuffer = new byte[sourceRect.Width * sourceRect.Height * 2];

            // get the raw pixel data for the bitmap
            bmd = bm.LockBits(sourceRect, ImageLockMode.ReadOnly, bm.PixelFormat);

            Marshal.Copy(bmd.Scan0, bitmapData, 0, bitmapData.Length);

            bm.UnlockBits(bmd);

            // iterate over the source bitmap converting each pixle in the raw data
            // to a format suitablle for sending to the display
            for (int i = 0; i < bitmapData.Length; i += 4)
            {
                    (outputBuffer[i / 4 * 2 + 0], outputBuffer[i / 4 * 2 + 1]) = Color565(Color.FromArgb(bitmapData[i + 2], bitmapData[i + 1], bitmapData[i + 0]));
            }

            return (outputBuffer);
        }

        /// <summary>
        /// Send an array of pixel data to the display.
        /// </summary>
        /// <param name="pixelData">The data to be sent to the display.</param>
        /// <param name="destinationRect">A rectangle that defines where in the display the data is to be written.</param>
        public void SendBitmapPixelData(Span<byte> pixelData, Rectangle destinationRect)
        {
            SetWindow((uint)destinationRect.X, (uint)destinationRect.Y, (uint)(destinationRect.Right - 1), (uint)(destinationRect.Bottom - 1));   // specifiy a location for the rows and columns on the display where the data is to be written
            SendData(pixelData);
        }
    }
}

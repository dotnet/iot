// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Iot.Device.Ssd1351
{
    public partial class Ssd1351 : IDisposable
    {
        /// <summary>
        /// Send a bitmap to the ssd1351 display specifying the starting position and destination clipping rectangle.
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller note that only Pixel Format Format32bppArgb is supported.</param>
        public void SendBitmap(Bitmap bm)
        {
            SendBitmap(bm, new Point(0, 0), new Rectangle(0, 0, ScreenWidthPx, ScreenWidthPx));
        }

        /// <summary>
        /// Send a bitmap to the ssd1351 display specifying the starting position and destination clipping rectangle.
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller note that only Pixel Format Format32bppArgb is supported.</param>
        /// <param name="updateRect">A rectangle that defines where in the display the bitmap is written. Note that no scaling is done.</param>
        public void SendBitmap(Bitmap bm, Rectangle updateRect)
        {
            SendBitmap(bm, new Point(updateRect.X, updateRect.Y), updateRect);
        }

        /// <summary>
        /// Send a bitmap to the ssd1351 display specifying the starting position and destination clipping rectangle.
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller note that only Pixel Format Format32bppArgb is supported.</param>
        /// <param name="sourcePoint">A coordinate point in the source bitmap where copying starts from.</param>
        /// <param name="destinationRect">A rectangle that defines where in the display the bitmap is written. Note that no scaling is done.</param>
        public void SendBitmap(Bitmap bm, Point sourcePoint, Rectangle destinationRect)
        {
            if (bm == null)
            {
                throw new ArgumentNullException(nameof(bm));
            }

            if (sourcePoint == null)
            {
                throw new ArgumentNullException(nameof(sourcePoint));
            }

            if (destinationRect == null)
            {
                throw new ArgumentNullException(nameof(destinationRect));
            }

            if (bm.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArgumentException($"Pixel format {bm.PixelFormat.ToString()} not supported.", nameof(bm.PixelFormat));
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

            if (bm == null)
            {
                throw new ArgumentNullException(nameof(bm));
            }

            if (sourceRect == null)
            {
                throw new ArgumentNullException(nameof(sourceRect));
            }

            if (bm.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArgumentException($"Pixel format {bm.PixelFormat.ToString()} not supported.", nameof(bm.PixelFormat));
            }

            // allocate the working arrays.
            bitmapData = new byte[sourceRect.Width * sourceRect.Height * 4];
            outputBuffer = new byte[sourceRect.Width * sourceRect.Height * (_colorDepth == ColorDepth.ColourDepth65K ? 2 : 3)];

            // get the raw pixel data for the bitmap
            bmd = bm.LockBits(sourceRect, ImageLockMode.ReadOnly, bm.PixelFormat);

            Marshal.Copy(bmd.Scan0, bitmapData, 0, bitmapData.Length);

            bm.UnlockBits(bmd);

            // iterate over the source bitmap converting each pixle in the raw data
            // to a format suitablle for sending to the display
            for (int i = 0; i < bitmapData.Length; i += 4)
            {
                if (_colorDepth == ColorDepth.ColourDepth65K)
                {
                    (outputBuffer[i / 4 * 2 + 0], outputBuffer[i / 4 * 2 + 1]) = Color565(Color.FromArgb(bitmapData[i + 2], bitmapData[i + 1], bitmapData[i + 0]));
                }
                else
                {
                    outputBuffer[i / 4 * 3 + 0] = (byte)(bitmapData[i + (_colorSequence == ColorSequence.BGR ? 0 : 2)] >> 2);
                    outputBuffer[i / 4 * 3 + 1] = (byte)(bitmapData[i + 1] >> 2);
                    outputBuffer[i / 4 * 3 + 2] = (byte)(bitmapData[i + (_colorSequence == ColorSequence.BGR ? 2 : 0)] >> 2);
                }
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
            // specifiy a location for the rows and columns on the display where the data is to be written
            SetColumnAddress((byte)destinationRect.X, (byte)(destinationRect.Right - 1));
            SetRowAddress((byte)destinationRect.Y, (byte)(destinationRect.Bottom - 1));

            // write out the pixel data
            SendCommand(Ssd1351Command.WriteRam, pixelData);
        }

    }
}

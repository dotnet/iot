// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using Iot.Device.MemoryLcd;

namespace MemoryLcd.Extends
{
    /// <summary>
    /// Extend methods for LSxxxB7DHxx
    /// </summary>
    public static class LSxxxB7DHxxExtends
    {
        /// <summary>
        /// Show image to device
        /// </summary>
        /// <param name="mlcd">Memory LCD device</param>
        /// <param name="image">Image to show</param>
        /// <param name="split">Number to splits<br/>To avoid buffer overflow exceptions, it needs to split one frame into multiple sends for some device.</param>
        /// <param name="frameInversion">Frame inversion flag</param>
        public static void ShowImage(this LSxxxB7DHxx mlcd, Bitmap image, int split = 1, bool frameInversion = false)
        {
            mlcd.FillImageBufferWithImage(image);

            int linesToSend = mlcd.PixelHeight / split;
            int bytesToSend = mlcd._frameBuffer.Length / split;

            for (int fs = 0; fs < split; fs++)
            {
                Span<byte> lineNumbers = mlcd._lineNumberBuffer.AsSpan(linesToSend * fs, linesToSend);
                Span<byte> bytes = mlcd._frameBuffer.AsSpan(bytesToSend * fs, bytesToSend);

                mlcd.DataUpdateMultipleLines(lineNumbers, bytes, frameInversion);
            }
        }

        private static void FillImageBufferWithImage(this LSxxxB7DHxx mlcd, Bitmap image)
        {
            if (image.Width != mlcd.PixelWidth)
            {
                throw new ArgumentException($"The width of the image should be {mlcd.PixelWidth}", nameof(image));
            }

            if (image.Height != mlcd.PixelHeight)
            {
                throw new ArgumentException($"The height of the image should be {mlcd.PixelHeight}", nameof(image));
            }

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x += 8)
                {
                    int bx = x / 8;
                    byte dataByte = (byte)(
                        (image.GetPixel(x + 0, y).GetBrightness() > 0.5 ? 0b10000000 : 0) |
                        (image.GetPixel(x + 1, y).GetBrightness() > 0.5 ? 0b01000000 : 0) |
                        (image.GetPixel(x + 2, y).GetBrightness() > 0.5 ? 0b00100000 : 0) |
                        (image.GetPixel(x + 3, y).GetBrightness() > 0.5 ? 0b00010000 : 0) |
                        (image.GetPixel(x + 4, y).GetBrightness() > 0.5 ? 0b00001000 : 0) |
                        (image.GetPixel(x + 5, y).GetBrightness() > 0.5 ? 0b00000100 : 0) |
                        (image.GetPixel(x + 6, y).GetBrightness() > 0.5 ? 0b00000010 : 0) |
                        (image.GetPixel(x + 7, y).GetBrightness() > 0.5 ? 0b00000001 : 0));

                    mlcd._frameBuffer[bx + y * mlcd.BytesPerLine] = dataByte;
                }
            }
        }
    }
}

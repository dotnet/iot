// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using Iot.Device.Graphics;

namespace Iot.Device.Ht1632
{
    /// <summary>
    /// Extension for sending image
    /// </summary>
    public static class Ht1632ImageSending
    {
        /// <summary>
        /// Method for whether pixel is lit or not.
        /// </summary>
        /// <param name="pixel">Input pixel</param>
        /// <returns>True if lit.</returns>
        public delegate bool BrightnessConvertor(Color pixel);

        /// <summary>
        /// Show image with 8-Com mode
        /// </summary>
        /// <param name="ht1632">HT1632 device</param>
        /// <param name="image">Image to show. Width at least 8 pixels, height at least 32 pixels </param>
        /// <param name="brightnessConvertor">Method for whether pixel is lit or not. Use <see cref="LinearBrightnessConvertor"/> if null.</param>
        public static void ShowImageWith8Com(this Ht1632 ht1632, BitmapImage image, BrightnessConvertor? brightnessConvertor = null)
            => ht1632.ShowImage(image, 8, 32, brightnessConvertor ?? LinearBrightnessConvertor);

        /// <summary>
        /// Show image with 16-Com mode
        /// </summary>
        /// <param name="ht1632">HT1632 device</param>
        /// <param name="image">Image to show. Width at least 16 pixels, height at least 24 pixels </param>
        /// <param name="brightnessConvertor">Method for whether pixel is lit or not. Use <see cref="LinearBrightnessConvertor"/> if null.</param>
        public static void ShowImageWith16Com(this Ht1632 ht1632, BitmapImage image, BrightnessConvertor? brightnessConvertor = null)
            => ht1632.ShowImage(image, 16, 24, brightnessConvertor ?? LinearBrightnessConvertor);

        /// <summary>
        /// Lit if average value of RGB is greater than half.
        /// </summary>
        /// <param name="pixel">Input pixel</param>
        /// <returns>True if average value of RGB is greater than half.</returns>
        public static bool LinearBrightnessConvertor(Color pixel)
        {
            return pixel.R + pixel.G + pixel.B > (3 * 127);
        }

        private static void ShowImage(this Ht1632 ht1632, BitmapImage image, int com, int row, BrightnessConvertor brightnessConvertor)
        {
            if (image.Width < com || image.Height < row)
            {
                throw new Exception($"Image is too small. Width: {image.Width}/{com}, height: {image.Height}/{row}.");
            }

            var data = new byte[row * com / 4];

            for (var y = 0; y < row; y++)
            {
                for (var x = 0; x < com; x += 4)
                {
                    var value = (byte)(
                        (brightnessConvertor(image[x + 0, y]) ? 0b_1000 : 0) |
                        (brightnessConvertor(image[x + 1, y]) ? 0b_0100 : 0) |
                        (brightnessConvertor(image[x + 2, y]) ? 0b_0010 : 0) |
                        (brightnessConvertor(image[x + 3, y]) ? 0b_0001 : 0));
                    var index = (x + com * y) / 4;
                    data[index] = value;
                }
            }

            ht1632.WriteData(0, data);
        }
    }
}

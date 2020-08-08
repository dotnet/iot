// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Blinkt
{
    /// <summary>
    /// Pimoroni Blinkt! - 8-pixel APA102 LED display
    /// </summary>
    public interface IBlinkt : IDisposable
    {
        /// <summary>
        /// If true, set all LEDs to black and zero brightness on dispose.
        /// </summary>
        bool ClearOnExit { get; set; }

        /// <summary>
        /// Set the brightness for all the LEDs
        /// </summary>
        /// <param name="brightness">The percentage of brightness desired.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void SetBrightness(double brightness);

        /// <summary>
        /// Set all LEDs to black.
        /// </summary>
        void Clear();

        /// <summary>
        /// Displays the buffer on the Blinkt.
        /// </summary>
        void Show();

        /// <summary>
        /// Sets all LEDs to the desired color and brightness.
        /// </summary>
        /// <param name="red">Red color.</param>
        /// <param name="green">Green color.</param>
        /// <param name="blue">Blue color.</param>
        /// <param name="brightness">Brightness percentage.</param>
        void SetAll(byte red, byte green, byte blue, double? brightness = null);

        /// <summary>
        /// Set a specific LED to the desired color and brightness.
        /// </summary>
        /// <param name="pixel">LED to set.</param>
        /// <param name="red">Red color.</param>
        /// <param name="green">Green color.</param>
        /// <param name="blue">Blue color.</param>
        /// <param name="brightness">Brightness percentage.</param>
        void SetPixel(int pixel, byte red, byte green, byte blue, double? brightness = null);
    }
}

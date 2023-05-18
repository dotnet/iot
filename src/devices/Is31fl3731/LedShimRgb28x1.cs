// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Drawing;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents 28x1 RGB matrix, driven by an IS31FL3731 LED chip.
    /// </summary>
    // Product: https://shop.pimoroni.com/products/led-shim
    // Port of: https://github.com/adafruit/Adafruit_CircuitPython_IS31FL3731/blob/main/adafruit_is31fl3731/led_shim.py
    public class LedShimRgb28x1 : Is31fl3731
    {
        /// <summary>
        /// Initialize LED driver.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public LedShimRgb28x1(I2cDevice i2cDevice)
        : base(i2cDevice, 28, 1)
        {
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public static new readonly int DefaultI2cAddress = 0x75;

        /// <inheritdoc/>
        public override int GetLedAddress(int x, int y) => (x, y) switch
        {
            // y == 0
            (< 7, 0) => 118 - x,
            (< 15, 0) => 141 - x,
            (< 21, 0) => 106 + x,
            (21, 0) => 15,
            (_, 0) => x - 14,
            // y == 1
            (< 2, 1) => 69 - x,
            (< 7, 1) => 86 - x,
            (< 12, 1) => 28 - x,
            (< 14, 1) => 45 - x,
            (14, 1) => 47,
            (15, 1) => 41,
            (< 21, 1) => x + 9,
            (21, 1) => 95,
            (< 26, 1) => x + 67,
            (_, 1) => x + 50,
            // y == 2
            (0, 2) => 85,
            (< 7, 2) => 102 - x,
            (< 11, 2) => 44 - x,
            (< 14, 2) => 61 - x,
            (14, 2) => 63,
            (< 17, 2) => 42 + x,
            (< 21, 2) => x + 25,
            (21, 2) => 111,
            (< 27, 2) => x + 83,
            (_, 2) => 93,
            _ => throw new Exception($"The ({x}, {y}) is not supported.")
        };

        /// <summary>
        /// Write RGB value.
        /// </summary>
        /// <param name="x">Horizontal pixel position.</param>
        /// <param name="color">Color value.</param>
        public void WritePixelRgb(int x, Color color)
        {
            this[x, 0] = color.R;
            this[x, 1] = color.G;
            this[x, 2] = color.B;
        }

    }
}

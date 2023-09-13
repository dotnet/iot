// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Drawing;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents 5x5 RGB matrix, driven by an IS31FL3731 LED chip.
    /// </summary>
    // Product: https://shop.pimoroni.com/products/5x5-rgb-matrix-breakout
    // Port of: https://github.com/adafruit/Adafruit_CircuitPython_IS31FL3731/blob/main/adafruit_is31fl3731/rgbmatrix5x5.py
    public class BreakoutRgb5x5 : Is31fl3731
    {
        private byte[][] _addresses = new byte[][]
        {
            new byte[] { 118, 69, 85 },
            new byte[] { 117, 68, 101 },
            new byte[] { 116, 84, 100 },
            new byte[] { 115, 83, 99 },
            new byte[] { 114, 82, 98 },
            new byte[] { 132, 19, 35 },
            new byte[] { 133, 20, 36 },
            new byte[] { 134, 21, 37 },
            new byte[] { 112, 80, 96 },
            new byte[] { 113, 81, 97 },
            new byte[] { 131, 18, 34 },
            new byte[] { 130, 17, 50 },
            new byte[] { 129, 33, 49 },
            new byte[] { 128, 32, 48 },
            new byte[] { 127, 47, 63 },
            new byte[] { 125, 28, 44 },
            new byte[] { 124, 27, 43 },
            new byte[] { 123, 26, 42 },
            new byte[] { 122, 25, 58 },
            new byte[] { 121, 41, 57 },
            new byte[] { 126, 29, 45 },
            new byte[] { 15, 95, 111 },
            new byte[] { 8, 89, 105 },
            new byte[] { 9, 90, 106 },
            new byte[] { 10, 91, 107 }
        };

        /// <summary>
        /// Initialize LED driver.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public BreakoutRgb5x5(I2cDevice i2cDevice)
        : base(i2cDevice, 25, 3)
        {
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public static new readonly int DefaultI2cAddress = 0x74;

        /// <inheritdoc/>
        public override int GetLedAddress(int x, int y) => _addresses[x][y];

        /// <summary>
        /// Write RGB value.
        /// </summary>
        /// <param name="x">Horizontal pixel position.</param>
        /// <param name="y">Vertical pixel position.</param>
        /// <param name="color">Color value.</param>
        public void WritePixelRgb(int x, int y, Color color)
        {
            x += y * 5;
            this[x, 0] = color.R;
            this[x, 1] = color.G;
            this[x, 2] = color.B;
        }
    }
}

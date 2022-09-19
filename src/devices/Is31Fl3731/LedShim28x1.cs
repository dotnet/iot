// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents 28x1 matrix, driven by an IS31FL3731 LED chip.
    /// </summary>
    // Product: https://shop.pimoroni.com/products/led-shim
    public class LedShim28x1 : Is31Fl3731
    {
        /// <summary>
        /// Initialize LED driver.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public LedShim28x1(I2cDevice i2cDevice)
        : base(i2cDevice, 28, 1)
        {
        }

        /// <summary>
        /// Write RGB value.
        /// </summary>
        public void WritePixelRgb(int x, int r, int g, int b)
        {
            this[x, 0] = (byte)r;
            this[x, 1] = (byte)g;
            this[x, 2] = (byte)b;
        }

        /// <inheritdoc/>
        public override int GetLedAddress(int x, int y) => (x, y) switch
        {
            (< 7, 0) => 118 - x,
            (< 15, 0) => 141 - x,
            (< 21, 0) => 106 + x,
            (21, 0) => 15,
            (_, 0) => x - 14,
            (<2, 1) => 69 - x,
            (<7, 1) => 86 - x,
            (<12, 1) => 28 - x,
            (<14, 1) => 45 - x,
            (14, 1) => 47,
            (15, 1) => 41,
            (< 21, 1) => x + 9,
            (21, 1) => 95,
            (<26, 1) => x + 67,
            (_, 1) => x + 50,
            (0, _) => 85,
            (<7, _) => 102 - x,
            (<11, _) => 44 - x,
            (<14, _) => 61 - x,
            (14, _) => 63,
            (<17, _) => 42 + x,
            (<21, _) => x + 25,
            (21, _) => 111,
            (<27, _) => x + 83,
            _ => 93
        };
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents 11x7 matrix, driven by an IS31FL3731 LED chip.
    /// </summary>
    // Product: https://shop.pimoroni.com/products/11x7-led-matrix-breakout
    // Port of: https://github.com/adafruit/Adafruit_CircuitPython_IS31FL3731/blob/main/adafruit_is31fl3731/matrix_11x7.py
    public class Breakout11x7 : Is31fl3731
    {
        /// <summary>
        /// Initialize LED driver.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public Breakout11x7(I2cDevice i2cDevice)
        : base(i2cDevice, 11, 7)
        {
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public static new readonly int DefaultI2cAddress = 0x75;

        /// <inheritdoc/>
        public override int GetLedAddress(int x, int y) => (x << 4) - y + (x <= 5 ? 6 : -82);
    }
}

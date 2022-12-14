// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents 16x8 matrix, driven by an IS31FL3731 LED chip.
    /// </summary>
    // Product: https://shop.pimoroni.com/products/scroll-phat-hd
    // Product: https://shop.pimoroni.com/products/scroll-hat-mini
    // Port of: https://github.com/adafruit/Adafruit_CircuitPython_IS31FL3731/blob/main/adafruit_is31fl3731/matrix_11x7.py
    public class ScrollPhat17x7 : Is31fl3731
    {
        /// <summary>
        /// Initialize LED driver.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public ScrollPhat17x7(I2cDevice i2cDevice)
        : base(i2cDevice, 17, 7)
        {
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public static new readonly int DefaultI2cAddress = 0x74;

        /// <inheritdoc/>
        public override int GetLedAddress(int x, int y)
        {
            if (x <= 8)
            {
                x = 8 - x;
                y = 6 - y;
            }
            else
            {
                x = x - 8;
                y = y - 8;
            }

            return x * 16 + y;
        }
    }
}

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
    // Product: https://www.adafruit.com/product/4122
    public class Bonnet16x8 : Is31fl3731
    {
        /// <summary>
        /// Initialize LED driver.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public Bonnet16x8(I2cDevice i2cDevice)
        : base(i2cDevice, 16, 8)
        {
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public static new readonly int DefaultI2cAddress = 0x74;

        /// <inheritdoc/>
        public override int GetLedAddress(int x, int y)
        {
            if (x >= 8)
            {
                return (x - 6) * 16 - (y + 1);
            }

            return (x + 1) * 16 + (7 - y);
        }
    }
}

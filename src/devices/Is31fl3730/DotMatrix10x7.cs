// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an IS31FL3731 LED Matrix driver
    /// </summary>
    // Datasheet: https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf
    // Product: https://shop.pimoroni.com/products/led-dot-matrix-breakout
    // Related repo: https://github.com/pimoroni/microdot-phat
    public class DotMatrix10x7 : DotMatrix
    {
        /// <summary>
        /// Initialize Dot Matrix Breakout IS31FL3730 device.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public DotMatrix10x7(I2cDevice i2cDevice)
        : this(DotMatrix.InitializeI2c(i2cDevice))
        {
        }

        /// <summary>
        /// Initialize Dot Matrix Breakout IS31FL3730 device.
        /// </summary>
        /// <param name="is31fl3730">The <see cref="Iot.Device.Display.Is31fl3730"/> to create with.</param>
        public DotMatrix10x7(Is31fl3730 is31fl3730)
        : base(Initialize(is31fl3730))
        {
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public const int DefaultI2cAddress = 0x61;

        private static DotMatrix5x7[] Initialize(Is31fl3730 is31fl3730)
        {
            is31fl3730 = is31fl3730 ?? throw new ArgumentNullException(nameof(is31fl3730));
            return new DotMatrix5x7[]
            {
                is31fl3730[1],
                is31fl3730[0]
            };
        }
    }
}

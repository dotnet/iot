// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents a Pimoroni Micro Dot pHat.
    /// </summary>
    // Datasheet: https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf
    // Product: https://shop.pimoroni.com/products/microdot-phat
    // Product: https://shop.pimoroni.com/products/led-dot-matrix-breakout
    // Related repo: https://github.com/pimoroni/microdot-phat
    public class MicroDotPhat30x7 : DotMatrix
    {
        /// <summary>
        /// Initialize Micro Dot pHAT IS31FL3730 device.
        /// </summary>
        /// <param name="first">The first <see cref="System.Device.I2c.I2cDevice"/> Dot Matrix pair.</param>
        /// <param name="second">The first <see cref="System.Device.I2c.I2cDevice"/> Dot Matrix pair.</param>
        /// <param name="third">The first <see cref="System.Device.I2c.I2cDevice"/> Dot Matrix pair.</param>
        public MicroDotPhat30x7(I2cDevice first, I2cDevice second, I2cDevice third)
        : this(DotMatrix.InitializeI2c(first), DotMatrix.InitializeI2c(second), DotMatrix.InitializeI2c(third))
        {
        }

        /// <summary>
        /// Initialize Micro Dot pHAT IS31FL3730 device.
        /// </summary>
        /// <param name="first">The first <see cref="Iot.Device.Display.Is31fl3730"/> Dot Matrix pair.</param>
        /// <param name="second">The first <see cref="Iot.Device.Display.Is31fl3730"/> Dot Matrix pair.</param>
        /// <param name="third">The first <see cref="Iot.Device.Display.Is31fl3730"/> Dot Matrix pair.</param>
        public MicroDotPhat30x7(Is31fl3730 first, Is31fl3730 second, Is31fl3730 third)
        : base(Initialize(first, second, third))
        {
        }

        /// <summary>
        /// I2C addresses for Micro Dot pHat, left to right.
        /// </summary>
        public static int[] I2cAddresses = new int[] { 0x63, 0x62, 0x61 };

        private static DotMatrix5x7[] Initialize(Is31fl3730 first, Is31fl3730 second, Is31fl3730 third)
        {
            if (first is null || second is null || third is null)
            {
                throw new ArgumentException($"Input argument is null.");
            }

            return new DotMatrix5x7[]
            {
                first[1],
                first[0],
                second[1],
                second[0],
                third[1],
                third[0],
            };
        }
    }
}

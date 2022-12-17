// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an IS31FL3731 LED Matrix driver
    /// </summary>
    // Datasheet: https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf
    // Product: https://shop.pimoroni.com/products/led-dot-matrix-breakout
    // Related repo: https://github.com/pimoroni/microdot-phat
    public class DotMatrixBreakout10x7
    {
        private readonly DotMatrix5x7[] _matrices;
        private Is31fl3730 _is31fl3730;

        /// <summary>
        /// Initialize Dot Matrix Breakout IS31FL3730 device.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public DotMatrixBreakout10x7(I2cDevice i2cDevice)
        {
            i2cDevice = i2cDevice ?? throw new ArgumentException($"{nameof(i2cDevice)} is null.");
            _is31fl3730 = new(i2cDevice);
            _is31fl3730.DisplayMode = DisplayMode.MatrixOneAndTwo;
            _is31fl3730.Initialize();
            _matrices = new DotMatrix5x7[]
            {
                new DotMatrix5x7(_is31fl3730, 0),
                new DotMatrix5x7(_is31fl3730, 1)
            };
        }

        /// <summary>
        /// Initialize Dot Matrix Breakout IS31FL3730 device.
        /// </summary>
        /// <param name="is31fl3730">The <see cref="Iot.Device.Display.Is31fl3730"/> to create with.</param>
        public DotMatrixBreakout10x7(Is31fl3730 is31fl3730)
        {
            is31fl3730 = is31fl3730 ?? throw new ArgumentException($"{nameof(is31fl3730)} is null.");
            _is31fl3730 = is31fl3730;
            _matrices = new DotMatrix5x7[]
            {
                new DotMatrix5x7(_is31fl3730, 0),
                new DotMatrix5x7(_is31fl3730, 1)
            };
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public const int DefaultI2cAddress = 0x61;

        /// <summary>
        /// Width of LED matrix (x axis).
        /// </summary>
        public readonly int Width = 10;

        /// <summary>
        /// Height of LED matrix (y axis).
        /// </summary>
        public readonly int Height = 7;

        /// <summary>
        /// Indexer for matrices.
        /// </summary>
        public DotMatrix5x7 this[int matrix] => _matrices[matrix];

        /// <summary>
        /// Indexer for matrix pair.
        /// </summary>
        public int this[int x, int y]
        {
            get => x switch
                {
                    < 5 => this[0][x, y],
                    < 10 => this[1][x - 5, y],
                    _ => throw new ArgumentException($"{nameof(x)} value out of range")
                };
            set
            {
                if (x < 5)
                {
                    this[0][x, y] = value;
                }
                else if (x < 10)
                {
                    this[1][x - 5, y] = value;
                }
                else
                {
                    throw new ArgumentException($"{nameof(x)} value out of range");
                }
            }
        }

        /// <summary>
        /// Fill All LEDs.
        /// </summary>
        public void Fill(int value) => _is31fl3730.Fill(value);
    }
}

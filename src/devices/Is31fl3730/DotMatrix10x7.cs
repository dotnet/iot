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
    public class DotMatrix10x7
    {
        private readonly DotMatrix _matrix;

        /// <summary>
        /// Initialize Dot Matrix Breakout IS31FL3730 device.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public DotMatrix10x7(I2cDevice i2cDevice)
        : this(DotMatrix.Initialize(i2cDevice))
        {
        }

        /// <summary>
        /// Initialize Dot Matrix Breakout IS31FL3730 device.
        /// </summary>
        /// <param name="is31fl3730">The <see cref="Iot.Device.Display.Is31fl3730"/> to create with.</param>
        public DotMatrix10x7(Is31fl3730 is31fl3730)
        {
            is31fl3730 = is31fl3730 ?? throw new ArgumentException($"{nameof(is31fl3730)} is null.");
            var matrices = new DotMatrix5x7[]
            {
                is31fl3730[1],
                is31fl3730[0]
            };

            _matrix = new DotMatrix(matrices);
            Width = _matrix.Width;
            Height = _matrix.Height;
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public const int DefaultI2cAddress = 0x61;

        /// <summary>
        /// Width of LED matrix (x axis).
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of LED matrix (y axis).
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Indexer for matrices.
        /// </summary>
        public DotMatrix5x7 this[int matrix] => _matrix[matrix];

        /// <summary>
        /// Length (or count) of matrices.
        /// </summary>
        public int Length => _matrix.Length;

        /// <summary>
        /// Indexer for matrix pair.
        /// </summary>
        public int this[int x, int y]
        {
            get => _matrix[x, y];
            set => _matrix[x, y] = value;
        }

        /// <summary>
        /// Fill All LEDs.
        /// </summary>
        public void Fill(int value) => _matrix.Fill(value);
    }
}

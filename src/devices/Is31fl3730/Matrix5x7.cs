// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents LED Dot Matrix Breakout
    /// </summary>
    // Datasheet: https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf
    // Product: https://shop.pimoroni.com/products/led-dot-matrix-breakout
    // Related repo: https://github.com/pimoroni/microdot-phat
    public class Matrix5x7
    {
        private readonly Is31fl3730 _is31fl3730;

        /// <summary>
        /// Initialize IS31FL3730 device
        /// </summary>
        /// <param name="is31fl3730">The <see cref="Iot.Device.Display.Is31fl3730"/> to create with.</param>
        /// <param name="matrix">The index of the matrix (of a pair).</param>
        public Matrix5x7(Is31fl3730 is31fl3730, int matrix)
        {
            _is31fl3730 = is31fl3730;
            Width = _is31fl3730.Width;
            Height = _is31fl3730.Height;
            Matrix = matrix;
        }

        /// <summary>
        /// Indexer for matrix.
        /// </summary>
        public int this[int x, int y]
        {
            get => _is31fl3730.ReadLed(Matrix, x, y);
            set => _is31fl3730.WriteLed(Matrix, x, y, value);
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public static readonly int DefaultI2cAddress = 0x61;

        /// <summary>
        /// Width of LED matrix (x axis).
        /// </summary>
        public readonly int Width = 5;

        /// <summary>
        /// Height of LED matrix (y axis).
        /// </summary>
        public readonly int Height = 7;

        /// <summary>
        /// Identification of matrix (of 2).
        /// </summary>
        public readonly int Matrix = 0;

        /// <summary>
        /// Identification of matrix (of 2).
        /// </summary>
        public void Fill(int value)
        {
            _is31fl3730.Fill(Matrix, value);
        }

    }
}

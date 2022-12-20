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
    /// Represents a virtual LED Matrix.
    /// </summary>
    // Datasheet: https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf
    // Product: https://shop.pimoroni.com/products/led-dot-matrix-breakout
    // Related repo: https://github.com/pimoroni/microdot-phat
    public class DotMatrix
    {
        private readonly DotMatrix5x7[] _matrices;

        /// <summary>
        /// Initialize virtual Dot Matrix.
        /// </summary>
        /// <param name="matrices">The  to create with.</param>
        public DotMatrix(DotMatrix5x7[] matrices)
        {
            _matrices = matrices;
            Width = DotMatrix5x7.BaseWidth * matrices.Length;
            Height = DotMatrix5x7.BaseHeight;
        }

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
        public DotMatrix5x7 this[int matrix] => _matrices[matrix];

        /// <summary>
        /// Length (or count) of matrices.
        /// </summary>
        public int Length => _matrices.Length;

        /// <summary>
        /// Indexer for matrix pair.
        /// </summary>
        public int this[int x, int y]
        {
            get
            {
                var (matrixIndex, index) = GetMatrixIndex(x, y);
                return _matrices[matrixIndex][index, y];
            }
            set
            {
                var (matrixIndex, index) = GetMatrixIndex(x, y);
                _matrices[matrixIndex][index, y] = value;
            }
        }

        /// <summary>
        /// Fill All LEDs.
        /// </summary>
        public void Fill(int value)
        {
            foreach (DotMatrix5x7 matrix in _matrices)
            {
                matrix.Fill(value);
            }
        }

        private (int MatrixIndex, int Index) GetMatrixIndex(int x, int y)
        {
            if (x >= Width || x < 0)
            {
                throw new ArgumentException($"{nameof(x)} value ({x}) out of range.");
            }

            if (y >= Height || y < 0)
            {
                throw new ArgumentException($"{nameof(y)} value ({y}) out of range.");
            }

            int matrixIndex = x / DotMatrix5x7.BaseWidth;
            int index = x % DotMatrix5x7.BaseWidth;

            return (matrixIndex, index);
        }

        /// <summary>
        /// Default Is31fl3730 initialization
        /// </summary>
        public static Is31fl3730 Initialize(I2cDevice i2cDevice)
        {
            i2cDevice = i2cDevice ?? throw new ArgumentException($"{nameof(i2cDevice)} is null.");
            Is31fl3730 is31fl3730 = new(i2cDevice);
            is31fl3730.DisplayMode = DisplayMode.MatrixOneAndTwo;
            is31fl3730.Initialize();
            return is31fl3730;
        }
    }
}

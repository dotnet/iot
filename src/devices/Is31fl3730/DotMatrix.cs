// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;

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
        /// <param name="matrices">The matrices to use.</param>
        public DotMatrix(DotMatrix5x7[] matrices)
        {
            _matrices = matrices;
            Width = DotMatrix5x7.BaseWidth * matrices.Length;
        }

        /// <summary>
        /// Width (x-axis) of matrix.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height (y-axis) for matrix.
        /// </summary>
        public int Height { get; } = DotMatrix5x7.BaseHeight;

        /// <summary>
        /// Indexer for matrix.
        /// </summary>
        public DotMatrix5x7 this[int matrix] => _matrices[matrix];

        /// <summary>
        /// Length (or count) of matrices.
        /// </summary>
        public int Length => _matrices.Length;

        /// <summary>
        /// Indexer for matrix.
        /// </summary>
        public PinValue this[int x, int y]
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
        /// Fill LEDs with value.
        /// </summary>
        public void Fill(PinValue value)
        {
            foreach (DotMatrix5x7 matrix in _matrices)
            {
                matrix.Fill(value);
            }
        }

        private (int MatrixIndex, int Index) GetMatrixIndex(int x, int y)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentException($"{nameof(x)} value ({x}) out of range.");
            }

            if (y < 0 || y >= Height)
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
        public static Is31fl3730 InitializeI2c(I2cDevice i2cDevice)
        {
            i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            Is31fl3730 is31fl3730 = new(i2cDevice);
            is31fl3730.UpdateConfiguration(ShowdownMode.Normal, MatrixMode.Size8x8, DisplayMode.MatrixOneAndTwo);
            return is31fl3730;
        }
    }
}

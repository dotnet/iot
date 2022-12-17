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
    public class MicroDotPhat30x7
    {
        private DotMatrixBreakout10x7[] _pairs;
        private DotMatrix5x7[] _matrices = new DotMatrix5x7[6];

        /// <summary>
        /// Initialize Micro Dot pHAT IS31FL3730 device.
        /// </summary>
        public MicroDotPhat30x7()
        {
            _pairs = new DotMatrixBreakout10x7[3];

            foreach (var pair in Enumerable.Range(0, 3))
            {
                I2cDevice i2cDevice = I2cDevice.Create(new(1, MatrixValues.Addresses[pair]));
                _pairs[pair] = new DotMatrixBreakout10x7(i2cDevice);
                _matrices[pair * 2] = _pairs[pair][1];
                _matrices[pair * 2 + 1] = _pairs[pair][0];
            }
        }

        /// <summary>
        /// Initialize Micro Dot pHAT IS31FL3730 device.
        /// </summary>
        /// <param name="first">The first <see cref="Iot.Device.Display.Is31fl3730"/> Dot Matrix pair.</param>
        /// <param name="second">The first <see cref="Iot.Device.Display.Is31fl3730"/> Dot Matrix pair.</param>
        /// <param name="third">The first <see cref="Iot.Device.Display.Is31fl3730"/> Dot Matrix pair.</param>
        public MicroDotPhat30x7(Is31fl3730 first, Is31fl3730 second, Is31fl3730 third)
        {
            if (first is null || second is null || third is null)
            {
                throw new ArgumentException($"Input argument is null.");
            }

            _pairs = new DotMatrixBreakout10x7[]
            {
                new(first),
                new(second),
                new(third)
            };

            foreach (var pair in Enumerable.Range(0, 3))
            {
                _matrices[pair * 2] = _pairs[pair][1];
                _matrices[pair * 2 + 1] = _pairs[pair][0];
            }
        }

        /// <summary>
        /// Width of LED matrix (x axis).
        /// </summary>
        public readonly int Width = 30;

        /// <summary>
        /// Height of LED matrix (y axis).
        /// </summary>
        public readonly int Height = 7;

        /// <summary>
        /// Indexer for matrices.
        /// </summary>
        public DotMatrix5x7 this[int matrix] => _matrices[matrix];

        /// <summary>
        /// Length (or count) of matrices.
        /// </summary>
        public int Length => _matrices.Length;

        /// <summary>
        /// Indexer for Micro Dot pHat matrix.
        /// </summary>
        public int this[int x, int y]
        {
            get
            {
                if (x >= 30 || y > 6)
                {
                    throw new ArgumentException("Input value out of range.");
                }

                int matrix = x % 5;
                int modx = x - (matrix * 5);
                return this[matrix][modx, y];
            }
            set
            {
                if (x >= 30 || y > 6)
                {
                    throw new ArgumentException("Input value out of range.");
                }

                int matrix = x / 5;
                int modx = x - (matrix * 5);
                this[matrix][modx, y] = value;
            }
        }

        /// <summary>
        /// Fill All LEDs.
        /// </summary>
        public void Fill(int value)
        {
            foreach (var pair in _pairs)
            {
                pair.Fill(value);
            }
        }
    }
}

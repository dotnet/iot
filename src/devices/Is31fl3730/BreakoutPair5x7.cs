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
    // Product: https://shop.pimoroni.com/products/microdot-phat
    // Product: https://shop.pimoroni.com/products/led-dot-matrix-breakout
    // Related repo: https://github.com/pimoroni/microdot-phat
    public class BreakoutPair5x7 : IDisposable
    {
        private readonly Matrix3730 _matrixOne;
        private readonly Matrix3730 _matrixTwo;
        private readonly Matrix3730[] _pair = new Matrix3730[2];
        private Is31fl3730 _is31fl3730;
        private I2cDevice? _i2cDevice;

        /// <summary>
        /// Initialize IS31FL3730 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public BreakoutPair5x7(I2cDevice? i2cDevice = null)
        {
            _i2cDevice = i2cDevice is not null ? i2cDevice : I2cDevice.Create(new(1, Is31fl3730.DefaultI2cAddress));
            _is31fl3730 = new(_i2cDevice);
            _is31fl3730.DisplayMode = DisplayMode.MatrixOneAndTwo;
            _is31fl3730.Initialize();
            _matrixOne = new Matrix3730(_is31fl3730, 0);
            _matrixTwo = new Matrix3730(_is31fl3730, 1);
            _pair = new Matrix3730[] { _matrixOne, _matrixTwo };
        }

        /// <summary>
        /// Initialize IS31FL3730 device
        /// </summary>
        /// <param name="is31fl3730">The <see cref="Iot.Device.Display.Is31fl3730"/> to create with.</param>
        public BreakoutPair5x7(Is31fl3730 is31fl3730)
        {
            _is31fl3730 = is31fl3730;
            _matrixOne = new Matrix3730(_is31fl3730, 0);
            _matrixTwo = new Matrix3730(_is31fl3730, 1);
            _pair = new Matrix3730[] { _matrixOne, _matrixTwo };
        }

        /// <summary>
        /// Indexer for matrix pair.
        /// </summary>
        public Matrix3730 this[int matrix]
        {
            get => _pair[matrix];
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
        /// Identification of matrix (of 2).
        /// </summary>
        public readonly int Matrix = 0;

        /// <summary>
        /// Fill All LEDs.
        /// </summary>
        public void Fill(int value) => _is31fl3730.FillAll(value);

        /// <inheritdoc/>
        public void Dispose()
        {
            _is31fl3730?.Dispose();
            _is31fl3730 = null!;
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }
    }
}

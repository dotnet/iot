// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents LED Dot Matrix driven by IS31FL3730.
    /// </summary>
    public class DotMatrix5x7
    {
        private readonly Is31fl3730 _is31fl3730;
        private readonly int _matrix;

        /// <summary>
        /// Initialize Dot Matrix IS31FL3730 device.
        /// </summary>
        /// <param name="is31fl3730">The <see cref="Iot.Device.Display.Is31fl3730"/> to create with.</param>
        /// <param name="matrix">The index of the matrix (of a pair).</param>
        public DotMatrix5x7(Is31fl3730 is31fl3730, int matrix)
        {
            _is31fl3730 = is31fl3730;
            _matrix = matrix;
        }

        /// <summary>
        /// Indexer for matrix.
        /// </summary>
        public PinValue this[int x, int y]
        {
            get => _is31fl3730.Read(_matrix, x, y);
            set => _is31fl3730.Write(_matrix, x, y, value);
        }

        /// <summary>
        /// Fill LEDs with value.
        /// </summary>
        public void Fill(PinValue value) => _is31fl3730.Fill(_matrix, value);

        /// <summary>
        /// Fill matrix (0 is dark; 1 is lit).
        /// </summary>
        public void WriteDecimalPoint(PinValue value) => _is31fl3730.WriteDecimalPoint(_matrix, value);

        /// <summary>
        /// Width of LED matrix (x axis).
        /// </summary>
        public static readonly int BaseWidth = 5;

        /// <summary>
        /// Height of LED matrix (y axis).
        /// </summary>
        public static readonly int BaseHeight = 7;
    }
}

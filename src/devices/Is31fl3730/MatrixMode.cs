// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Multiplexing.Utility;

namespace Iot.Device.Display
{
    /// <summary>
    /// IS31FL3730 matrix modes, controlling the size of the matrix.
    /// </summary>
    public enum MatrixMode
    {
        /// <summary>
        /// Represents a 8x8 LED matrix.
        /// </summary>
        Size8x8 = 0,

        /// <summary>
        /// Represents a 7x9 LED matrix.
        /// </summary>
        Size7x9 = 1,

        /// <summary>
        /// Represents a 6x10 LED matrix.
        /// </summary>
        Size6x10 = 2,

        /// <summary>
        /// Represents an 5x11 LED matrix.
        /// </summary>
        Size5x11 = 3
    }
}

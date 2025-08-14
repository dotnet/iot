﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi Lite.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H3 (sun8iw7p1)
    /// </remarks>
    public class OrangePiLiteDriver : Sun8iw7p1Driver
    {
        private static readonly int[] _pinNumberConverter = new int[]
        {
            -1, -1, -1, MapPinNumber('A', 12), -1, MapPinNumber('A', 11), -1, MapPinNumber('A', 6), MapPinNumber('A', 13), -1,
            MapPinNumber('A', 14), MapPinNumber('A', 1), MapPinNumber('D', 14), MapPinNumber('A', 0), -1, MapPinNumber('A', 3),
            MapPinNumber('C', 4), -1, MapPinNumber('C', 7), MapPinNumber('C', 0), -1, MapPinNumber('C', 1), MapPinNumber('A', 2),
            MapPinNumber('C', 2), MapPinNumber('C', 3), -1, MapPinNumber('A', 21), MapPinNumber('A', 19), MapPinNumber('A', 18),
            MapPinNumber('A', 7), -1, MapPinNumber('A', 8), MapPinNumber('G', 8), MapPinNumber('A', 9), -1, MapPinNumber('A', 10),
            MapPinNumber('G', 9), MapPinNumber('A', 20), MapPinNumber('G', 6), -1, MapPinNumber('G', 7)
        };

        /// <inheritdoc/>
        protected override int PinCount => 28;

        /// <inheritdoc/>
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi Lite 2.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H6 (sun50iw6p1)
    /// </remarks>
    public class OrangePiLite2Driver : Sun50iw6p1Driver
    {
        private static readonly int[] _pinNumberConverter = new int[]
        {
            -1, -1, -1, MapPinNumber('H', 6), -1, MapPinNumber('H', 5), -1, MapPinNumber('H', 4), MapPinNumber('D', 21), -1,
            MapPinNumber('D', 22), MapPinNumber('D', 24), MapPinNumber('C', 9), MapPinNumber('D', 23), -1, MapPinNumber('D', 26),
            MapPinNumber('C', 8), -1, MapPinNumber('C', 7), MapPinNumber('C', 2), -1, MapPinNumber('C', 3), MapPinNumber('D', 25),
            MapPinNumber('C', 0), MapPinNumber('C', 5), -1, MapPinNumber('H', 3)
        };

        /// <inheritdoc/>
        protected override int PinCount => 17;

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            int num = _pinNumberConverter[pinNumber];

            return num != -1 ? num : throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber));
        }
    }
}
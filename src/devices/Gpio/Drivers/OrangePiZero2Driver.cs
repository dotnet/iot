// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi Zero 2.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H616 (sun50iw9p1)
    /// </remarks>
    public class OrangePiZero2Driver : Sun50iw9p1Driver
    {
        private static readonly int[] _pinNumberConverter = new int[]
        {
            -1, -1, -1, MapPinNumber('H', 5), -1, MapPinNumber('H', 4), -1, MapPinNumber('C', 9), MapPinNumber('H', 2), -1,
            MapPinNumber('H', 3), MapPinNumber('C', 6), MapPinNumber('C', 11), MapPinNumber('C', 5), -1, MapPinNumber('C', 8),
            MapPinNumber('C', 15), -1, MapPinNumber('C', 14), MapPinNumber('H', 7), -1, MapPinNumber('H', 8), MapPinNumber('C', 7),
            MapPinNumber('H', 6), MapPinNumber('H', 9), -1, MapPinNumber('C', 10)
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
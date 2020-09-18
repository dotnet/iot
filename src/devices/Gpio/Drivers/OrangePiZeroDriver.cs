// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi Zero.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H2+ (sun8iw7p1)
    /// </remarks>
    public class OrangePiZeroDriver : Sun8iw7p1Driver
    {
        private readonly int[] _pinNumberConverter = new int[27]
        {
            -1, -1, -1, MapPinNumber('A', 12), -1, MapPinNumber('A', 11), -1, MapPinNumber('A', 6), MapPinNumber('G', 6), -1,
            MapPinNumber('G', 7), MapPinNumber('A', 1), MapPinNumber('A', 7), MapPinNumber('A', 0), -1, MapPinNumber('A', 3),
            MapPinNumber('A', 19), -1, MapPinNumber('A', 18), MapPinNumber('A', 15), -1, MapPinNumber('A', 16), MapPinNumber('A', 2),
            MapPinNumber('A', 14), MapPinNumber('A', 13), -1, MapPinNumber('A', 10)
        };

        /// <summary>
        /// Orange Pi Zero has 17 GPIO pins.
        /// </summary>
        protected override int PinCount => 17;

        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            int num = _pinNumberConverter[pinNumber];

            return num != -1 ? num : throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber));
        }
    }
}

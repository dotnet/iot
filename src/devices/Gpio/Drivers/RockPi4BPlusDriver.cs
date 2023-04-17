// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Rock Pi 4B Plus.
    /// </summary>
    /// <remarks>
    /// SoC: Rockchip RK3399
    /// </remarks>
    public class RockPi4bPlusDriver : Rk3399Driver
    {
        private static readonly int[] _pinNumberConverter = new int[]
        {
            -1, -1, MapPinNumber(2, 'A', 7), -1, MapPinNumber(2, 'B', 0), -1, MapPinNumber(2, 'B', 3),
            MapPinNumber(4, 'C', 4), -1, MapPinNumber(4, 'C', 3), MapPinNumber(4, 'C', 2), MapPinNumber(4, 'A', 3),
            MapPinNumber(4, 'C', 6), -1, MapPinNumber(4, 'C', 5), MapPinNumber(4, 'D', 2), -1, MapPinNumber(4, 'D', 4),
            MapPinNumber(1, 'B', 0), -1, MapPinNumber(1, 'A', 7), MapPinNumber(4, 'D', 5), MapPinNumber(1, 'B', 1),
            MapPinNumber(1, 'B', 2), -1, -1, MapPinNumber(2, 'A', 0), MapPinNumber(2, 'A', 1), MapPinNumber(2, 'B', 2),
            -1, MapPinNumber(2, 'B', 1), MapPinNumber(3, 'C', 0), MapPinNumber(2, 'B', 4), -1, MapPinNumber(4, 'A', 5),
            MapPinNumber(4, 'A', 4), MapPinNumber(4, 'D', 6), MapPinNumber(4, 'A', 6), -1, MapPinNumber(4, 'A', 7)
        };

        /// <inheritdoc/>
        protected override int PinCount => 40;

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            int num = _pinNumberConverter[pinNumber];

            return num != -1 ? num : throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber));
        }
    }
}

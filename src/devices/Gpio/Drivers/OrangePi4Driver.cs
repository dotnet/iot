// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi 4/4B.
    /// </summary>
    /// <remarks>
    /// SoC: Rockchip RK3399
    /// </remarks>
    public class OrangePi4Driver : Rk3399Driver
    {
        private static readonly int[] _pinNumberConverter = new int[]
        {
            -1, -1, -1,  MapPinNumber(2, 'A', 0), -1, MapPinNumber(2, 'A', 1), -1, MapPinNumber(4, 'C', 6),
            MapPinNumber(4, 'C', 1), -1, MapPinNumber(4, 'C', 0), MapPinNumber(1, 'A', 1),
            MapPinNumber(1, 'C', 2), MapPinNumber(1, 'A', 3), -1, MapPinNumber(2, 'D', 4),
            MapPinNumber(1, 'C', 6), -1, MapPinNumber(1, 'C', 7), MapPinNumber(1, 'B', 0), -1, MapPinNumber(1, 'A', 7),
            MapPinNumber(1, 'D', 0), MapPinNumber(1, 'B', 1), MapPinNumber(1, 'B', 2), -1, MapPinNumber(4, 'C', 5),
            MapPinNumber(2, 'A', 0), MapPinNumber(2, 'A', 1), MapPinNumber(3, 'D', 1), -1, MapPinNumber(3, 'D', 2),
            MapPinNumber(4, 'A', 0), MapPinNumber(3, 'D', 0), -1, MapPinNumber(3, 'D', 3), MapPinNumber(3, 'D', 7),
            MapPinNumber(3, 'D', 4), MapPinNumber(3, 'D', 5), -1, MapPinNumber(3, 'D', 6)
        };

        /// <inheritdoc/>
        protected override int PinCount => 28;

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            int num = _pinNumberConverter[pinNumber];

            return num != -1 ? num : throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber));
        }
    }
}

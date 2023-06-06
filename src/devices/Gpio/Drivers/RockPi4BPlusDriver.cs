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
        /// <inheritdoc/>
        protected override int PinCount => 40;

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber switch
            {
                3 => MapPinNumber(2, 'A', 7),
                5 => MapPinNumber(2, 'B', 0),
                7 => MapPinNumber(2, 'B', 3),
                8 => MapPinNumber(4, 'C', 4),
                10 => MapPinNumber(4, 'C', 3),
                11 => MapPinNumber(4, 'C', 2),
                12 => MapPinNumber(4, 'A', 3),
                13 => MapPinNumber(4, 'C', 6),
                15 => MapPinNumber(4, 'C', 5),
                16 => MapPinNumber(4, 'D', 2),
                18 => MapPinNumber(4, 'D', 4),
                19 => MapPinNumber(1, 'B', 0),
                21 => MapPinNumber(1, 'A', 7),
                22 => MapPinNumber(4, 'D', 5),
                23 => MapPinNumber(1, 'B', 1),
                24 => MapPinNumber(1, 'B', 2),
                27 => MapPinNumber(2, 'A', 0),
                28 => MapPinNumber(2, 'A', 1),
                29 => MapPinNumber(2, 'B', 2),
                31 => MapPinNumber(2, 'B', 1),
                32 => MapPinNumber(3, 'C', 0),
                33 => MapPinNumber(2, 'B', 4),
                35 => MapPinNumber(4, 'A', 5),
                36 => MapPinNumber(4, 'A', 4),
                37 => MapPinNumber(4, 'D', 6),
                38 => MapPinNumber(4, 'A', 6),
                40 => MapPinNumber(4, 'A', 7),
                _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
            };
        }
    }
}

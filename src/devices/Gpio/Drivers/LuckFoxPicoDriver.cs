// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the LuckFox Pico
    /// </summary>
    /// <remarks>
    /// SoC: Rockchip RV1103
    /// </remarks>
    public class LuckFoxPicoDriver : Rv1103Driver
    {
        /// <inheritdoc/>
        protected override int PinCount => 24;

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber switch
            {
                1 => MapPinNumber(1, 'B', 2),
                2 => MapPinNumber(1, 'B', 3),
                4 => MapPinNumber(1, 'C', 7),
                5 => MapPinNumber(1, 'C', 6),
                6 => MapPinNumber(1, 'C', 5),
                7 => MapPinNumber(1, 'C', 4),
                9 => MapPinNumber(1, 'D', 2),
                10 => MapPinNumber(1, 'D', 3),
                11 => MapPinNumber(1, 'A', 2),
                12 => MapPinNumber(1, 'C', 0),
                14 => MapPinNumber(1, 'C', 1),
                15 => MapPinNumber(1, 'C', 2),
                16 => MapPinNumber(1, 'C', 3),
                17 => MapPinNumber(0, 'A', 4),
                19 => MapPinNumber(1, 'D', 0),
                20 => MapPinNumber(1, 'D', 1),
                21 => MapPinNumber(4, 'B', 1),
                22 => MapPinNumber(4, 'B', 0),
                24 => MapPinNumber(4, 'A', 6),
                25 => MapPinNumber(4, 'A', 2),
                26 => MapPinNumber(4, 'A', 3),
                27 => MapPinNumber(4, 'A', 4),
                31 => MapPinNumber(4, 'C', 0),
                32 => MapPinNumber(4, 'C', 1),
                _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
            };
        }
    }
}

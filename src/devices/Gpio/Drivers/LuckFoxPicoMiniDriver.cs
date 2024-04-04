// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the LuckFox Pico Mini
    /// </summary>
    /// <remarks>
    /// SoC: Rockchip RV1103
    /// </remarks>
    public class LuckFoxPicoMiniDriver : Rv1103Driver
    {
        /// <inheritdoc/>
        protected override int PinCount => 17;

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber switch
            {
                4 => MapPinNumber(1, 'B', 2),
                5 => MapPinNumber(1, 'B', 3),
                6 => MapPinNumber(1, 'C', 0),
                7 => MapPinNumber(1, 'C', 1),
                8 => MapPinNumber(1, 'C', 2),
                9 => MapPinNumber(1, 'C', 3),
                10 => MapPinNumber(1, 'C', 4),
                11 => MapPinNumber(1, 'C', 5),
                12 => MapPinNumber(1, 'D', 0),
                13 => MapPinNumber(1, 'D', 1),
                14 => MapPinNumber(1, 'D', 2),
                15 => MapPinNumber(1, 'D', 3),
                16 => MapPinNumber(1, 'C', 6),
                17 => MapPinNumber(1, 'C', 7),
                18 => MapPinNumber(0, 'A', 4),
                19 => MapPinNumber(4, 'C', 0),
                20 => MapPinNumber(4, 'C', 1),
                _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
            };
        }
    }
}

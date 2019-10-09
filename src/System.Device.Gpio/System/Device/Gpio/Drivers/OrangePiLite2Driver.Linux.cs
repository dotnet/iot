// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi Lite 2.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H6 (sun50iw6p1)
    /// </remarks>
    public class OrangePiLite2Driver : SunxiDriver
    {
        protected override int GpioRegisterOffset0 => 0x0300B000;
        protected override int GpioRegisterOffset1 => 0x07022000;

        /// <summary>
        /// Orange Pi Lite 2 has 17 GPIO pins.
        /// </summary>
        protected internal override int PinCount => 17;

        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber switch
            {
                3 => MapPinNumber('H', 6),
                5 => MapPinNumber('H', 5),
                7 => MapPinNumber('H', 4),
                8 => MapPinNumber('D', 21),
                10 => MapPinNumber('D', 22),
                11 => MapPinNumber('D', 24),
                12 => MapPinNumber('C', 9),
                13 => MapPinNumber('D', 23),
                15 => MapPinNumber('D', 26),
                16 => MapPinNumber('C', 8),
                18 => MapPinNumber('C', 7),
                19 => MapPinNumber('C', 2),
                21 => MapPinNumber('C', 3),
                22 => MapPinNumber('D', 25),
                23 => MapPinNumber('C', 0),
                24 => MapPinNumber('C', 5),
                26 => MapPinNumber('H', 3),
                _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
            };
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi Zero.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H2+ (sun8iw7p1)
    /// </remarks>
    public class OrangePiZeroDriver : SunxiDriver
    {
        protected override int GpioRegisterOffset0 => 0x01C20800;
        protected override int GpioRegisterOffset1 => 0x01F02C00;

        /// <summary>
        /// Orange Pi Zero has 17 GPIO pins.
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
                3 => MapPinNumber('A', 12),
                5 => MapPinNumber('A', 11),
                7 => MapPinNumber('A', 6),
                8 => MapPinNumber('G', 6),
                10 => MapPinNumber('G', 7),
                11 => MapPinNumber('A', 1),
                12 => MapPinNumber('A', 7),
                13 => MapPinNumber('A', 0),
                15 => MapPinNumber('A', 3),
                16 => MapPinNumber('A', 19),
                18 => MapPinNumber('A', 18),
                19 => MapPinNumber('A', 15),
                21 => MapPinNumber('A', 16),
                22 => MapPinNumber('A', 2),
                23 => MapPinNumber('A', 14),
                24 => MapPinNumber('A', 13),
                26 => MapPinNumber('A', 10),
                _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
            };
        }
    }
}

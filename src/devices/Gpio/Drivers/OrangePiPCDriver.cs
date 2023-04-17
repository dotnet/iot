// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Gpio.Drivers;

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi PC and PC+.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H3
    /// </remarks>
    public class OrangePiPCDriver : SunxiDriver
    {
        /// <inheritdoc/>
        protected override int CpuxPortBaseAddress => 0x01C20800;

        /// <inheritdoc/>
        protected override int CpusPortBaseAddress => 0x01F02C00;

        /// <summary>
        /// Orange Pi PC has 28 GPIO pins.
        /// </summary>
        protected override int PinCount => 28;

        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber switch
            {
                3 => MapPinNumber('A', 12),
                5 => MapPinNumber('A', 11),
                7 => MapPinNumber('A', 6),
                8 => MapPinNumber('A', 13),
                10 => MapPinNumber('A', 14),
                11 => MapPinNumber('A', 1),
                12 => MapPinNumber('C', 14),
                13 => MapPinNumber('A', 0),
                15 => MapPinNumber('A', 3),
                16 => MapPinNumber('C', 4),
                18 => MapPinNumber('C', 7),
                19 => MapPinNumber('C', 0),
                21 => MapPinNumber('C', 1),
                22 => MapPinNumber('A', 2),
                23 => MapPinNumber('C', 2),
                24 => MapPinNumber('C', 3),
                26 => MapPinNumber('A', 21),
                27 => MapPinNumber('A', 19),
                28 => MapPinNumber('A', 18),
                29 => MapPinNumber('A', 7),
                31 => MapPinNumber('A', 8),
                32 => MapPinNumber('E', 8),
                33 => MapPinNumber('A', 9),
                35 => MapPinNumber('A', 10),
                36 => MapPinNumber('E', 9),
                37 => MapPinNumber('A', 20),
                38 => MapPinNumber('E', 6),
                39 => MapPinNumber('E', 7),
                _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
            };
        }
    }
}

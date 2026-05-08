// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using Iot.Device.Gpio;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi 5B (26-pin header).
    /// </summary>
    /// <remarks>
    /// SoC: Rockchip RK3588S
    /// Pin mapping sourced from wiringOP (orangepi-xunlong/wiringOP).
    /// </remarks>
    public class OrangePi5BDriver : Rk3588Driver
    {
        // Mapping from physical header pin (index) to GPIO logical number.
        // -1 indicates power, ground, or non-GPIO pins.
        private static readonly int[] _physicalToGpio = new int[]
        {
            -1,       // 0 (no pin)
            -1, -1,   // 1 = 3.3V,   2 = 5V
            MapPinNumber(1, 'B', 7), -1,   // 3 = GPIO1_B7 (47),  4 = 5V
            MapPinNumber(1, 'B', 6), -1,   // 5 = GPIO1_B6 (46),  6 = GND
            MapPinNumber(1, 'C', 6), MapPinNumber(4, 'A', 3),   // 7 = GPIO1_C6 (54),  8 = GPIO4_A3 (131)
            -1, MapPinNumber(4, 'A', 4),   // 9 = GND,           10 = GPIO4_A4 (132)
            MapPinNumber(4, 'B', 2), MapPinNumber(0, 'D', 5),   // 11 = GPIO4_B2 (138), 12 = GPIO0_D5 (29)
            MapPinNumber(4, 'B', 3), -1,   // 13 = GPIO4_B3 (139), 14 = GND
            MapPinNumber(0, 'D', 4), MapPinNumber(1, 'D', 3),   // 15 = GPIO0_D4 (28), 16 = GPIO1_D3 (59)
            -1, MapPinNumber(1, 'D', 2),   // 17 = 3.3V,         18 = GPIO1_D2 (58)
            MapPinNumber(1, 'C', 1), -1,   // 19 = GPIO1_C1 (49), 20 = GND
            MapPinNumber(1, 'C', 0), -1,   // 21 = GPIO1_C0 (48), 22 = NC
            MapPinNumber(1, 'C', 2), MapPinNumber(1, 'C', 4),   // 23 = GPIO1_C2 (50), 24 = GPIO1_C4 (52)
            -1, MapPinNumber(1, 'A', 3),   // 25 = GND,          26 = GPIO1_A3 (35)
        };

        /// <inheritdoc/>
        protected override int PinCount => 26;

        /// <summary>
        /// Creates a <see cref="VirtualGpioController"/> that maps physical header pin numbers to logical GPIO numbers.
        /// </summary>
        /// <param name="controller">A <see cref="GpioController"/> wrapping this driver.</param>
        /// <returns>A <see cref="VirtualGpioController"/> using physical pin numbering.</returns>
        public static VirtualGpioController CreatePhysicalPinMapping(GpioController controller)
        {
            return new MappingGpioController(controller, ConvertPinNumberFromPhysicalToLogical);
        }

        private static int ConvertPinNumberFromPhysicalToLogical(int pinNumber)
        {
            if (pinNumber <= 0 || pinNumber >= _physicalToGpio.Length)
            {
                return -1;
            }

            return _physicalToGpio[pinNumber];
        }
    }
}

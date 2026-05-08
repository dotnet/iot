// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using Iot.Device.Gpio;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi 5 Plus (40-pin header).
    /// </summary>
    /// <remarks>
    /// SoC: Rockchip RK3588
    /// Pin mapping sourced from wiringOP (orangepi-xunlong/wiringOP).
    /// </remarks>
    public class OrangePi5PlusDriver : Rk3588Driver
    {
        // Mapping from physical header pin (index) to GPIO logical number.
        // -1 indicates power, ground, or non-GPIO pins.
        private static readonly int[] _physicalToGpio = new int[]
        {
            -1,       // 0 (no pin)
            -1, -1,   // 1 = 3.3V,   2 = 5V
            MapPinNumber(0, 'C', 0), -1,   // 3 = GPIO0_C0 (16),  4 = 5V
            MapPinNumber(0, 'B', 7), -1,   // 5 = GPIO0_B7 (15),  6 = GND
            MapPinNumber(1, 'D', 6), MapPinNumber(1, 'A', 1),   // 7 = GPIO1_D6 (62),  8 = GPIO1_A1 (33)
            -1, MapPinNumber(1, 'A', 0),   // 9 = GND,           10 = GPIO1_A0 (32)
            MapPinNumber(1, 'A', 4), MapPinNumber(3, 'A', 1),   // 11 = GPIO1_A4 (36), 12 = GPIO3_A1 (97)
            MapPinNumber(1, 'A', 7), -1,   // 13 = GPIO1_A7 (39), 14 = GND
            MapPinNumber(1, 'B', 0), MapPinNumber(3, 'B', 5),   // 15 = GPIO1_B0 (40), 16 = GPIO3_B5 (109)
            -1, MapPinNumber(3, 'B', 6),   // 17 = 3.3V,         18 = GPIO3_B6 (110)
            MapPinNumber(1, 'B', 2), -1,   // 19 = GPIO1_B2 (42), 20 = GND
            MapPinNumber(1, 'B', 1), MapPinNumber(1, 'A', 2),   // 21 = GPIO1_B1 (41), 22 = GPIO1_A2 (34)
            MapPinNumber(1, 'B', 3), MapPinNumber(1, 'B', 4),   // 23 = GPIO1_B3 (43), 24 = GPIO1_B4 (44)
            -1, MapPinNumber(1, 'B', 5),   // 25 = GND,          26 = GPIO1_B5 (45)
            MapPinNumber(1, 'B', 7), MapPinNumber(1, 'B', 6),   // 27 = GPIO1_B7 (47), 28 = GPIO1_B6 (46)
            MapPinNumber(1, 'D', 7), -1,   // 29 = GPIO1_D7 (63), 30 = GND
            MapPinNumber(3, 'A', 0), MapPinNumber(1, 'A', 3),   // 31 = GPIO3_A0 (96), 32 = GPIO1_A3 (35)
            MapPinNumber(3, 'C', 2), -1,   // 33 = GPIO3_C2 (114), 34 = GND
            MapPinNumber(3, 'A', 2), MapPinNumber(3, 'A', 5),   // 35 = GPIO3_A2 (98), 36 = GPIO3_A5 (101)
            MapPinNumber(3, 'C', 1), MapPinNumber(3, 'A', 4),   // 37 = GPIO3_C1 (113), 38 = GPIO3_A4 (100)
            -1, MapPinNumber(3, 'A', 3),   // 39 = GND,          40 = GPIO3_A3 (99)
        };

        /// <inheritdoc/>
        protected override int PinCount => 40;

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

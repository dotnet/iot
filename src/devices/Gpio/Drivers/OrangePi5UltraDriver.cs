// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using Iot.Device.Gpio;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi 5 Ultra (40-pin header).
    /// </summary>
    /// <remarks>
    /// SoC: Rockchip RK3588S
    /// Pin mapping sourced from wiringOP (orangepi-xunlong/wiringOP).
    /// </remarks>
    public class OrangePi5UltraDriver : Rk3588Driver
    {
        // Mapping from physical header pin (index) to GPIO logical number.
        // -1 indicates power, ground, or non-GPIO pins.
        private static readonly int[] _physicalToGpio = new int[]
        {
            -1,       // 0 (no pin)
            -1, -1,   // 1 = 3.3V,   2 = 5V
            MapPinNumber(0, 'C', 0), -1,   // 3 = GPIO0_C0 (16),  4 = 5V
            MapPinNumber(0, 'B', 7), -1,   // 5 = GPIO0_B7 (15),  6 = GND
            MapPinNumber(1, 'A', 7), MapPinNumber(0, 'B', 5),   // 7 = GPIO1_A7 (39),  8 = GPIO0_B5 (13)
            -1, MapPinNumber(0, 'B', 6),   // 9 = GND,           10 = GPIO0_B6 (14)
            MapPinNumber(1, 'A', 0), MapPinNumber(4, 'A', 6),   // 11 = GPIO1_A0 (32), 12 = GPIO4_A6 (134)
            MapPinNumber(1, 'A', 1), -1,   // 13 = GPIO1_A1 (33), 14 = GND
            MapPinNumber(1, 'A', 2), MapPinNumber(1, 'A', 3),   // 15 = GPIO1_A2 (34), 16 = GPIO1_A3 (35)
            -1, MapPinNumber(1, 'A', 4),   // 17 = 3.3V,         18 = GPIO1_A4 (36)
            MapPinNumber(1, 'B', 2), -1,   // 19 = GPIO1_B2 (42), 20 = GND
            MapPinNumber(1, 'B', 1), MapPinNumber(1, 'B', 0),   // 21 = GPIO1_B1 (41), 22 = GPIO1_B0 (40)
            MapPinNumber(1, 'B', 3), MapPinNumber(1, 'B', 4),   // 23 = GPIO1_B3 (43), 24 = GPIO1_B4 (44)
            -1, MapPinNumber(1, 'B', 5),   // 25 = GND,          26 = GPIO1_B5 (45)
            MapPinNumber(4, 'C', 1), MapPinNumber(4, 'C', 0),   // 27 = GPIO4_C1 (145), 28 = GPIO4_C0 (144)
            MapPinNumber(3, 'C', 1), -1,   // 29 = GPIO3_C1 (113), 30 = GND
            MapPinNumber(3, 'B', 5), MapPinNumber(4, 'B', 3),   // 31 = GPIO3_B5 (109), 32 = GPIO4_B3 (139)
            MapPinNumber(3, 'B', 6), -1,   // 33 = GPIO3_B6 (110), 34 = GND
            MapPinNumber(3, 'C', 2), MapPinNumber(4, 'B', 7),   // 35 = GPIO3_C2 (114), 36 = GPIO4_B7 (143)
            MapPinNumber(4, 'A', 7), MapPinNumber(3, 'C', 0),   // 37 = GPIO4_A7 (135), 38 = GPIO3_C0 (112)
            -1, MapPinNumber(3, 'B', 7),   // 39 = GND,          40 = GPIO3_B7 (111)
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

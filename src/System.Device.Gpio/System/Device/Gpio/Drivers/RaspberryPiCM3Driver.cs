// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Raspberry Pi 3.
    /// </summary>
    public class RaspberryPiCM3Driver : RaspberryPi3Driver // Different base classes declared in RaspberryPi3Driver.Linux.cs and RaspberryPi3Driver.Windows.cs
    {
        /// <summary>
        /// Raspberry CM3 has 48 GPIO pins.
        /// </summary>
        protected internal override int PinCount => 48;

        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            // CM3 has no logical numbering scheme
            return pinNumber;
        }
    }
}

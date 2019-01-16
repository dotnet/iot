// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the HummingBoard.
    /// </summary>
    public partial class HummingBoardDriver  // Different base classes declared in HummingboardDriver.Linux.cs and HummingboardDriver.Windows.cs
    {
        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            switch (pinNumber)
            {
                case 7:
                    return 1;
                case 11:
                    return 73;
                case 12:
                    return 72;
                case 13:
                    return 71;
                case 15:
                    return 10;
                case 16:
                    return 194;
                case 18:
                    return 195;
                case 22:
                    return 67;
            }

            throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber));
        }

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        protected internal override PinMode GetPinMode(int pinNumber)
        {
            throw new NotImplementedException();
        }
    }
}

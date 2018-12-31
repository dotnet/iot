// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio.Drivers
{
    public partial class HummingBoardGpioDriver  // Different base classes declared in HummingboardGpioDriver.Linux.cs and HummingboardGpioDriver.Windows.cs
    {
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            switch (pinNumber)
            {
                case 7: return 1;
                case 11: return 73;
                case 12: return 72;
                case 13: return 71;
                case 15: return 10;
                case 16: return 194;
                case 18: return 195;
                case 22: return 67;
            }

            throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {this.GetType().Name} device", nameof(pinNumber));
        }

        protected internal override PinMode GetPinMode(int pinNumber)
        {
            throw new NotImplementedException();
        }
    }
}

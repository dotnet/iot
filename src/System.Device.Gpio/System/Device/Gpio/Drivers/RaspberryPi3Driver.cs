// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio.Drivers
{
    public partial class RaspberryPi3Driver  // Different base classes declared in RaspberryPi3Driver.Linux.cs and RaspberryPi3Driver.Windows.cs
    {
        /// <summary>
        /// Raspberry Pi 3 has 24 Gpio Pins.
        /// </summary>
        protected internal override int PinCount => 24;

        private void ValidatePinNumber(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber > 27)
            {
                throw new ArgumentException("The specified pin number is invalid.", nameof(pinNumber));
            }
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            switch (pinNumber)
            {
                case 3: return 2;
                case 5: return 3;
                case 7: return 4;
                case 11: return 17;
                case 12: return 18;
                case 13: return 27;
                case 15: return 22;
                case 16: return 23;
                case 18: return 24;
                case 19: return 10;
                case 21: return 9;
                case 22: return 25;
                case 23: return 11;
                case 24: return 8;
                case 26: return 7;
                case 29: return 5;
                case 31: return 6;
                case 32: return 12;
                case 33: return 13;
                case 35: return 19;
                case 36: return 16;
                case 37: return 26;
                case 38: return 20;
                case 40: return 21;
            }

            throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {this.GetType().Name} device", nameof(pinNumber));
        }
    }
}

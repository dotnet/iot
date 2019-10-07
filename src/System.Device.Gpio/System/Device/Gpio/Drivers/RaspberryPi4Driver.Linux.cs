using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device.Gpio.Drivers
{
    public unsafe partial class RaspberryPi4Driver
    {
        /// <summary>
        /// Sets the resistor pull up/down mode for an input pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode of a pin to set the resistor pull up/down mode.</param>
        protected override void SetInputPullMode(int pinNumber, PinMode mode)
        {
            // Doesn't hurt to use the old (Pi3) code as well
            base.SetInputPullMode(pinNumber, mode);

            int shift = (pinNumber & 0xf) << 1;
            UInt32 pull = 0;
            UInt32 bits = 0;
            switch (mode)
            {
                case PinMode.Input: pull = 0; break;
                case PinMode.InputPullUp: pull = 1; break;
                case PinMode.InputPullDown: pull = 2; break;
            }

            var gpioReg = RegisterViewPointer;
            bits = (gpioReg->GPPUPPDN[(pinNumber >> 4)]);
            bits &= ~(3u << shift);
            bits |= (pull << shift);
            gpioReg->GPPUPPDN[(pinNumber >> 4)] = bits;
        }
    }
}

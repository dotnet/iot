// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A virtual GpioPin to allow creating a new GpioPin and a new pin number
    /// </summary>
    internal class VirtualGpioPin : GpioPin
    {
        protected internal VirtualGpioPin(GpioPin gpioPin, int pinNumber)
            : base(gpioPin, pinNumber)
        {
        }
    }
}

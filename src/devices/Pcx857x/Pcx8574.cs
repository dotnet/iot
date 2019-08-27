// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Pcx857x
{
    /// <summary>
    /// Base class for 8 bit I/O expanders.
    /// </summary>
    public abstract class Pcx8574 : Pcx857x
    {
        public Pcx8574(I2cDevice device, int interrupt = -1, GpioController gpioController = null)
            : base(device, interrupt, gpioController)
        {
        }

        protected override int PinCount => 8;
    }
}

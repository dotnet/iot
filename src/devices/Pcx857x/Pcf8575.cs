// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using System.Device.I2c.Devices;

namespace Iot.Device.Pcx857x
{
    /// <summary>
    /// Remote 16-bit I/O expander for I2C-bus with interrupt.
    /// </summary>
    public class Pcf8575 : Pcx8575
    {
        /// <summary>
        /// /// Initializes a new instance of the Pcf8575 device.
        /// </summary>
        /// <param name="device">The I2C device.</param>
        /// <param name="interrupt">The interrupt pin number, if present.</param>
        /// <param name="gpioController">
        /// The GPIO controller for the <paramref name="interrupt"/>.
        /// If not specified, the default controller will be used.
        /// </param>
        public Pcf8575(I2cDevice device, int interrupt = -1, IGpioController gpioController = null)
            : base(device, interrupt, gpioController)
        {
        }
    }
}

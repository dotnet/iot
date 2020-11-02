// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Pcx857x
{
    /// <summary>
    /// Base class for 8 bit I/O expanders.
    /// </summary>
    public abstract class Pcx8574 : Pcx857x
    {
        /// <summary>
        /// Constructs Pcx8574 instance
        /// </summary>
        /// <param name="device">I2C device</param>
        /// <param name="interrupt">Interrupt pin</param>
        /// <param name="gpioController"><see cref="GpioController"/> related with <paramref name="interrupt"/> pin</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Pcx8574(I2cDevice device, int interrupt = -1, GpioController gpioController = null, bool shouldDispose = true)
            : base(device, interrupt, gpioController, shouldDispose)
        {
        }

        /// <inheritdoc/>
        protected override int PinCount => 8;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Pcx857x
{
    /// <summary>
    /// Remote 16-bit I/O expander for I2C-bus with interrupt.
    /// </summary>
    /// <remarks>
    /// Fast mode I2C variant of the Pcf8575.
    /// </remarks>
    public class Pca8575 : Pcx8575
    {
        /// <summary>
        /// Initializes a new instance of the Pca8575 device.
        /// </summary>
        /// <param name="device">The I2C device.</param>
        /// <param name="interrupt">The interrupt pin number, if present.</param>
        /// <param name="gpioController">
        /// The GPIO controller for the <paramref name="interrupt"/>.
        /// If not specified, the default controller will be used.
        /// </param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Pca8575(I2cDevice device, int interrupt = -1, GpioController gpioController = null, bool shouldDispose = true)
            : base(device, interrupt, gpioController, shouldDispose)
        {
        }
    }
}

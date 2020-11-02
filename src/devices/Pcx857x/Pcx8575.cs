// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Pcx857x
{
    /// <summary>
    /// Base class for 16 bit I/O expanders.
    /// </summary>
    public abstract class Pcx8575 : Pcx857x
    {
        /// <summary>
        /// Constructs Pcx8575 instance
        /// </summary>
        /// <param name="device">I2C device</param>
        /// <param name="interrupt">Interrupt pin</param>
        /// <param name="gpioController"><see cref="GpioController"/> related with <paramref name="interrupt"/> pin</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Pcx8575(I2cDevice device, int interrupt = -1, GpioController gpioController = null, bool shouldDispose = true)
            : base(device, interrupt, gpioController, shouldDispose)
        {
        }

        /// <inheritdoc/>
        protected override int PinCount => 16;

        /// <summary>
        /// Writes 16-bit unsigned integer to the device
        /// </summary>
        /// <param name="value">16-bit unsigned value to be written</param>
        public void WriteUInt16(ushort value) => InternalWriteUInt16(value);

        /// <summary>
        /// Reads 16-bit unsigned integer from the device
        /// </summary>
        /// <returns></returns>
        public ushort ReadUInt16() => InternalReadUInt16();
    }
}

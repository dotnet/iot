// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// Wraps 8-bit MCP I/O expanders.
    /// </summary>
    public abstract class Mcp23x0x : Mcp23xxx
    {
        /// <summary>
        /// Constructs Mcp23x0x instance
        /// </summary>
        /// <param name="device">I2C device used to communicate with the device</param>
        /// <param name="reset">Reset pin</param>
        /// <param name="interrupt">Interrupt pin</param>
        /// <param name="controller">
        /// <see cref="GpioController"/> related with
        /// <paramref name="reset"/> and <paramref name="interrupt"/> pins
        /// </param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        protected Mcp23x0x(BusAdapter device, int reset, int interrupt, GpioController? controller = null, bool shouldDispose = true)
            : base(device, reset, interrupt, controller: controller, shouldDispose: shouldDispose)
        {
        }

        /// <inheritdoc/>
        protected override int PinCount => 8;
    }
}

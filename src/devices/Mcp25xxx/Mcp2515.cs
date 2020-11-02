// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// Driver for the Microchip MCP2515 CAN controller.
    /// </summary>
    public class Mcp2515 : Mcp25xxx
    {
        /// <summary>
        /// Initializes a new instance of the Mcp2515 class.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to Reset.</param>
        /// <param name="tx0rts">The output pin number that is connected to Tx0RTS.</param>
        /// <param name="tx1rts">The output pin number that is connected to Tx1RTS.</param>
        /// <param name="tx2rts">The output pin number that is connected to Tx2RTS.</param>
        /// <param name="interrupt">The input pin number that is connected to INT.</param>
        /// <param name="rx0bf">The input pin number that is connected to Rx0BF.</param>
        /// <param name="rx1bf">The input pin number that is connected to Rx1BF.</param>
        /// <param name="clkout">The input pin number that is connected to CLKOUT.</param>
        /// <param name="gpioController">
        /// The GPIO controller for defined external pins. If not specified, the default controller will be used.
        /// </param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Mcp2515(
            SpiDevice spiDevice,
            int reset = -1,
            int tx0rts = -1,
            int tx1rts = -1,
            int tx2rts = -1,
            int interrupt = -1,
            int rx0bf = -1,
            int rx1bf = -1,
            int clkout = -1,
            GpioController gpioController = null,
            bool shouldDispose = true)
            : base(
                  spiDevice,
                  reset,
                  tx0rts,
                  tx1rts,
                  tx2rts,
                  interrupt,
                  rx0bf,
                  rx1bf,
                  clkout,
                  gpioController,
                  shouldDispose)
        {
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Mcp23xxx
{
    public class Mcp23S09 : Mcp23x0x
    {
        /// <summary>
        /// Initializes new instance of Mcp23S09 device.
        /// A general purpose parallel I/O expansion for SPI applications.
        /// </summary>
        /// <param name="deviceAddress">The device address for the connection on the SPI bus.</param>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interrupt">The input pin number that is connected to the interrupt.</param>
        public Mcp23S09(int deviceAddress, SpiDevice spiDevice, int reset = -1, int interrupt = -1)
            : base(new SpiAdapter(spiDevice, deviceAddress), deviceAddress, reset, interrupt)
        {
        }
    }
}

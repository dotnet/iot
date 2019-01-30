// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Mcp23xxx
{
    public class Mcp23S17 : Mcp23Sxx
    {
        /// <summary>
        /// Initializes new instance of Mcp23xxx device.
        /// A general purpose parallel I/O expansion for I2C or SPI applications.
        /// </summary>
        /// <param name="deviceAddress">The device address for the connection on the I2C or SPI bus.</param>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">The input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">The input pin number that is connected to the interrupt for Port B (INTB).</param>
        public Mcp23S17(int deviceAddress, SpiDevice spiDevice, int? reset = null, int? interruptA = null, int? interruptB = null)
            : base(deviceAddress, spiDevice, reset, interruptA, interruptB)
        {
        }

        /// <summary>
        /// The I/O pin count of the device.
        /// </summary>
        public override int PinCount => 16;
    }
}

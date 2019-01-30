// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Mcp23xxx
{
    public class Mcp23S09 : Mcp23Sxx
    {
        /// <summary>
        /// Initializes new instance of Mcp23S09 device.
        /// A general purpose parallel I/O expansion for SPI applications.
        /// </summary>
        /// <param name="deviceAddress">The device address for the connection on the SPI bus.</param>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">The input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">The input pin number that is connected to the interrupt for Port B (INTB).</param>
        public Mcp23S09(int deviceAddress, SpiDevice spiDevice, int? reset = null, int? interruptA = null, int? interruptB = null)
           : base(deviceAddress, spiDevice, reset, interruptA, interruptB)
        {
        }

        /// <summary>
        /// The I/O pin count of the device.
        /// </summary>
        public override int PinCount => 8;

        /// <summary>
        /// Reads a byte from a register.
        /// </summary>
        /// <param name="registerAddress">The register address to read.</param>
        /// <returns>The data read from the register.</returns>
        public byte Read(Register.Address registerAddress)
        {
            return Read(registerAddress, Port.PortA, Bank.Bank1);
        }

        /// <summary>
        /// Reads a number of bytes from registers.
        /// </summary>
        /// <param name="startingRegisterAddress">The starting register address to read.</param>
        /// <param name="byteCount">The number of bytes to read.</param>
        /// <returns>The data read from the registers.</returns>
        public byte[] Read(Register.Address startingRegisterAddress, byte byteCount)
        {
            return Read(startingRegisterAddress, byteCount, Port.PortA, Bank.Bank1);
        }

        /// <summary>
        ///  Writes a byte to a register.
        /// </summary>
        /// <param name="registerAddress">The register address to write.</param>
        /// <param name="data">The data to write to the register.</param>
        public void Write(Register.Address registerAddress, byte data)
        {
            Write(registerAddress, data, Port.PortA, Bank.Bank1);
        }

        /// <summary>
        /// Writes a number of bytes to registers.
        /// </summary>
        /// <param name="startingRegisterAddress">The starting register address to write.</param>
        /// <param name="data">The data to write to registers.</param>
        public void Write(Register.Address startingRegisterAddress, byte[] data)
        {
            Write(startingRegisterAddress, data, Port.PortA, Bank.Bank1);
        }
    }
}

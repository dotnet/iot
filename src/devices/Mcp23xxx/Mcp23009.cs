// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    public class Mcp23009 : Mcp230xx
    {
        /// <summary>
        /// Initializes new instance of Mcp23009 device.
        /// A general purpose parallel I/O expansion for I2C applications.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">The input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">The input pin number that is connected to the interrupt for Port B (INTB).</param>
        public Mcp23009(I2cDevice i2cDevice, int? reset = null, int? interruptA = null, int? interruptB = null)
            : base(i2cDevice, reset, interruptA, interruptB)
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

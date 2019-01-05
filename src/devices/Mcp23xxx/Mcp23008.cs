// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    public class Mcp23008 : Mcp230xx
    {
        /// <summary>
        /// Initializes new instance of Mcp23008 device.
        /// A general purpose parallel I/O expansion for I2C applications.
        /// </summary>
        /// <param name="i2cDevice">I2C device used for communication.</param>
        /// <param name="reset">Output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">Input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">Input pin number that is connected to the interrupt for Port B (INTB).</param>
        public Mcp23008(I2cDevice i2cDevice, int? reset = null, int? interruptA = null, int? interruptB = null)
            : base(i2cDevice, reset, interruptA, interruptB)
        {
        }

        public override int PinCount => 8;

        public byte Read(Register.Address registerAddress)
        {
            return Read(registerAddress, Port.PortA, Bank.Bank1);
        }

        public byte[] Read(Register.Address startingRegisterAddress, byte byteCount)
        {
            return Read(startingRegisterAddress, byteCount, Port.PortA, Bank.Bank1);
        }

        public void Write(Register.Address registerAddress, byte data)
        {
            Write(registerAddress, data, Port.PortA, Bank.Bank1);
        }

        public void Write(Register.Address startingRegisterAddress, byte[] data)
        {
            Write(startingRegisterAddress, data, Port.PortA, Bank.Bank1);
        }
    }
}

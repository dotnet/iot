// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// Wraps 8 bit MCP I/O extenders
    /// </summary>
    public abstract class Mcp23x0x : Mcp23xxx
    {
        public Mcp23x0x(IBusDevice device, int deviceAddress,  int reset = -1, int interruptA = -1, int interruptB = -1)
            : base(device, deviceAddress, reset, interruptA, interruptB)
        {
        }

        public override int PinCount => 8;

        /// <summary>
        /// Read a byte from the given register.
        /// </summary>
        public byte ReadByte(Register register) => InternalReadByte(register, Port.PortA);

        /// <summary>
        /// Write a byte to the given register.
        /// </summary>
        public void WriteByte(Register register, byte value) => InternalWriteByte(register, value, Port.PortA);

    }
}

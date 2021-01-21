// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// Wraps 16-bit MCP I/O expanders.
    /// </summary>
    public abstract class Mcp23x1x : Mcp23xxx
    {
        /// <summary>
        /// Constructs Mcp23x1x instance
        /// </summary>
        /// <param name="device">I2C device used to communicate with the device</param>
        /// <param name="reset">Reset pin</param>
        /// <param name="interruptA">Interrupt A pin</param>
        /// <param name="interruptB">Interrupt B pin</param>
        /// <param name="controller">
        /// <see cref="GpioController"/> related with
        /// <paramref name="reset"/> <paramref name="interruptA"/> and <paramref name="interruptB"/> pins
        /// </param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        protected Mcp23x1x(BusAdapter device, int reset, int interruptA, int interruptB, GpioController? controller, bool shouldDispose = true)
            : base(device, reset, interruptA, interruptB, controller, shouldDispose: shouldDispose)
        {
        }

        /// <inheritdoc/>
        protected override int PinCount => 16;

        /// <summary>
        /// Read a byte from the given register on the given port.
        /// </summary>
        public byte ReadByte(Register register, Port port) => InternalReadByte(register, port);

        /// <summary>
        /// Write a byte to the given register on the given port.
        /// </summary>
        public void WriteByte(Register register, byte value, Port port) => InternalWriteByte(register, value, port);

        /// <summary>
        /// Read a ushort from the given register.
        /// </summary>
        public ushort ReadUInt16(Register register) => InternalReadUInt16(register);

        /// <summary>
        /// Write a ushort to the given register. Writes the value to both ports.
        /// </summary>
        public void WriteUInt16(Register register, ushort value) => InternalWriteUInt16(register, value);

        /// <summary>
        /// Reads the interrupt pin for the given port if configured.
        /// </summary>
        public PinValue ReadInterrupt(Port port) => InternalReadInterrupt(port);
    }
}

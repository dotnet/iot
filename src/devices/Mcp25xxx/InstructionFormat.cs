// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// MCP25xxx instruction format
    /// </summary>
    public enum InstructionFormat : byte
    {
        /// <summary>
        /// Writes data to the register beginning at the selected address.
        /// </summary>
        Write = 0b0000_0010,

        /// <summary>
        /// Reads data from the register beginning at the selected address.
        /// </summary>
        Read = 0b0000_0011,

        /// <summary>
        /// Allows the user to set or clear individual bits in a particular register.
        /// </summary>
        BitModify = 0b0000_0101,

        /// <summary>
        /// When loading a transmit buffer, reduces the overhead of a normal WRITE
        /// command by placing the Address Pointer at one of six locations, as
        /// indicated by the 3 lower bits.
        /// </summary>
        LoadTxBuffer = 0b0100_0000,

        /// <summary>
        /// Instructs the controller to begin the message transmission sequence for
        /// any of the transmit buffers.  Buffers are indicated by the 3 lower bits.
        /// </summary>
        RequestToSend = 0b1000_0000,

        /// <summary>
        /// When reading a receive buffer, reduces the overhead of a normal READ
        /// command by placing the Address Pointer at one of four locations, as
        /// indicated by the lower 2nd and 3rd bits.
        /// </summary>
        ReadRxBuffer = 0b1001_0000,

        /// <summary>
        /// Quick polling command that reads several Status bits for transmit and
        /// receive functions.
        /// </summary>
        ReadStatus = 0b1010_0000,

        /// <summary>
        /// Quick polling command that indicates a filter match and message type
        /// (standard, extended and/or remote) of the received message.
        /// </summary>
        RxStatus = 0b1011_0000,

        /// <summary>
        /// Resets the internal registers to the default state, sets Configuration mode.
        /// </summary>
        Reset = 0b1100_0000,
    }
}

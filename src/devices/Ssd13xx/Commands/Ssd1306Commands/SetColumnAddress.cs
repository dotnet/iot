// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    /// <summary>
    /// Represents SetColumnAddress
    /// </summary>
    public class SetColumnAddress : ISsd1306Command
    {
        /// <summary>
        /// This triple byte command specifies column start address and end address of the display data RAM.
        /// This command also sets the column address pointer to column start address. This pointer is used
        /// to define the current read/write column address in graphic display data RAM. If horizontal address
        /// increment mode is enabled by command 20h, after finishing read/write one column data, it is
        /// incremented automatically to the next column address. Whenever the column address pointer finishes
        /// accessing the end column address, it is reset back to start column address and the row address
        /// is incremented to the next row.  This command is only for horizontal or vertical addressing modes.
        /// </summary>
        /// <param name="startAddress">Column start address with a range of 0-127.</param>
        /// <param name="endAddress">Column end address with a range of 0-127.</param>
        public SetColumnAddress(byte startAddress = 0x00, byte endAddress = 0x7F)
        {
            if (startAddress > 0x7F)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress));
            }

            if (endAddress > 0x7F)
            {
                throw new ArgumentOutOfRangeException(nameof(endAddress));
            }

            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0x21;

        /// <summary>
        /// Column start address with a range of 0-127.
        /// </summary>
        public byte StartAddress { get; set; }

        /// <summary>
        /// Column end address with a range of 0-127.
        /// </summary>
        public byte EndAddress { get; set; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, StartAddress, EndAddress };
        }
    }
}

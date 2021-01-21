// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    /// <summary>
    /// Represents SetLowerColumnStartAddressForPageAddressingMode command
    /// </summary>
    public class SetLowerColumnStartAddressForPageAddressingMode : ISsd1306Command
    {
        /// <summary>
        /// This command specifies the lower nibble of the 8-bit column start address for the display
        /// data RAM under Page Addressing Mode. The column address will be incremented by each data access.
        /// This command is only for page addressing mode.
        /// </summary>
        /// <param name="lowerColumnStartAddress">Lower column start address with a range of 0-15.</param>
        public SetLowerColumnStartAddressForPageAddressingMode(byte lowerColumnStartAddress = 0x00)
        {
            if (lowerColumnStartAddress > 0x0F)
            {
                throw new ArgumentOutOfRangeException(nameof(lowerColumnStartAddress));
            }

            LowerColumnStartAddress = lowerColumnStartAddress;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => LowerColumnStartAddress;

        /// <summary>
        /// Lower column start address with a range of 0-15.
        /// </summary>
        public byte LowerColumnStartAddress { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id };
        }
    }
}

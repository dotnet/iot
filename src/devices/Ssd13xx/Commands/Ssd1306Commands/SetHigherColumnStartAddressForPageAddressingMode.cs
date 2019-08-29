// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    /// <summary>
    /// Represents SetHigherColumnStartAddressForPageAddressingMode command
    /// </summary>
    public class SetHigherColumnStartAddressForPageAddressingMode : ISsd1306Command
    {
        /// <summary>
        /// This command specifies the higher nibble of the 8-bit column start address for the display
        /// data RAM under Page Addressing Mode. The column address will be incremented by each data access.
        /// This command is only for page addressing mode.
        /// </summary>
        /// <param name="higherColumnStartAddress">Higher column start address with a range of 0-15.</param>
        public SetHigherColumnStartAddressForPageAddressingMode(byte higherColumnStartAddress = 0x00)
        {
            if (higherColumnStartAddress > 0x0F)
            {
                throw new ArgumentOutOfRangeException(nameof(higherColumnStartAddress));
            }

            HigherColumnStartAddress = higherColumnStartAddress;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => (byte)(0x10 | HigherColumnStartAddress);

        /// <summary>
        /// Higher column start address with a range of 0-15.
        /// </summary>
        public byte HigherColumnStartAddress { get; }

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

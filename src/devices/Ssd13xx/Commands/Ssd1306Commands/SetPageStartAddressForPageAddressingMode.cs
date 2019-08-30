// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    /// <summary>
    /// Represents SetPageStartAddressForPageAddressingMode command
    /// </summary>
    public class SetPageStartAddressForPageAddressingMode : ISsd1306Command
    {
        /// <summary>
        /// This command positions the page start address from 0 to 7 in GDDRAM under Page Addressing Mode.
        /// </summary>
        /// <param name="startAddress">Page start address with a range of 0-7.</param>
        public SetPageStartAddressForPageAddressingMode(PageAddress startAddress = PageAddress.Page0)
        {
            StartAddress = startAddress;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => (byte)(0xB0 + StartAddress);

        /// <summary>
        /// Page start address with a range of 0-7.
        /// </summary>
        public PageAddress StartAddress { get; }

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

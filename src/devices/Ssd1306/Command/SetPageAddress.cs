// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetPageAddress : ICommand
    {
        /// <summary>
        /// This triple byte command specifies page start address and end address of the display data RAM.
        /// This command also sets the page address pointer to page start address. This pointer is used to
        /// define the current read/write page address in graphic display data RAM. If vertical address
        /// increment mode is enabled by command 20h, after finishing read/write one page data, it is
        /// incremented automatically to the next page address. Whenever the page address pointer finishes
        /// accessing the end page address, it is reset back to start page address.
        /// This command is only for horizontal or vertical addressing modes.
        /// </summary>
        /// <param name="startAddress">Page start address with a range of 0-7.</param>
        /// <param name="endAddress">Page end address with a range of 0-7.</param>
        public SetPageAddress(PageAddress startAddress = PageAddress.Page0, PageAddress endAddress = PageAddress.Page7)
        {
            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0x22;

        /// <summary>
        /// Page start address with a range of 0-7.
        /// </summary>
        public PageAddress StartAddress { get; set; }

        /// <summary>
        /// Page end address with a range of 0-7.
        /// </summary>
        public PageAddress EndAddress { get; set; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, (byte)StartAddress, (byte)EndAddress };
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    /// <summary>
    /// Page address
    /// </summary>
    public enum PageAddress
    {
        /// <summary>Page0</summary>
        Page0 = 0x00,

        /// <summary>Page1</summary>
        Page1 = 0x01,

        /// <summary>Page2</summary>
        Page2 = 0x02,

        /// <summary>Page3</summary>
        Page3 = 0x03,

        /// <summary>Page4</summary>
        Page4 = 0x04,

        /// <summary>Page5</summary>
        Page5 = 0x05,

        /// <summary>Page6</summary>
        Page6 = 0x06,

        /// <summary>Page6</summary>
        Page7 = 0x07
    }

    /// <summary>
    /// Represents SetPageAddress command
    /// </summary>
    public class SetPageAddress : ISsd1306Command
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

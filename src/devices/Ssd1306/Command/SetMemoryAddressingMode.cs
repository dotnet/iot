// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetMemoryAddressingMode : ICommand
    {
        /// <summary>
        /// This command sets the memory addressing mode.
        /// </summary>
        /// <param name="memoryAddressingMode">Memory addressing mode.</param>
        public SetMemoryAddressingMode(AddressingMode memoryAddressingMode = AddressingMode.Page)
        {
            MemoryAddressingMode = memoryAddressingMode;
        }

        public byte Id => 0x20;

        /// <summary>
        /// Memory addressing mode.
        /// </summary>
        public AddressingMode MemoryAddressingMode { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Id, (byte)MemoryAddressingMode };
        }

        public enum AddressingMode
        {
            /// <summary>
            /// In horizontal addressing mode, after the display RAM is read/written, the column address
            /// pointer is increased automatically by 1. If the column address pointer reaches column end
            /// address, the column address pointer is reset to column start address and page address
            /// pointer is increased by 1. When both column and page address pointers reach the end address,
            /// the pointers are reset to column start address and page start address.
            /// </summary>
            Horizontal = 0x00,
            /// <summary>
            /// In vertical addressing mode, after the display RAM is read/written, the page address pointer
            /// is increased automatically by 1. If the page address pointer reaches the page end address,
            /// the page address pointer is reset to page start address and column address pointer is
            /// increased by 1. When both column and page address pointers reach the end address, the
            /// pointers are reset to column start address and page start address.
            /// </summary>
            Vertical = 0x01,
            /// <summary>
            /// In page addressing mode, after the display RAM is read/written, the column address pointer
            /// is increased automatically by 1. If the column address pointer reaches column end address,
            /// the column address pointer is reset to column start address and page address pointer is not
            /// changed. Users have to set the new page and column addresses in order to access the next
            /// page RAM content.
            /// </summary>
            Page = 0x02,
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd1306.Command
{
    public class SetLowerColumnStartAddressForPageAddressingMode : ICommand
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
                throw new ArgumentException("The lower column start address is invalid.", nameof(lowerColumnStartAddress));
            }

            LowerColumnStartAddress = lowerColumnStartAddress;
        }

        public byte Id => LowerColumnStartAddress;

        /// <summary>
        /// Lower column start address with a range of 0-15.
        /// </summary>
        public byte LowerColumnStartAddress { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Id };
        }
    }
}

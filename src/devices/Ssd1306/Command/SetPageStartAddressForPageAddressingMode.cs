// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd1306.Command
{
    public class SetPageStartAddressForPageAddressingMode : ICommand
    {
        /// <summary>
        /// This command positions the page start address from 0 to 7 in GDDRAM under Page Addressing Mode.
        /// </summary>
        /// <param name="startAddress">Page start address with a range of 0-7.</param>
        public SetPageStartAddressForPageAddressingMode(PageAddress startAddress = 0x00)
        {
            if ((byte)startAddress > 0x07)
            {
                throw new ArgumentException("The page start address is invalid.", nameof(startAddress));
            }

            StartAddress = startAddress;
        }

        public byte Value => (byte)(0xB0 + StartAddress);

        /// <summary>
        /// Page start address with a range of 0-7.
        /// </summary>
        public PageAddress StartAddress { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}

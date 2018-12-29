// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetPageStartAddressForPageAddressingMode : ICommand
    {
        public SetPageStartAddressForPageAddressingMode(PageAddress startAddress = 0x00)
        {
            // TODO: Validate values.   Ranges from 0x00 to 0x07.

            StartAddress = startAddress;
        }

        public byte Value => (byte)(0xB0 + StartAddress);

        public PageAddress StartAddress { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}

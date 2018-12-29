// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetPageAddress : ICommand
    {
        public SetPageAddress(PageAddress startAddress = PageAddress.Page0, PageAddress endAddress = PageAddress.Page7)
        {
            // TODO: Validate values. 0x00 - 0x07.

            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        public byte Value => 0x22;

        public PageAddress StartAddress { get; set; }

        public PageAddress EndAddress { get; set; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, (byte)StartAddress, (byte)EndAddress };
        }
    }
}

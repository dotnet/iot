// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetHigherColumnStartAddressForPageAddressingMode : ICommand
    {
        public SetHigherColumnStartAddressForPageAddressingMode(byte higherColumnStartAddress = 0x00)
        {
            // TODO: Validate value. 0x00 - 0x0F;

            HigherColumnStartAddress = higherColumnStartAddress;
        }

        public byte Value => (byte)(0x10 | HigherColumnStartAddress);

        public byte HigherColumnStartAddress { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}

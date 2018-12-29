// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetLowerColumnStartAddressForPageAddressingMode : ICommand
    {
        public SetLowerColumnStartAddressForPageAddressingMode(byte lowerColumnStartAddress = 0x00)
        {
            // TODO: Validate value. 0x00 - 0x0F;

            LowerColumnStartAddress = lowerColumnStartAddress;
        }

        public byte Value => LowerColumnStartAddress;

        public byte LowerColumnStartAddress { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}

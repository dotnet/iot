// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetMemoryAddressingMode : ICommand
    {
        public SetMemoryAddressingMode(AddressingMode memoryAddressingMode = AddressingMode.Page)
        {
            // TODO: Validate value. 0x00 - 0x03;

            MemoryAddressingMode = memoryAddressingMode;
        }

        public byte Value => 0x20;

        public AddressingMode MemoryAddressingMode { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, (byte)MemoryAddressingMode };
        }

        public enum AddressingMode
        {
            Horizontal = 0x00,            
            Vertical = 0x01,
            Page = 0x02,
        }
    }
}

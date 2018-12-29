// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetColumnAddress : ICommand
    {
        public SetColumnAddress(byte startAddress = 0x00, byte endAddress = 0x80)
        {
            // TODO: Validate values. 0 - 127.

            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        public byte Value => 0x21;

        public byte StartAddress { get; set; }

        public byte EndAddress { get; set; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, StartAddress, EndAddress };
        }
    }
}

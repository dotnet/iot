// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetDisplayOffset : ICommand
    {
        public SetDisplayOffset(byte displayOffset = 0x00)
        {
            // Validate values: 0 - 63.

            DisplayOffset = displayOffset;
        }

        public byte Value => 0xD3;

        public byte DisplayOffset { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, DisplayOffset };
        }
    }
}

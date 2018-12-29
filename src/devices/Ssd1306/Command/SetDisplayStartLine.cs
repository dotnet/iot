// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetDisplayStartLine : ICommand
    {
        public SetDisplayStartLine(byte displayStartLine = 0x00)
        {
            // TODO: Validate values.   Ranges from 0 to 63.

            DisplayStartLine = displayStartLine;
        }

        public byte Value => (byte)(0x40 + DisplayStartLine);

        public byte DisplayStartLine { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}

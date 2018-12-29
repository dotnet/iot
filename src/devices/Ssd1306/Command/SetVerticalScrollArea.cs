// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetVerticalScrollArea : ICommand
    {
        public SetVerticalScrollArea(byte topFixedAreaRows = 0x00, byte scrollAreaRows = 0x40)
        {
            TopFixedAreaRows = topFixedAreaRows;
            ScrollAreaRows = scrollAreaRows;
        }

        public byte Value => 0xA3;

        public byte TopFixedAreaRows { get; }

        public byte ScrollAreaRows { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, TopFixedAreaRows, ScrollAreaRows };
        }
    }
}
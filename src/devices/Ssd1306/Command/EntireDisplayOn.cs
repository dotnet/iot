// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class EntireDisplayOn : ICommand
    {
        public EntireDisplayOn(bool entireDisplay = false)
        {
            EntireDisplay = entireDisplay;
        }

        public byte Value => (byte)(EntireDisplay ? 0xA5 : 0xA4);

        public bool EntireDisplay { get; set; }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}

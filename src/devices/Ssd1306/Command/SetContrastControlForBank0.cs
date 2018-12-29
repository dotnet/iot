// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetContrastControlForBank0 : ICommand
    {
        public SetContrastControlForBank0(byte contrastSetting = 0x7F)
        {
            ContrastSetting = contrastSetting;
        }

        public byte Value => 0x81;

        public byte ContrastSetting { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, ContrastSetting };
        }
    }
}

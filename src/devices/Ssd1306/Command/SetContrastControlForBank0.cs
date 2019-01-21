// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetContrastControlForBank0 : ICommand
    {
        /// <summary>
        /// This command sets the Contrast Setting of the display.
        /// The chip has 256 contrast steps from 00h to FFh.
        /// The segment output current increases as the contrast step value increases.
        /// </summary>
        /// <param name="contrastSetting">Contrast setting.</param>
        public SetContrastControlForBank0(byte contrastSetting = 0x7F)
        {
            ContrastSetting = contrastSetting;
        }

        public byte Value => 0x81;

        /// <summary>
        /// Contrast setting.
        /// </summary>
        public byte ContrastSetting { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, ContrastSetting };
        }
    }
}

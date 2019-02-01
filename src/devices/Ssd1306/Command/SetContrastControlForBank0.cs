// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetContrastControlForBank0 : ICommand
    {
        /// <summary>
        /// This command sets the Contrast Setting of the display.
        /// The chip has 256 contrast steps from 0 to 255.
        /// The segment output current increases as the contrast step value increases.
        /// </summary>
        /// <param name="contrastSetting">Contrast setting with a range of 0-255.</param>
        public SetContrastControlForBank0(byte contrastSetting = 0x7F)
        {
            ContrastSetting = contrastSetting;
        }

        public byte Id => 0x81;

        /// <summary>
        /// Contrast setting with a range of 0-255.
        /// </summary>
        public byte ContrastSetting { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Id, ContrastSetting };
        }
    }
}

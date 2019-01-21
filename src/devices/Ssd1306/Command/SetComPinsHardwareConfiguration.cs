// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetComPinsHardwareConfiguration : ICommand
    {
        /// <summary>
        /// This command sets the COM signals pin configuration to match the OLED panel hardware layout.
        /// </summary>
        /// <param name="alternativeComPinConfiguration">Alternative COM pin configuration.</param>
        /// <param name="enableLeftRightRemap">Enable left/right remap.</param>
        public SetComPinsHardwareConfiguration(bool alternativeComPinConfiguration = true, bool enableLeftRightRemap = false)
        {
            AlternativeComPinConfiguration = alternativeComPinConfiguration;
            EnableLeftRightRemap = enableLeftRightRemap;
        }

        public byte Value => 0xDA;

        /// <summary>
        /// Alternative COM pin configuration.
        /// </summary>
        public bool AlternativeComPinConfiguration { get; }

        /// <summary>
        /// Enable left/right remap.
        /// </summary>
        public bool EnableLeftRightRemap { get; }

        public byte[] GetBytes()
        {
            byte comPinsHardwareConfiguration = 0x02;

            if (AlternativeComPinConfiguration)
            {
                comPinsHardwareConfiguration |= 0x10;
            }

            if (EnableLeftRightRemap)
            {
                comPinsHardwareConfiguration |= 0x20;
            }

            return new byte[] { Value, comPinsHardwareConfiguration };
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// This CfgItem is used to choose the analog settings that the PN532 will use for the
    /// type B when configured as PCD.
    /// When using this command, the host controller has to provide 3 new values
    /// (ConfigurationData[]) for the following internal registers:     /// Except for these two specific settings, the 8 remaining analog settings are the
    /// same as the CfgItem 106 kbps type A.
    /// Please refer to documentation for more information
    /// </summary>
    public class AnalogSettingsTypeBMode
    {
        public byte CIU_GsNOn { get; set; } = 0xFF;
        public byte CIU_ModGsP { get; set; } = 0x17;
        public byte CIU_RxThreshold { get; set; } = 0x85;

        /// <summary>
        /// Get the byte array to send
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            return new byte[3] {
                CIU_GsNOn,
                CIU_ModGsP,
                CIU_RxThreshold,
            };
        }
    }
}

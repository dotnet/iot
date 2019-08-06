// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// This CfgItem is used to choose the analog settings that the PN532 will use for the
    /// baudrates 212/424kbps.
    /// When using this command, the host controller has to provide 8 values
    /// (ConfigurationData[]) for the following internal registers
    /// Actually, there is only one CIU_Demod register which defines a setting used by the
    /// reader in reception only.But depending on the RF condition, two different settings
    /// can be used for this register:
    /// • CIU_Demod when own RF is On defines a setting when its RF field is on
    /// during a reception i.e.initiator passive mode,
    /// • CIU_Demod when own RF is Off defines a setting when its RF field is off
    /// during a reception i.e.initiator active mode.
    /// Please refer to documentation for more information
    /// </summary>
    public class Analog212_424kbpsMode
    {
        /// <summary>
        /// RfCfg, cf page 105 documentation 141520.pdf
        /// </summary>
        public byte CIU_RfConfiguration { get; set; } = 0x69;

        /// <summary>
        /// GsNon, cf page 105 documentation 141520.pdf
        /// </summary>
        public byte CIU_GsNOn { get; set; } = 0xFF;

        /// <summary>
        /// CWGsP, cf page 105 documentation 141520.pdf
        /// </summary>
        public byte CIU_CWGsP { get; set; } = 0x3F;

        /// <summary>
        /// ModGsP, cf page 105 documentation 141520.pdf
        /// </summary>
        public byte CIU_ModGsP { get; set; } = 0x11;

        /// <summary>
        /// DmodWhenRfOn, cf page 105 documentation 141520.pdf
        /// </summary>
        public byte CIU_DemodWhenRfOn { get; set; } = 0x41;

        /// <summary>
        /// RxThreshold, cf page 105 documentation 141520.pdf
        /// </summary>
        public byte CIU_RxThreshold { get; set; } = 0x85;

        /// <summary>
        /// DemodWhenRfOff, cf page 105 documentation 141520.pdf
        /// </summary>
        public byte CIU_DemodWhenRfOff { get; set; } = 0x61;

        /// <summary>
        /// GsNOff, cf page 105 documentation 141520.pdf
        /// </summary>
        public byte CIU_GsNOff { get; set; } = 0x6F;

        /// <summary>
        /// Get the byte array to send
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            return new byte[8] {
                CIU_RfConfiguration, CIU_GsNOn, CIU_CWGsP,
                CIU_ModGsP, CIU_DemodWhenRfOn,
                CIU_RxThreshold, CIU_DemodWhenRfOff,
                CIU_GsNOff
            };
        }
    }
}

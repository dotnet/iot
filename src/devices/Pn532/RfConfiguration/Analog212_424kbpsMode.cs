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
    /// CIU = Contactless Interface Unit
    /// Please refer to https://www.nxp.com/docs/en/nxp/data-sheets/PN532_C1.pdf page 144
    /// </summary>
    public class Analog212_424kbpsMode
    {
        /// <summary>
        /// RfCfg, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// </summary>
        public byte RfConfiguration { get; set; } = 0x69;

        /// <summary>
        /// GsNon, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Selects the conductance of the antenna driver pins TX1 and
        /// TX2 for modulation, when own RF field is switched on
        /// </summary>
        public byte GsNOn { get; set; } = 0xFF;

        /// <summary>
        /// CWGsP, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Selects the conductance of the antenna driver pins TX1 and
        /// TX2 when not in modulation phase
        /// </summary>
        public byte CWGsP { get; set; } = 0x3F;

        /// <summary>
        /// ModGsP, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        ///  Selects the conductance of the antenna driver pins TX1 and
        /// TX2 when in modulation phase
        /// </summary>
        public byte ModGsP { get; set; } = 0x11;

        /// <summary>
        /// DmodWhenRfOn, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Defines demodulator settings when radio frequency is on
        /// </summary>
        public byte DemodWhenRfOn { get; set; } = 0x41;

        /// <summary>
        /// RxThreshold, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Selects thresholds for the bit decoder
        /// </summary>
        public byte RxThreshold { get; set; } = 0x85;

        /// <summary>
        /// DemodWhenRfOff, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Defines demodulator settings when radio frequency is off
        /// </summary>
        public byte DemodWhenRfOff { get; set; } = 0x61;

        /// <summary>
        /// GsNOff, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Selects the conductance of the antenna driver pins TX1 and
        /// TX2 for load modulation when own RF field is switched off
        /// </summary>
        public byte GsNOff { get; set; } = 0x6F;

        /// <summary>
        /// Get the byte array to send
        /// </summary>
        /// <returns>Serialized value</returns>
        public byte[] Serialize()
        {
            return new byte[8]
            {
                RfConfiguration, GsNOn, CWGsP,
                ModGsP, DemodWhenRfOn,
                RxThreshold, DemodWhenRfOff,
                GsNOff
            };
        }
    }
}

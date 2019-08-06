// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// This CfgItem is used to choose the analog settings that the PN532 will use for the
    /// baudrate 106kbps type A.
    /// When using this command, the host controller has to provide 11 values
    /// (ConfigurationData[]) for the following internal registers
    /// Actually, there is only one CIU_Demod register which defines a setting used by the
    /// reader in reception only.But depending on the RF condition, two different settings
    /// can be used for this register:
    /// • CIU_Demod when own RF is On defines a setting when its RF field is on
    /// during a reception i.e.initiator passive mode,
    /// • CIU_Demod when own RF is Off defines a setting when its RF field is off
    /// during a reception i.e.initiator active mode.
    /// Please refer to document for detailed documentation
    /// </summary>
    public class Analog106kbpsTypeAMode
    {
        /// <summary>
        /// RFCfg, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_RfConfiguration { get; set; } = 0x59;

        /// <summary>
        /// GsNOn, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_GsNOn { get; set; } = 0xF4;

        /// <summary>
        /// CWGsP, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_CWGsP { get; set; } = 0x3F;

        /// <summary>
        /// ModGsP, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_ModGsP { get; set; } = 0x11;

        /// <summary>
        /// DemodWhenRfOn, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_DemodWhenRfOn { get; set; } = 0x4D;

        /// <summary>
        /// RxThreshold, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_RxThreshold { get; set; } = 0x85;

        /// <summary>
        /// DemodWhenRfOff, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_DemodWhenRfOff { get; set; } = 0x61;

        /// <summary>
        /// GsNOff, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_GsNOff { get; set; } = 0x6F;

        /// <summary>
        /// ModWidth, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_ModWidth { get; set; } = 0x26;

        /// <summary>
        /// MifNFC, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_MifNFC { get; set; } = 0x62;

        /// <summary>
        /// TxBitPhase, cf page 104 documentation 141520.pdf
        /// </summary>
        public byte CIU_TxBitPhase { get; set; } = 0x87;

        /// <summary>
        /// Get the byte array to send
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            return new byte[11] {
                CIU_RfConfiguration, CIU_GsNOn, CIU_CWGsP,
                CIU_ModGsP, CIU_DemodWhenRfOn,
                CIU_RxThreshold, CIU_DemodWhenRfOff,
                CIU_GsNOff, CIU_ModWidth,
                CIU_MifNFC, CIU_TxBitPhase
            };
        }
    }
}

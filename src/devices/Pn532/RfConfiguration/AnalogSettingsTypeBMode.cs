// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// This CfgItem is used to choose the analog settings that the PN532 will use for the
    /// type B when configured as PCD.
    /// When using this command, the host controller has to provide 3 new values
    /// (ConfigurationData[]) for the following internal registers:
    /// Except for these two specific settings, the 8 remaining analog settings are the
    /// same as the CfgItem 106 kbps type A.
    /// CIU = Contactless Interface Unit
    /// Please refer to https://www.nxp.com/docs/en/nxp/data-sheets/PN532_C1.pdf page 144
    /// </summary>
    public class AnalogSettingsTypeBMode
    {
        /// <summary>
        /// GsNOn, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Selects the conductance of the antenna driver pins TX1 and
        /// TX2 for modulation, when own RF field is switched on
        /// </summary>
        public byte GsNOn { get; set; } = 0xFF;

        /// <summary>
        /// ModGsP, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Selects the conductance of the antenna driver pins TX1 and
        /// TX2 when in modulation phase
        /// </summary>
        public byte ModGsP { get; set; } = 0x17;

        /// <summary>
        /// RxThreshold, cf page 105 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Selects thresholds for the bit decoder
        /// </summary>
        public byte RxThreshold { get; set; } = 0x85;

        /// <summary>
        /// Get the byte array to send
        /// </summary>
        /// <returns>Serialized value</returns>
        public byte[] Serialize()
        {
            return new byte[3]
            {
                GsNOn,
                ModGsP,
                RxThreshold,
            };
        }
    }
}

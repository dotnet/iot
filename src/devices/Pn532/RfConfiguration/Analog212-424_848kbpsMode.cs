// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// This CfgItem is used to choose the analog settings that the PN532 will use for the
    /// baudrates 212/424/848 kbps with ISO/IEC14443-4 cards.
    /// When using this command, the host controller has to provide 9 values
    /// (ConfigurationData[]) for the following internal registers: 
    /// Except for these three specific registers (CIU_RxThreshold, CIU_ModWidth and
    /// CIU_MifNFC), the 8 remaining analog registers are the same as the previous
    /// CfgItem 0x0A. 
    /// CIU = Contactless Interface Unit 
    /// Please refer to https://www.nxp.com/docs/en/nxp/data-sheets/PN532_C1.pdf page 144
    /// </summary>
    public class Analog212_424_848kbpsMode
    {
        /// <summary>
        /// RxThreshold212, cf page 106 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Selects thresholds for the bit decoder for 212 kbps
        /// </summary>
        public byte RxThreshold212 { get; set; } = 0x85;

        /// <summary>
        /// ModWidth212, cf page 106 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Controls the setting of the width of the Miller pause for 212 kbps
        /// </summary>
        public byte ModWidth212 { get; set; } = 0x15;

        /// <summary>
        /// MifNFC212, cf page 106 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Controls the communication in ISO/IEC 14443/MIFARE and
        /// NFC target mode at 212 kbit/s
        /// </summary>
        public byte MifNFC212 { get; set; } = 0x8A;

        /// <summary>
        /// RxThreshold424, cf page 106 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Selects thresholds for the bit decoder for 424 kbps
        /// </summary>
        public byte RxThreshold424 { get; set; } = 0x85;

        /// <summary>
        /// ModWidth424, cf page 106 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Controls the setting of the width of the Miller pause for 424 kbps
        /// </summary>
        public byte ModWidth424 { get; set; } = 0x08;

        /// <summary>
        /// MifNFC424, cf page 106 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// NFC target mode at 424 kbit/s
        /// </summary>
        public byte MifNFC424 { get; set; } = 0xB2;

        /// <summary>
        /// RxThreshold848, cf page 106 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Selects thresholds for the bit decoder for 848 kbps
        /// </summary>
        public byte RxThreshold848 { get; set; } = 0x85;

        /// <summary>
        /// ModWidth848, cf page 106 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// Controls the setting of the width of the Miller pause for 848 kbps
        /// </summary>
        public byte ModWidth848 { get; set; } = 0x01;

        /// <summary>
        /// MifNFC848, cf page 106 documentation 141520.pdf and page 144 documentation PN532_C1.pdf
        /// NFC target mode at 848 kbit/s
        /// </summary>
        public byte MifNFC848 { get; set; } = 0xDA;

        /// <summary>
        /// Get the byte array to send
        /// </summary>
        /// <returns>Serialized value</returns>
        public byte[] Serialize()
        {
            return new byte[9] {
                RxThreshold212, ModWidth212, MifNFC212,
                RxThreshold424, ModWidth424, MifNFC424,
                RxThreshold848, ModWidth848, MifNFC848,
            };
        }
    }
}

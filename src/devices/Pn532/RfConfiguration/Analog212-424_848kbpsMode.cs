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
    /// Please refer to documentation for more information
    /// </summary>
    public class Analog212_424_848kbpsMode
    {
        /// <summary>
        /// RxThreshold212, cf page 106 documentation 141520.pdf
        /// </summary>
        public byte CIU_RxThreshold212 { get; set; } = 0x85;

        /// <summary>
        /// ModWidth212, cf page 106 documentation 141520.pdf
        /// </summary>
        public byte CIU_ModWidth212 { get; set; } = 0x15;

        /// <summary>
        /// MifNFC212, cf page 106 documentation 141520.pdf
        /// </summary>
        public byte CIU_MifNFC212 { get; set; } = 0x8A;

        /// <summary>
        /// RxThreshold424, cf page 106 documentation 141520.pdf
        /// </summary>
        public byte CIU_RxThreshold424 { get; set; } = 0x85;

        /// <summary>
        /// ModWidth424, cf page 106 documentation 141520.pdf
        /// </summary>
        public byte CIU_ModWidth424 { get; set; } = 0x08;

        /// <summary>
        /// MifNFC424, cf page 106 documentation 141520.pdf
        /// </summary>
        public byte CIU_MifNFC424 { get; set; } = 0xB2;

        /// <summary>
        /// RxThreshold848, cf page 106 documentation 141520.pdf
        /// </summary>
        public byte CIU_RxThreshold848 { get; set; } = 0x85;

        /// <summary>
        /// ModWidth848, cf page 106 documentation 141520.pdf
        /// </summary>
        public byte CIU_ModWidth848 { get; set; } = 0x01;

        /// <summary>
        /// MifNFC848, cf page 106 documentation 141520.pdf
        /// </summary>
        public byte CIU_MifNFC848 { get; set; } = 0xDA;

        /// <summary>
        /// Get the byte array to send
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            return new byte[9] {
                CIU_RxThreshold212, CIU_ModWidth212, CIU_MifNFC212,
                CIU_RxThreshold424, CIU_ModWidth424, CIU_MifNFC424,
                CIU_RxThreshold848, CIU_ModWidth848, CIU_MifNFC848,
            };
        }
    }
}

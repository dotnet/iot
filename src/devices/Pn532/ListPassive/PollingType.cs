// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.ListPassive
{
    /// <summary>
    /// Polling type for all supported cards
    /// </summary>
    public enum PollingType
    {
        /// <summary>
        /// Generic passive 106 kbps (ISO/IEC14443-4A, Mifare and DEP)
        /// </summary>
        GenericPassive106kbps = 0x00,

        /// <summary>
        /// Generic passive 212 kbps (FeliCa and DEP)
        /// </summary>
        GenericPassive212kbps = 0x01,

        /// <summary>
        /// Generic passive 424 kbps (FeliCa and DEP),
        /// </summary>
        GenericPassive424kbps = 0x02,

        /// <summary>
        /// Passive 106 kbps ISO/IEC14443-4B
        /// </summary>
        Passive106kbps = 0x03,

        /// <summary>
        /// Innovision Jewel tag
        /// </summary>
        InnovisionJewel = 0x04,

        /// <summary>
        /// Mifare card
        /// </summary>
        MifareCard = 0x10,

        /// <summary>
        /// FeliCa 212 kbps card
        /// </summary>
        Felica212kbps = 0x11,

        /// <summary>
        /// FeliCa 424 kbps card
        /// </summary>
        Felica424kbps = 0x12,

        /// <summary>
        /// Passive 106 kbps ISO/IEC14443-4A
        /// </summary>
        Passive106kbpsISO144443_4A = 0x20,

        /// <summary>
        /// Passive 106 kbps ISO/IEC14443-4B
        /// </summary>
        Passive106kbpsISO144443_4B = 0x23,

        /// <summary>
        /// DEP passive 106 kbps
        /// </summary>
        DepPassive106kbps = 0x40,

        /// <summary>
        /// DEP passive 212 kbps
        /// </summary>
        DepPassive212kbps = 0x41,

        /// <summary>
        /// DEP passive 424 kbps
        /// </summary>
        DepPassive424kbps = 0x42,

        /// <summary>
        /// DEP active 106 kbps
        /// </summary>
        DepActive106kbps = 0x80,

        /// <summary>
        /// DEP active 212 kbps
        /// </summary>
        DepActive212kbps = 0x81,

        /// <summary>
        /// DEP active 424 kbps
        /// </summary>
        DepActive424kbps = 0x82
    }
}

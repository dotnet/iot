// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// Transmitter radio frequency configuration
    /// </summary>
    public enum TransmitterRadioFrequencyConfiguration
    {
        /// <summary>Protocol: ISO 14443-A / NFC PI-106 Speed (kbit/s): 106</summary>
        Iso14443A_Nfc_PI_106_106 = 0x0,

        /// <summary>Protocol: ISO 14443-A Speed (kbit/s): 212</summary>
        Iso14443A_212 = 0x1,

        /// <summary>Protocol: ISO 14443-A Speed (kbit/s): 424</summary>
        Iso14443A_424 = 0x2,

        /// <summary>Protocol: ISO 14443-A Speed (kbit/s): 848</summary>
        Iso14443A_848 = 0x3,

        /// <summary>Protocol: ISO 14443-B Speed (kbit/s): 106</summary>
        Iso14443B_106 = 0x4,

        /// <summary>Protocol: ISO 14443-B Speed (kbit/s): 212</summary>
        Iso14443B_212 = 0x5,

        /// <summary>Protocol: ISO 14443-B Speed (kbit/s): 424</summary>
        Iso14443B_424 = 0x6,

        /// <summary>Protocol: ISO 14443-B Speed (kbit/s): 848</summary>
        Iso14443B_848 = 0x7,

        /// <summary>Protocol: FeliCa / NFC PI 212 Speed (kbit/s): 212</summary>
        FeliCa_Nfc_PI_212_212 = 0x8,

        /// <summary>Protocol: FeliCa / NFC PI 424 Speed (kbit/s): 424</summary>
        FeliCa_Nfc_PI_424_424 = 0x9,

        /// <summary>Protocol: NFC-Active Initiator Speed (kbit/s): 106</summary>
        Nfc_Active_Initiator_106 = 0x0A,

        /// <summary>Protocol: NFC-Active Initiator Speed (kbit/s): 212</summary>
        Nfc_Active_Initiator_212 = 0x0B,

        /// <summary>Protocol: NFC-Active Initiator Speed (kbit/s): 424</summary>
        Nfc_Active_Initiator_424 = 0x0C,

        /// <summary>Protocol: ISO 15693 ASK100 Speed (kbit/s): 26</summary>
        Iso15693_ASK100_26 = 0x0D,

        /// <summary>Protocol: ISO 15693 ASK10 Speed (kbit/s): 26</summary>
        Iso15693_ASK10_26 = 0x0E,

        /// <summary>Protocol: ISO 18003M3 Manch. 424_4 Speed (kbit/s): Tari=18.88</summary>
        Iso18003M3_Manch_424_4_Tari_18_88 = 0x0F,

        /// <summary>Protocol: ISO 18003M3 Manch. 424_2 Speed (kbit/s): Tari=9.44</summary>
        Iso18003M3_Manch_424_2_Tari_9_44 = 0x10,

        /// <summary>Protocol: ISO 18003M3 Manch. 848_4 Speed (kbit/s): Tari=18.88</summary>
        Iso18003M3_Manch_848_4_Tari_18_88 = 0x11,

        /// <summary>Protocol: ISO 18003M3 Manch. 848_2 Speed (kbit/s): Tari=9.44</summary>
        Iso18003M3_Manch_848_2_Tari_9_44 = 0x12,

        /// <summary>Protocol: ISO 18003M3 Manch. 424_4 Speed (kbit/s): 106</summary>
        Iso18003M3_Manch_424_4_106 = 0x13,

        /// <summary>Protocol: ISO 14443-A PICC Speed (kbit/s): 212</summary>
        Iso14443A_PICC_212 = 0x14,

        /// <summary>Protocol: ISO 14443-A PICC Speed (kbit/s): 424</summary>
        Iso14443A_PICC_424 = 0x15,

        /// <summary>Protocol: ISO 14443-A PICC Speed (kbit/s): 848</summary>
        Iso14443A_PICC_848 = 0x16,

        /// <summary>Protocol: NFC Passive Target Speed (kbit/s): 212</summary>
        Nfc_PassiveTarget_212 = 0x17,

        /// <summary>Protocol: NFC Passive Target Speed (kbit/s): 424</summary>
        Nfc_PassiveTarget_424 = 0x18,

        /// <summary>Protocol: NFC Active Target 106 Speed (kbit/s): 106</summary>
        Nfc_ActiveTarget_106_106 = 0x19,

        /// <summary>Protocol: NFC Active Target 212 Speed (kbit/s): 212</summary>
        Nfc_ActiveTarget_212_212 = 0x1A,

        /// <summary>Protocol: NFC Active Target 424 Speed (kbit/s): 424</summary>
        Nfc_ActiveTarget_424_424 = 0x1B,

        /// <summary>Protocol: GTM Speed (kbit/s): ALL</summary>
        GTM_ALL = 0x1C,

        /// <summary>No change</summary>
        NoChange = 0xFF,
    }
}

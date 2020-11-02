// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// Receiver radio frequency configuration
    /// </summary>
    public enum ReceiverRadioFrequencyConfiguration
    {
        /// <summary>Protocol: ISO 14443-A / NFC PI-106 Speed (kbit/s): 106</summary>
        Iso14443A_Nfc_PI_106_106 = 0x80,

        /// <summary>Protocol: ISO 14443-A Speed (kbit/s): 212</summary>
        Iso14443A_212 = 0x81,

        /// <summary>Protocol: ISO 14443-A Speed (kbit/s): 424</summary>
        Iso14443A_424 = 0x82,

        /// <summary>Protocol: ISO 14443-A Speed (kbit/s): 848</summary>
        Iso14443A_848 = 0x83,

        /// <summary>Protocol: ISO 14443-B Speed (kbit/s): 106</summary>
        Iso14443B_106 = 0x84,

        /// <summary>Protocol: ISO 14443-B Speed (kbit/s): 212</summary>
        Iso14443B_212 = 0x85,

        /// <summary>Protocol: ISO 14443-B Speed (kbit/s): 424</summary>
        Iso14443B_424 = 0x86,

        /// <summary>Protocol: ISO 14443-B Speed (kbit/s): 848</summary>
        Iso14443B_848 = 0x87,

        /// <summary>Protocol: FeliCa / NFC PI 212 Speed (kbit/s): 212</summary>
        FeliCa_Nfc_PI_212_212 = 0x88,

        /// <summary>Protocol: FeliCa / NFC PI 212 Speed (kbit/s): 424</summary>
        FeliCa_Nfc_PI_212_424 = 0x89,

        /// <summary>Protocol: NFC-Active Initiator Speed (kbit/s): 106</summary>
        Nfc_Active_Initiator_106 = 0x8A,

        /// <summary>Protocol: NFC-Active Initiator Speed (kbit/s): 212</summary>
        Nfc_Active_Initiator_212 = 0x8B,

        /// <summary>Protocol: NFC-Active Initiator Speed (kbit/s): 424</summary>
        Nfc_Active_Initiator_424 = 0x8C,

        /// <summary>Protocol: ISO 15693 Speed (kbit/s): 26</summary>
        Iso15693_26 = 0x8D,

        /// <summary>Protocol: ISO 15693 Speed (kbit/s): 53</summary>
        Iso15693_53 = 0x8E,

        /// <summary>Protocol: ISO 18003M3 Manch. 424_4 Speed (kbit/s): 106</summary>
        Iso18003M3_Manch_424_4_106 = 0x8F,

        /// <summary>Protocol: ISO 18003M3 Manch. 424_2 Speed (kbit/s): 212</summary>
        Iso18003M3_Manch_424_2_212 = 0x90,

        /// <summary>Protocol: ISO 18003M3 Manch. 848_4 Speed (kbit/s): 212</summary>
        Iso18003M3_Manch_848_4_212 = 0x91,

        /// <summary>Protocol: ISO 18003M3 Manch. 848_2 Speed (kbit/s): 424</summary>
        Iso18003M3_Manch_848_2_424 = 0x92,

        /// <summary>Protocol: ISO 14443-A PICC Speed (kbit/s): 106</summary>
        Iso14443A_PICC_106 = 0x93,

        /// <summary>Protocol: ISO 14443-A PICC Speed (kbit/s): 212</summary>
        Iso14443A_PICC_212 = 0x94,

        /// <summary>Protocol: ISO 14443-A PICC Speed (kbit/s): 424</summary>
        Iso14443A_PICC_424 = 0x95,

        /// <summary>Protocol: ISO 14443-A PICC Speed (kbit/s): 848</summary>
        Iso14443A_PICC_848 = 0x96,

        /// <summary>Protocol: NFC Passive Target Speed (kbit/s): 212</summary>
        Nfc_PassiveTarget_212 = 0x97,

        /// <summary>Protocol: NFC Passive Target Speed (kbit/s): 424</summary>
        Nfc_PassiveTarget_424 = 0x98,

        /// <summary>Protocol: ISO 14443-A Speed (kbit/s): 106</summary>
        Iso14443A_Active_106 = 0x99,

        /// <summary>Protocol: ISO 14443-A Speed (kbit/s): 212</summary>
        Iso14443A_Active_212 = 0x9A,

        /// <summary>Protocol: ISO 14443-A Speed (kbit/s): 424</summary>
        Iso14443A_Active_424 = 0x9B,

        /// <summary>Protocol: GTM Speed (kbit/s): ALL</summary>
        GTM_ALL = 0x9C,

        /// <summary>No cahnge</summary>
        NoChange = 0xFF,
    }
}

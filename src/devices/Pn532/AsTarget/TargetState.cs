// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Pn532.AsTarget
{
    /// <summary>
    /// Status state for the PN532 when setup as a target.
    /// </summary>
    public enum TargetState
    {
        /// <summary>
        /// TG_IDLE / TG_RELEASED: the PN532 (acting as NFCIP-1 target) waits for an initiator or has been released by its initiator.
        /// </summary>
        NfcipReleased = 0x00,

        /// <summary>
        /// TG_ACTIVATED: the PN532 is activated as NFCIP-1 target.
        /// </summary>
        NfcipActivated = 0x01,

        /// <summary>
        /// TG_DESELECTED: the PN532 (acting as NFCIP-1 target) has been de-selected by its initiator.
        /// </summary>
        NfcipDeselected = 0x02,

        /// <summary>
        /// PICC_RELEASED: the PN532 (acting as ISO/IEC14443-4 PICC) has been released by its PCD (no more RF field is detected).
        /// </summary>
        PiccReleased = 0x80,

        /// <summary>
        ///  PICC_ACTIVATED: the PN532 is activated as ISO/IEC14443-4 PICC.
        /// </summary>
        PiccActivated = 0x81,

        /// <summary>
        /// PICC_DESELECTED: the PN532 (acting as ISO/IEC14443-4 PICC) has been de-selected by its PDC
        /// </summary>
        PiccDeselected = 0x82,
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Pn532
{
    /// <summary>
    /// Security Access Module parameters
    /// </summary>
    [Flags]
    public enum ParametersFlags
    {
        // bit 0: fNADUsed Î Use of the NAD information in case of initiator
        // configuration (DEP and ISO/IEC14443-4
        // PCD).
        fNADUsed = 0b0000_0001,
        // bit 1: fDIDUsed Î Use of the DID information in case of initiator
        // configuration (or CID in case of
        // ISO/IEC14443-4
        // PCD configuration).
        fDIDUsed = 0b0000_0010,
        // bit 2: fAutomaticATR_RES Î Automatic generation of the ATR_RES in
        // case of target configuration.
        fAutomaticATR_RES = 0b0000_0100,
        // bit 3: RFU Î Must be set to 0.
        // bit 4: fAutomaticRATS Î Automatic generation of the RATS in case of
        // ISO/IEC14443-4 PCD mode.
        fAutomaticRATS = 0b0001_0000,
        // bit 5: fISO14443-4_PICC Î The emulation of a ISO/IEC14443-4 PICC is
        // enabled.
        fISO14443_4_PICC = 0b0010_0000,
        // bit 6: fRemovePrePostAmble Î The PN532 does not send Preamble and
        // Postamble.
        fRemovePrePostAmble = 0b0100_0000,
        // bit 7: RFU Î Must be set to 0. 
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// Terminal Transaction Qualifier, capabilities and requirement
    /// for the terminal to interact with the card.
    /// This is used when asking for the Processing Data containing the 
    /// details of the files to read to get card information
    /// </summary>
    [Flags]
    public enum TerminalTransactionQualifier : uint
    {
        // byte 1 byte 2 byte 3 byte 4
        // byte 1
        // bit 8: 1 = Mag-stripe mode supported 
        MagStripeSupported = 0b1000_0000_0000_0000_0000_0000_0000_0000,
        // bit 7: RFU (0)         
        // bit 6: 1 = EMV mode supported 
        EmvModeSuypported = 0b0010_0000_0000_0000_0000_0000_0000_0000,
        // bit 5: 1 = EMV contact chip supported 
        EmvContactChipSupported = 0b0001_0000_0000_0000_0000_0000_0000_0000,
        // bit 4: 1 = Offline-only reader
        OfflineOnlyReader = 0b0000_1000_0000_0000_0000_0000_0000_0000,
        // bit 3: 1 = Online PIN supported
        OnlinePinSupported = 0b0000_0100_0000_0000_0000_0000_0000_0000,
        // bit 2: 1 = Signature supported 
        SignatureSupported = 0b0000_0010_0000_0000_0000_0000_0000_0000,
        // bit 1: 1 = Offline Data Authentication for Online Authorizations supported. 
        OfflineDataAuthentication = 0b0000_0001_0000_0000_0000_0000_0000_0000,
        // Note: The TTQ 'Mag-stripe mode supported' bit is set to 0b for products using this specification.
        // bit 8: 1 = Online cryptogram required 
        OnlineCryptogramRequired = 0b0000_0000_1000_0000_0000_0000_0000_0000,
        // Note: A qVSDC online-only reader must have TTQ 
        // byte 2 
        // bit 8 set to 1b. It may be coded to 1b or set as a result of device configuration parameters. 
        // bit 7: 1 = CVM required 
        CvmRequired = 0b0000_0000_0100_0000_0000_0000_0000_0000,
        // bit 6: 1 = (Contact Chip) Offline PIN supported 
        ConstactChipOfflinePinSupported = 0b0000_0000_0010_0000_0000_0000_0000_0000,
        // bits 5-1: RFU (0,0,0,0,0)
        // Byte 3 
        // bit 8: 1 = Issuer Update Processing supported 
        IssuerUpdateProcessingSupported = 0b0000_0000_0000_0000_1000_0000_0000_0000,
        // bit 7: 1 = Consumer Device CVM supported 
        ConsumerDeviceCvmSupported = 0b0000_0000_0000_0000_0100_0000_0000_0000,
        // bits 6-1: RFU (0,0,0,0,0,0) 
        // Byte 4 RFU (0,0,0,0,0,0,0,0)
    }
}

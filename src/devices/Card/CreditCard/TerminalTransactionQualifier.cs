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
        // Signification of the bits:
        // byte 1 byte 2 byte 3 byte 4
        //
        // byte 1
        // bit 8: 1 = Mag-stripe mode supported 
        // bit 7: RFU (0)         
        // bit 6: 1 = EMV mode supported 
        // bit 5: 1 = EMV contact chip supported 
        // bit 4: 1 = Offline-only reader
        // bit 3: 1 = Online PIN supported
        // bit 2: 1 = Signature supported 
        // bit 1: 1 = Offline Data Authentication for Online Authorizations supported. 
        // Note: The TTQ 'Mag-stripe mode supported' bit is set to 0b for products using this specification.
        // bit 8: 1 = Online cryptogram required 
        // Note: A qVSDC online-only reader must have TTQ 
        //
        // byte 2 
        // bit 8 set to 1b. It may be coded to 1b or set as a result of device configuration parameters. 
        // bit 7: 1 = CVM required 
        // bit 6: 1 = (Contact Chip) Offline PIN supported 
        // bits 5-1: RFU (0,0,0,0,0)
        //
        // Byte 3 
        // bit 8: 1 = Issuer Update Processing supported 
        // bit 7: 1 = Consumer Device CVM supported 
        // bits 6-1: RFU (0,0,0,0,0,0)
        //
        // Byte 4 RFU (0,0,0,0,0,0,0,0)

        /// <summary>
        /// Magnetic Stripe Supported
        /// </summary>
        MagStripeSupported = 0b1000_0000_0000_0000_0000_0000_0000_0000,
        /// <summary>
        /// Emv Mode Supported
        /// </summary>
        EmvModeSupported = 0b0010_0000_0000_0000_0000_0000_0000_0000,
        /// <summary>
        /// Emv Contact Chip Supported
        /// </summary>
        EmvContactChipSupported = 0b0001_0000_0000_0000_0000_0000_0000_0000,
        /// <summary>
        /// Offline Only Reader
        /// </summary>
        OfflineOnlyReader = 0b0000_1000_0000_0000_0000_0000_0000_0000,
        /// <summary>
        /// Online Pin Supported
        /// </summary>
        OnlinePinSupported = 0b0000_0100_0000_0000_0000_0000_0000_0000,
        /// <summary>
        /// Signature Supported
        /// </summary>
        SignatureSupported = 0b0000_0010_0000_0000_0000_0000_0000_0000,
        /// <summary>
        /// Offline Data Authentication
        /// </summary>
        OfflineDataAuthentication = 0b0000_0001_0000_0000_0000_0000_0000_0000,
        /// <summary>
        /// Online Cryptogram Required
        /// </summary>
        OnlineCryptogramRequired = 0b0000_0000_1000_0000_0000_0000_0000_0000,
        /// <summary>
        /// Cvm Required
        /// </summary>
        CvmRequired = 0b0000_0000_0100_0000_0000_0000_0000_0000,
        /// <summary>
        /// Contact Chip Offline Pin Supported
        /// </summary>
        ContactChipOfflinePinSupported = 0b0000_0000_0010_0000_0000_0000_0000_0000,
        /// <summary>
        /// Issuer Update Processing Supported
        /// </summary>
        IssuerUpdateProcessingSupported = 0b0000_0000_0000_0000_1000_0000_0000_0000,
        /// <summary>
        /// Consumer Device Cvm Supported
        /// </summary>
        ConsumerDeviceCvmSupported = 0b0000_0000_0000_0000_0100_0000_0000_0000,
    }
}

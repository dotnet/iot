// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Ultralight
{
    /// <summary>
    /// List of commands available for the Mifare cards
    /// </summary>
    public enum UltralightCommand
    {
        /// <summary>
        /// Get the NTAG version
        /// </summary>
        GetVersion = 0x60,

        /// <summary>
        /// Read 16 Bytes
        /// </summary>
        Read16Bytes = 0x30,

        /// <summary>
        /// Read multiple pages at once
        /// </summary>
        ReadFast = 0x3A,

        /// <summary>
        /// Write 16 Bytes but only last significant 4 bytes are written
        /// </summary>
        WriteCompatible = 0xA0,

        /// <summary>
        /// Write 4 Bytes
        /// </summary>
        Write4Bytes = 0xA2,

        /// <summary>
        /// Read the current value of the NFC one way counter
        /// </summary>
        ReadCounter = 0x39,

        /// <summary>
        /// Increase he 24 bit counter
        /// </summary>
        IncreaseCounter = 0xA5,

        /// <summary>
        /// Password authentication with 4 bytes
        /// </summary>
        PasswordAuthentication = 0x1B,

        /// <summary>
        /// For Ultralight C 3DS authentication
        /// </summary>
        ThreeDsAuthenticationPart1 = 0x1A,

        /// <summary>
        /// For Ultralight C 3DS authentication
        /// </summary>
        ThreeDsAuthenticationPart2 = 0xAF,

        /// <summary>
        /// Read the ECC specific 32 byte signature
        /// </summary>
        ReadSignature = 0x3C,
    }
}

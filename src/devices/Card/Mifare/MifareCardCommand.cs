// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// List of commands available for the Mifare cards
    /// </summary>
    public enum MifareCardCommand
    {
        /// <summary>
        /// Authentication A for Key A
        /// </summary>
        AuthenticationA = 0x60,

        /// <summary>
        /// Authentication B for Key B
        /// </summary>
        AuthenticationB = 0x61,

        /// <summary>
        /// Read 16 Bytes
        /// </summary>
        Read16Bytes = 0x30,

        /// <summary>
        /// Write 16 Bytes
        /// </summary>
        Write16Bytes = 0xA0,

        /// <summary>
        /// Write 4 Bytes
        /// </summary>
        Write4Bytes = 0xA2,

        /// <summary>
        /// Incrementation
        /// </summary>
        Incrementation = 0xC1,

        /// <summary>
        /// Decrementation
        /// </summary>
        Decrementation = 0xC0,

        /// <summary>
        /// Transfer
        /// </summary>
        Transfer = 0xB0,

        /// <summary>
        /// Restore
        /// </summary>
        Restore = 0xC2
    }
}

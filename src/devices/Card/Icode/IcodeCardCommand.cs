// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Icode
{
    /// <summary>
    /// List of commands available for the Icode cards
    /// </summary>
    internal enum IcodeCardCommand
    {
        /// <summary>
        /// Inventory cards
        /// </summary>
        Inventory = 0x01,

        /// <summary>
        /// Set VICC quiet state
        /// </summary>
        StayQuiet = 0x02,

        /// <summary>
        /// Read single block
        /// </summary>
        ReadSingleBlock = 0x20,

        /// <summary>
        /// Write single block
        /// </summary>
        WriteSingleBlock = 0x21,

        /// <summary>
        /// Lock block
        /// </summary>
        LockBlock = 0x22,

        /// <summary>
        /// Read multiple blocks
        /// </summary>
        ReadMultipleBlocks = 0x23,

        /// <summary>
        /// Write multiple block
        /// </summary>
        WriteMultipleBlocks = 0x24,

        /// <summary>
        /// Select
        /// </summary>
        Select = 0x25,

        /// <summary>
        /// Reset to read
        /// </summary>
        ResettoRead = 0x26,

        /// <summary>
        /// Write AFI
        /// </summary>
        WriteAfi = 0x27,

        /// <summary>
        /// Lock AFI
        /// </summary>
        LockAfi = 0x28,

        /// <summary>
        /// Write DSFID
        /// </summary>
        WriteDsfid = 0x29,

        /// <summary>
        /// Lock DSFID
        /// </summary>
        LockDsfid = 0x2A,

        /// <summary>
        /// Get system information
        /// </summary>
        GetSystemInformation = 0x2B,

        /// <summary>
        /// Get multiple block security status
        /// </summary>
        GetMultipleBlockSecurityStatus = 0x2C
    }
}

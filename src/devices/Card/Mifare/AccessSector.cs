// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// All access sectors rights for the keys and the access bits
    /// </summary>
    [Flags]
    public enum AccessSector
    {
        /// <summary>
        /// No access
        /// </summary>
        None = 0b0000_0000,
        /// <summary>
        /// Write Key A With Key A
        /// </summary>
        WriteKeyAWithKeyA = 0b0000_0001,
        /// <summary>
        /// Write Key A With Key B
        /// </summary>
        WriteKeyAWithKeyB = 0b0000_0010,
        /// <summary>
        /// Read Access Bits With Key A
        /// </summary>
        ReadAccessBitsWithKeyA = 0b0000_0100,
        /// <summary>
        /// Read Access Bits With Key B
        /// </summary>
        ReadAccessBitsWithKeyB = 0b0000_1000,
        /// <summary>
        /// Write Key B With Key A
        /// </summary>
        WriteKeyBWithKeyA = 0b0001_0000,
        /// <summary>
        /// Write Key B With Key B
        /// </summary>
        WriteKeyBWithKeyB = 0b0010_0000,
        /// <summary>
        /// Write Access Bits With Key A
        /// </summary>
        WriteAccessBitsWithKeyA = 0b0100_0000,
        /// <summary>
        /// Write Access Bits With Key B
        /// </summary>
        WriteAccessBitsWithKeyB = 0b1000_0000,
        /// <summary>
        /// Sometimes the KeyB may be read
        /// </summary>
        ReadKeyB = 0b0001_000_0000,
        /// <summary>
        /// Read Key B With Key A
        /// </summary>
        ReadKeyBWithKeyA = 0b0010_0000_0000,
    }
}
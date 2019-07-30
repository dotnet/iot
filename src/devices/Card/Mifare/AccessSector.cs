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
        None = 0b0000_0000,
        WriteKeyAWithKeyA = 0b0000_0001,
        WriteKeyAWithKeyB = 0b0000_0010,
        ReadAccessBitsWithKeyA = 0b0000_0100,
        ReadAccessBitsWithKeyB = 0b0000_1000,
        WriteKeyBWithKeyA = 0b0001_0000,
        WriteKeyBWithKeyB = 0b0010_0000,
        WriteAccessBitsWithKeyA = 0b0100_0000,
        WriteAccessBitsWithKeyB = 0b1000_0000,
        // Sometimes the KeyB may be read
        ReadKeyB = 0b0001_000_0000,
        ReadKeyBWithKeyA = 0b0010_0000_0000,
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// List of commands available for the Mifare cards
    /// </summary>
    public enum MifareCardCommand
    {
        AuthenticationA = 0x60,
        AuthenticationB = 0x61,
        Read16Bytes = 0x30,
        Write16Bytes = 0xA0,
        Write4Bytes = 0xA2,
        Incrementation = 0xC1,
        Decrementation = 0xC0,
        Transfer = 0xB0,
        Restore = 0xC2
    }
}

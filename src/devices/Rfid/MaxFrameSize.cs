// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Rfid
{
    /// <summary>
    /// Bit rate supported by 144443-4B
    /// http://ww1.microchip.com/downloads/en/AppNotes/doc2056.pdf
    /// page 20
    /// </summary>
    public enum MaxFrameSize
    {
        B016 = 0b0000_0000,
        B024 = 0b0001_0000,
        B032 = 0b0010_0000,
        B040 = 0b0011_0000,
        B048 = 0b0100_0000,
        B064 = 0b0101_0000,
        B096 = 0b0110_0000,
        B128 = 0b0111_0000,
        B256 = 0b1000_0000,
    }
}
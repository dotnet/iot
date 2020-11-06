// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rfid
{
    /// <summary>
    /// Bit rate supported by 144443-4B
    /// http://ww1.microchip.com/downloads/en/AppNotes/doc2056.pdf
    /// page 20
    /// </summary>
    public enum MaxFrameSize
    {
        /// <summary>
        /// 16 bytes
        /// </summary>
        B016 = 0b0000_0000,

        /// <summary>
        /// 24 bytes
        /// </summary>
        B024 = 0b0001_0000,

        /// <summary>
        /// 32 bytes
        /// </summary>
        B032 = 0b0010_0000,

        /// <summary>
        /// 40 bytes
        /// </summary>
        B040 = 0b0011_0000,

        /// <summary>
        /// 48 bytes
        /// </summary>
        B048 = 0b0100_0000,

        /// <summary>
        /// 64 bytes
        /// </summary>
        B064 = 0b0101_0000,

        /// <summary>
        /// 96 bytes
        /// </summary>
        B096 = 0b0110_0000,

        /// <summary>
        /// 128 bytes
        /// </summary>
        B128 = 0b0111_0000,

        /// <summary>
        /// 256 bytes
        /// </summary>
        B256 = 0b1000_0000,
    }
}
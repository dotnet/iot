// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mfrc522
{
    /// <summary>
    /// The reception gain for the antenna
    /// </summary>
    public enum Gain
    {
        /// <summary>
        /// Minimum gain 18 db
        /// </summary>
        G18dBa = 0b000_0000,

        /// <summary>
        /// 23 db
        /// </summary>
        G23dBa = 0b0001_0000,

        /// <summary>
        /// 18 db
        /// </summary>
        G18dBb = 0b0010_0000,

        /// <summary>
        /// 23 db
        /// </summary>
        G23dBb = 0b0011_0000,

        /// <summary>
        /// 33 db
        /// </summary>
        G33dB = 0b0100_0000,

        /// <summary>
        /// 38 db
        /// </summary>
        G38dB = 0b0101_0000,

        /// <summary>
        /// 43 db
        /// </summary>
        G43dB = 0b0110_0000,

        /// <summary>
        /// 48 db
        /// </summary>
        G48dB = 0b0111_0000,
    }
}

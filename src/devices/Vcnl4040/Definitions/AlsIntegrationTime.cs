// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of ALS integration times.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum AlsIntegrationTime : byte
    {
        /// <summary>
        /// Integration time is 80 ms.
        /// </summary>
        Time80ms = 0b0000_0000,

        /// <summary>
        /// Integration time is 160 ms.
        /// </summary>
        Time160ms = 0b0100_0000,

        /// <summary>
        /// Integration time is 320 ms.
        /// </summary>
        Time320ms = 0b1000_0000,

        /// <summary>
        /// Integration time is 640 ms.
        /// </summary>
        Time640ms = 0b1100_0000
    }
}

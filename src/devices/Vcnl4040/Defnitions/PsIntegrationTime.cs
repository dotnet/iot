// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Defnitions
{
    /// <summary>
    /// Defines the set of PS integration time settings.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsIntegrationTime : byte
    {
        /// <summary>
        /// Integration time 1.0
        /// </summary>
        Time1_0 = 0b0000_0000,

        /// <summary>
        /// Integration time 1.5
        /// </summary>
        Time1_5 = 0b0000_0010,

        /// <summary>
        /// Integration time 2.0
        /// </summary>
        Time2_0 = 0b0000_0100,

        /// <summary>
        /// Integration time 2.5
        /// </summary>
        Time2_5 = 0b0000_0110,

        /// <summary>
        /// Integration time 3.0
        /// </summary>
        Time3_0 = 0b0000_1000,

        /// <summary>
        /// Integration time 3.5
        /// </summary>
        Time3_5 = 0b0000_1010,

        /// <summary>
        /// Integration time 4.0
        /// </summary>
        Time4_0 = 0b0000_1100,

        /// <summary>
        /// Integration time 8.0
        /// </summary>
        Time8_0 = 0b0000_1110
    }
}

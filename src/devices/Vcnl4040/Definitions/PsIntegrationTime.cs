// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of PS integration time settings.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsIntegrationTime : byte
    {
        /// <summary>
        /// Integration time T1.0
        /// Emitter signal pulse length is approx. 100 us.
        /// </summary>
        Time1_0 = 0b0000_0000,

        /// <summary>
        /// Integration time T1.5
        /// Emitter signal pulse length is approx. 150 us.
        /// </summary>
        Time1_5 = 0b0000_0010,

        /// <summary>
        /// Integration time T2.0
        /// Emitter signal pulse length is approx. 200 us.
        /// </summary>
        Time2_0 = 0b0000_0100,

        /// <summary>
        /// Integration time T2.5
        /// Emitter signal pulse length is approx. 250 us.
        /// </summary>
        Time2_5 = 0b0000_0110,

        /// <summary>
        /// Integration time T3.0
        /// Emitter signal pulse length is approx. 300 us.
        /// </summary>
        Time3_0 = 0b0000_1000,

        /// <summary>
        /// Integration time T3.5
        /// Emitter signal pulse length is approx. 350 us.
        /// </summary>
        Time3_5 = 0b0000_1010,

        /// <summary>
        /// Integration time T4.0
        /// Emitter signal pulse length is approx. 400 us.
        /// </summary>
        Time4_0 = 0b0000_1100,

        /// <summary>
        /// Integration time T8.0
        /// Emitter signal pulse length is approx. 800 us.
        /// </summary>
        Time8_0 = 0b0000_1110
    }
}

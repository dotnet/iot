// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of proximity multi pulse numbers
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsMultiPulse : byte
    {
        /// <summary>
        /// 1 multi pulse
        /// </summary>
        Pulse1 = 0b0000_0000,

        /// <summary>
        /// 2 multi pulse
        /// </summary>
        Pulse2 = 0b0010_0000,

        /// <summary>
        /// 4 multi pulses
        /// </summary>
        Pulse4 = 0b0100_0000,

        /// <summary>
        /// 8 multi pulses
        /// </summary>
        Pulse8 = 0b0110_0000
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of PS interrupt settings.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal enum PsInterruptMode : byte
    {
        /// <summary>
        /// Interrupts disabled
        /// </summary>
        Disabled = 0b0000_0000,

        /// <summary>
        /// Interrupt triggers when close proximity is detected
        /// </summary>
        Close = 0b0000_0001,

        /// <summary>
        /// Interrupt triggers when long distance is detected
        /// </summary>
        Away = 0b0000_0010,

        /// <summary>
        /// Interrupt triggers when either close proximity or
        /// long distance is detected
        /// </summary>
        CloseOrAway = 0b0000_0011
    }
}

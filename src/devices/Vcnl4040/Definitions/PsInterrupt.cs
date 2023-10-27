// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of PS interrupt settings.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsInterrupt : byte
    {
        /// <summary>
        /// Interrupt disable
        /// </summary>
        Disable = 0b0000_0000,

        /// <summary>
        /// Trigger when close
        /// </summary>
        Close = 0b0000_0001,

        /// <summary>
        /// Trigger when away
        /// </summary>
        Away = 0b0000_0010,

        /// <summary>
        /// Trigger when close or away
        /// </summary>
        CloseOrAway = 0b0000_0011
    }
}

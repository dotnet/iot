// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Common.Defnitions
{
    /// <summary>
    /// Defines the set of PS interrupt settings.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsInterruptMode : byte
    {
        /// <summary>
        /// Interrupt disable
        /// </summary>
        Disabled = 0b0000_0000,

        /// <summary>
        /// Trigger when close
        /// MEHR
        /// </summary>
        Close = 0b0000_0001,

        /// <summary>
        /// Trigger when away
        /// MEHR
        /// </summary>
        Away = 0b0000_0010,

        /// <summary>
        /// Trigger when close or away
        /// MEHR
        /// </summary>
        CloseOrAway = 0b0000_0011
    }
}

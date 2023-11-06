// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Common.Defnitions
{
    /// <summary>
    /// Defines the set of PS detection logic output modes.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsDetectionLogicOutputMode : byte
    {
        /// <summary>
        /// Proximity normal operation with interrupt function
        /// </summary>
        Interrupt = 0b0000_0000,

        /// <summary>
        /// Proximity detection logic output mode enable
        /// </summary>
        LogicOutput = 0b0100_1000
    }
}

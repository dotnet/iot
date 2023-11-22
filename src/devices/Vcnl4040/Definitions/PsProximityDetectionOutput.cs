// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of PS proximity detection output modes.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal enum PsProximityDetectionOutput : byte
    {
        /// <summary>
        /// Proximity detection normal operation with interrupt function
        /// </summary>
        Interrupt = 0b0000_0000,

        /// <summary>
        /// Proximity detection logic output mode
        /// </summary>
        LogicOutput = 0b0100_0000
    }
}

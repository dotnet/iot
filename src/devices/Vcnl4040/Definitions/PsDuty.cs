// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of PS duty ratio settings.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsDuty : byte
    {
        /// <summary>
        /// Duty ratio is 1/40
        /// </summary>
        Duty40 = 0b0000_0000,

        /// <summary>
        /// Duty ratio is 1/80
        /// </summary>
        Duty80 = 0b0100_0000,

        /// <summary>
        /// Duty ratio is 1/160
        /// </summary>
        Duty160 = 0b1000_0000,

        /// <summary>
        /// Duty ratio is 1/320
        /// </summary>
        Duty320 = 0b1100_0000
    }
}

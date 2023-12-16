// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of ALS and SD power states.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal enum PowerState : byte
    {
        /// <summary>
        /// ALS/SD power on
        /// </summary>
        PowerOn = 0b0000_0000,

        /// <summary>
        /// ALS/SD shutdown / power off
        /// </summary>
        PowerOff = 0b0000_0001
    }
}

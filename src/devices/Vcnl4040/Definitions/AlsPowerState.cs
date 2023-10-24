// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vncl4040.Definitions
{
    /// <summary>
    /// Defines the set of ALS power states.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum AlsPowerState : byte
    {
        /// <summary>
        /// ALS power on
        /// </summary>
        PowerOn = 0b0,

        /// <summary>
        /// ALS shutdown / power off
        /// </summary>
        Shutdown = 0b1
    }
}

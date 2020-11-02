// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mhz19b
{
    /// <summary>
    /// Defines if automatic baseline correction (ABM) is on or off
    /// For details refer to datasheet, rev. 1.0, pg. 8
    /// </summary>
    public enum AbmState
    {
        /// <summary>
        /// ABM off (value acc. to datasheet)
        /// </summary>
        Off = 0x00,

        /// <summary>
        /// ABM on (value acc. to datasheet)
        /// </summary>
        On = 0xA0
    }
}

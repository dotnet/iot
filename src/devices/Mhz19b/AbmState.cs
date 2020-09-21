// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mhz19b
{
    /// <summary>
    /// Defines if automatic baseline correction is on or off
    /// </summary>
    public enum AbmState
    {
        /// <summary>
        /// ABM off
        /// </summary>
        Off = 0x00,

        /// <summary>
        /// ABM on
        /// </summary>
        On = 0x0a
    }
}

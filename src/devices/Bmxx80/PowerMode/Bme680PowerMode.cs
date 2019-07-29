// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmxx80.PowerMode
{
    /// <summary>
    /// Sensor power mode.
    /// </summary>
    /// <remarks>
    /// Section 3.1 in the datasheet.
    /// </remarks>
    public enum Bme680PowerMode : byte
    {
        /// <summary>
        /// No measurements are performed.
        /// </summary>
        /// <remarks>
        /// Minimal power consumption.
        /// </remarks>
        Sleep = 0b00,

        /// <summary>
        /// Single TPHG cycle is performed.
        /// </summary>
        /// <remarks>
        /// Sensor automatically returns to sleep mode afterwards.
        /// Gas sensor heater only operates during gas measurement.
        /// </remarks>
        Forced = 0b01
    }
}

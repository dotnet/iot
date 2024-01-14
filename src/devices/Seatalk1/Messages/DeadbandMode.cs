// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Deadband mode setting.
    /// This value configures whether the AP should try to filter boat movements from waves as good as possible, and save power by
    /// minimizing the amount of rudder commands.
    /// If it is set to minimal, the AP tries to minimize off-track limits
    /// </summary>
    public enum DeadbandMode
    {
        /// <summary>
        /// Value not available or unknown
        /// </summary>
        None = 0,

        /// <summary>
        /// Automatic deadband mode (default)
        /// </summary>
        Automatic,

        /// <summary>
        /// Minimize off-track limits, possibly increasing the number of rudder commands.
        /// </summary>
        Minimal,
    }
}

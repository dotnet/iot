// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.Tsl256x
{
    /// <summary>
    /// The channel to get the data from
    /// </summary>
    public enum Channel
    {
        /// <summary>
        /// Channel 0 is Visible and Infrared
        /// </summary>
        VisibleInfrared = 0,

        /// <summary>
        /// Channel 1 is infrared
        /// </summary>
        Infrared,

        /// <summary>
        /// Channel 0 - Channel 1 is Visible only
        /// </summary>
        Visible
    }
}

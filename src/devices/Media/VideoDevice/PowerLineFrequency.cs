// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Media
{
    /// <summary>
    /// The power line frequency of a video device.
    /// </summary>
    public enum PowerLineFrequency : int
    {
        /// <summary>
        /// Disabled
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// 50Hz
        /// </summary>
        Frequency50Hz = 1,

        /// <summary>
        /// 60Hz
        /// </summary>
        Frequency60Hz = 2,

        /// <summary>
        /// Auto
        /// </summary>
        Auto = 3,
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Media
{
    /// <summary>
    /// The white balance effect of a video device.
    /// </summary>
    public enum WhiteBalanceEffect : int
    {
        /// <summary>
        /// Manual
        /// </summary>
        Manual = 0,

        /// <summary>
        /// Auto
        /// </summary>
        Auto = 1,

        /// <summary>
        /// Incandescent
        /// </summary>
        Incandescent = 2,

        /// <summary>
        /// Fluorescent
        /// </summary>
        Fluorescent = 3,

        /// <summary>
        /// FluorescentH
        /// </summary>
        FluorescentH = 4,

        /// <summary>
        /// Horizon
        /// </summary>
        Horizon = 5,

        /// <summary>
        /// Daylight
        /// </summary>
        Daylight = 6,

        /// <summary>
        /// Flash
        /// </summary>
        Flash = 7,

        /// <summary>
        /// Cloudy
        /// </summary>
        Cloudy = 8,

        /// <summary>
        /// Shade
        /// </summary>
        Shade = 9,
    }
}

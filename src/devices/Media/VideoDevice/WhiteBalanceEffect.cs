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
        Manual = 0,
        Auto = 1,
        Incandescent = 2,
        Fluorescent = 3,
        FluorescentH = 4,
        Horizon = 5,
        Daylight = 6,
        Flash = 7,
        Cloudy = 8,
        Shade = 9,
    }
}

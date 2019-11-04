// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Imu
{
    internal enum ClockSource
    {
        /// <summary>
        /// Internal 20MHz
        /// </summary>
        Internal20MHz = 0,
        /// <summary>
        /// Auto Select
        /// </summary>
        AutoSelect = 1,
        /// <summary>
        /// Stop Clock
        /// </summary>
        StopClock = 7,
    }
}

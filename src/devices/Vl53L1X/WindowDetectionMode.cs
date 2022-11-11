// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vl53L1X
{
    /// <summary>
    /// The window detection mode.
    /// </summary>
    public enum WindowDetectionMode : byte
    {
        /// <summary>
        /// Object under a certain distance.
        /// </summary>
        Below = 0,

        /// <summary>
        /// Object beyond a certain distance.
        /// </summary>
        Above = 1,

        /// <summary>
        /// Object out of a window limited by a near and far threshold.
        /// </summary>
        Out = 2,

        /// <summary>
        /// Object within a window limited by a near and far threshold.
        /// </summary>
        In = 3
    }
}

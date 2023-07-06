// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd1331
{
    /// <summary>
    /// Frame Interval for scrolling
    /// </summary>
    public enum FrameInterval
    {
        /// <summary>
        /// 6 frames per step
        /// </summary>
        Frames6 = 0,

        /// <summary>
        /// 10 frames per step
        /// </summary>
        Framesd10 = 1,

        /// <summary>
        /// 100 frames per step
        /// </summary>
        Frames100 = 2,

        /// <summary>
        /// 200 frames per step
        /// </summary>
        Frames200 = 3
    }
}

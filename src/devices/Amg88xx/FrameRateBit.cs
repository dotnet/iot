// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Defines the bit of the frame rate register (addr: 0x02)
    /// </summary>
    public enum FrameRateBit : byte
    {
        /// <summary>
        /// Frame rate mode bit (not set: 10fps, set: 1fps)
        /// </summary>
        FPS = 1
    }
}

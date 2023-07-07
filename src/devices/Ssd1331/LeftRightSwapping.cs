// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd1331
{
    /// <summary>
    /// COM Left / Right Remap
    /// </summary>
    public enum LeftRightSwapping
    {
        /// <summary>
        /// Disable swapping. COM0 on left side
        /// </summary>
        Disable = 0,

        /// <summary>
        /// Enable swapping. COM0 on right side
        /// </summary>
        Enable = 1
    }
}

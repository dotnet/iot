// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vl53L1X
{
    /// <summary>
    /// The distance mode of the device
    /// </summary>
    public enum Precision : byte
    {
        /// <summary>
        /// Short distance. Maximum distance is limited to 1.3m but results in a better ambient immunity.
        /// </summary>
        Short = 1,

        /// <summary>
        /// Long distance. Can range up to 4 m in the dark with a timing budget of200 ms.
        /// </summary>
        Long = 2,

        /// <summary>
        /// Unknown distance mode or the distance mode is not configured.
        /// </summary>
        Unknown = 3
    }
}

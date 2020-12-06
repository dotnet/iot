// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.Tsl256x
{
    /// <summary>
    /// Gain for integration
    /// </summary>
    public enum Gain
    {
        /// <summary>
        /// Norma gain x1
        /// </summary>
        Normal = 0b0000_0000,

        /// <summary>
        /// High gain x16
        /// </summary>
        High = 0b0001_0000,
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Tcs3472x
{
    /// <summary>
    /// The gain used to integrate the colors
    /// </summary>
    public enum Gain
    {
        /// <summary>1x gain</summary>
        Gain01X = 0x00,

        /// <summary>4x gain</summary>
        Gain04X = 0x01,

        /// <summary>16x gain</summary>
        Gain16X = 0x02,

        /// <summary>60x gain</summary>
        Gain60X = 0x03,
    }
}

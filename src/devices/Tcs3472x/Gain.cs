// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Tcs3472x
{
    /// <summary>
    /// The gain used to integrate the colors
    /// </summary>
    public enum Gain
    {
        //  1x gain
        Gain01X = 0x00,
        //  4x gain
        Gain04X = 0x01,
        // 16x gain
        Gain16X = 0x02,
        // 60x gain
        Gain60X = 0x03,
    }
}

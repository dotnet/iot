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
        GAIN_1X = 0x00,
        //  4x gain
        GAIN_4X = 0x01,
        // 16x gain
        GAIN_16X = 0x02,
        // 60x gain
        GAIN_60X = 0x03,
    }
}

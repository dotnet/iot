// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pn532.AsTarget
{
    /// <summary>
    /// When PN532 is acting as a target, the baud rate
    /// it is engaged to
    /// </summary>
    public enum TargetBaudRateInialized
    {
        /// <summary>
        /// 106k bps
        /// </summary>
        B106kbps = 0b0000_0000,

        /// <summary>
        /// 212k bps
        /// </summary>
        B212kbps = 0b0001_0000,

        /// <summary>
        /// 424k bps
        /// </summary>
        B424kbps = 0b0010_0000,
    }
}

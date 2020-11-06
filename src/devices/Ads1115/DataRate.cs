// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Control the Data Rate (SPS, sample per second)
    /// </summary>
    public enum DataRate
    {
        /// <summary>
        /// 8 SPS
        /// </summary>
        SPS008 = 0x00,

        /// <summary>
        /// 16 SPS
        /// </summary>
        SPS016 = 0x01,

        /// <summary>
        /// 32 SPS
        /// </summary>
        SPS032 = 0x02,

        /// <summary>
        /// 64 SPS
        /// </summary>
        SPS064 = 0x03,

        /// <summary>
        /// 128 SPS
        /// </summary>
        SPS128 = 0x04,

        /// <summary>
        /// 250 SPS
        /// </summary>
        SPS250 = 0x05,

        /// <summary>
        /// 475 SPS
        /// </summary>
        SPS475 = 0x06,

        /// <summary>
        /// 860 SPS
        /// </summary>
        SPS860 = 0x07
    }
}

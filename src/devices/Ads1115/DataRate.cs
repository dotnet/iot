// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ads1115
{
    /// <summary>
    ///  Control the Data Rate 
    /// </summary>
    public enum DataRate
    {
        SPS008 = 0x00,
        SPS016 = 0x01,
        SPS032 = 0x02,
        SPS064 = 0x03,
        SPS128 = 0x04,
        SPS250 = 0x05,
        SPS475 = 0x06,
        SPS860 = 0x07
    }
}
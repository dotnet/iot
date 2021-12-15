// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// The ADS frequency.
    /// </summary>
    public enum AdcFrequency
    {
        /// <summary>25 Hz frequency</summary>
        Frequency25Hz = 0b0000_0000,

        /// <summary>50 Hz frequency</summary>
        Frequency50Hz = 0b0100_0000,

        /// <summary>100 Hz frequency</summary>
        Frequency100Hz = 0b1000_0000,

        /// <summary>200 Hz frequency</summary>
        Frequency200Hz = 0b1100_0000,
    }
}

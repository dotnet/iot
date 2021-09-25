// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Shutdown battery timing.
    /// </summary>
    public enum ShutdownBatteryTiming
    {
        /// <summary>0.5 seconds</summary>
        S0_5 = 0b0000_0000,

        /// <summary>1 seconds</summary>
        S1 = 0b0000_0001,

        /// <summary>2 seconds</summary>
        S2 = 0b0000_0010,

        /// <summary>3 seconds</summary>
        S3 = 0b0000_0011,
    }
}

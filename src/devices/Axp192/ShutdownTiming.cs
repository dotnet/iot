// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Shutdown timing
    /// </summary>
    public enum ShutdownTiming
    {
        /// <summary>4 seconds</summary>
        S4 = 0b0000_0000,

        /// <summary>6 seconds</summary>
        S6 = 0b0000_0001,

        /// <summary>8 seconds</summary>
        S8 = 0b0000_0010,

        /// <summary>10 seconds</summary>
        S10 = 0b0000_0011,
    }
}

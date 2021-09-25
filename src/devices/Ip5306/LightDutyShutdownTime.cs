// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ip5306
{
    /// <summary>
    /// Light duty shutdown time
    /// </summary>
    public enum LightDutyShutdownTime
    {
        /// <summary>64 seconds</summary>
        S64 = 0b0000_1100,

        /// <summary>16 seconds</summary>
        S16 = 0b0000_1000,

        /// <summary>32 seconds</summary>
        S32 = 0b0000_0100,

        /// <summary>8 seconds</summary>
        S08 = 0b0000_0000,
    }
}

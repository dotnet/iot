// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Long press timing
    /// </summary>
    public enum LongPressTiming
    {
        /// <summary>1 second</summary>
        S1 = 0b0000_0000,

        /// <summary>1.5 seconds</summary>
        S1_5 = 0b0001_0000,

        /// <summary>2 seconds</summary>
        S2 = 0b0010_0000,

        /// <summary>2.5 seconds</summary>
        S2_5 = 0b0011_0000,
    }
}

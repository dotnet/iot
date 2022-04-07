// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Short press timing
    /// </summary>
    public enum ShortPressTiming
    {
        /// <summary>128 milliseconds</summary>
        Ms128 = 0b0000_0000,

        /// <summary>512 milliseconds</summary>
        Ms512 = 0b0100_0000,

        /// <summary>1 second</summary>
        S1 = 0b1000_0000,

        /// <summary>2 seconds</summary>
        S2 = 0b1100_0000,
    }
}

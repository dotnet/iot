// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// PWROK signal delay after power start-up
    /// </summary>
    public enum SignalDelayAfterPowerUp
    {
        /// <summary>32 milliseconds</summary>
        Ms32 = 0b0000_0000,

        /// <summary>64 milliseconds</summary>
        Ms64 = 0b0000_0100,
    }
}

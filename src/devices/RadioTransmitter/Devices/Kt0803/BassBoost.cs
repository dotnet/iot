// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.RadioTransmitter
{
    /// <summary>
    /// Bass Boost
    /// </summary>
    public enum BassBoost : byte
    {
        /// <summary>
        /// Disable
        /// </summary>
        BoostDisable = 0,

        /// <summary>
        /// 5 dB
        /// </summary>
        Boost05dB = 1,

        /// <summary>
        /// 11 dB
        /// </summary>
        Boost11dB = 2,

        /// <summary>
        /// 17 dB
        /// </summary>
        Boost17dB = 3
    }
}

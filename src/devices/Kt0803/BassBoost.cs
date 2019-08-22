// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Kt0803
{
    /// <summary>
    /// Bass Boost
    /// </summary>
    public enum BassBoost : byte
    {
        /// <summary>
        /// Disable
        /// </summary>
        Boost_Disable = 0,

        /// <summary>
        /// 5 dB
        /// </summary>
        Boost_5dB = 1,

        /// <summary>
        /// 11 dB
        /// </summary>
        Boost_11dB = 2,

        /// <summary>
        /// 17 dB
        /// </summary>
        Boost_17dB = 3
    }
}

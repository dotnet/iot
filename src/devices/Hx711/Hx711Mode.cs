// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hx711
{
    /// <summary>
    /// Hx711 has 3 modes of operation, choose the one based on the fisical connection with load cell.
    /// </summary>
    public enum Hx711Mode
    {
        /// <summary>
        /// Load cell link in channel A and use gain of 128, default mode
        /// </summary>
        ChannelAGain128,

        /// <summary>
        /// Load cell link in channel A and use gain of 64
        /// </summary>
        ChannelAGain64,

        /// <summary>
        /// Load cell link in channel B and use gain of 32
        /// </summary>
        ChannelBGain32
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Tca9548a
{
    /// <summary>
    /// Mux States
    /// </summary>
    public enum MuxState : byte
    {
        /// <summary>
        /// Disables all channels of MuX
        /// </summary>
        DisbleAllChannels = 0x00,

        /// <summary>
        /// Enables all channels of MuX
        /// </summary>
        EnableAllChannels = 0xFF,
    }

}

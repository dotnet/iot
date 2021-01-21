// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Defines the operating modes.
    /// The modes correspond to the setting in the power control register (addr: 0x00)
    /// </summary>
    public enum OperatingMode : byte
    {
        /// <summary>
        /// normal mode
        /// </summary>
        Normal = 0x00,

        /// <summary>
        /// sleep mode
        /// </summary>
        Sleep = 0x10,

        /// <summary>
        /// stand-by mode, 10s intermittence
        /// </summary>
        StandBy10Seconds = 0x21,

        /// <summary>
        /// stand-by mode, 60s intermittence
        /// </summary>
        StandBy60Seconds = 0x20,
    }
}

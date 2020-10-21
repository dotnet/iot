// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Defines the operating modes of the power control register (addr: 0x00)
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
        StandBy10 = 0x21,

        /// <summary>
        /// stand-by mode, 60s intermittence
        /// </summary>
        StandBy60 = 0x20,
    }
}

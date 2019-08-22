// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Kt0803
{
    /// <summary>
    /// PGA ( Programmable Gain Amplifier ) Gain
    /// </summary>
    public enum PgaGain : byte
    {
        /// <summary>
        /// 0 dB
        /// </summary>
        PGA_00dB = 4,

        /// <summary>
        /// 4 dB
        /// </summary>
        PGA_04dB = 5,

        /// <summary>
        /// 8 dB
        /// </summary>
        PGA_08dB = 6,

        /// <summary>
        /// 12 dB
        /// </summary>
        PGA_12dB = 7,

        /// <summary>
        /// -4 dB
        /// </summary>
        PGA_N04dB = 1,

        /// <summary>
        /// -8 dB
        /// </summary>
        PGA_N08dB = 2,

        /// <summary>
        /// -12 dB
        /// </summary>
        PGA_N12dB = 3,
    }
}

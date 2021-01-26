// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// Over sample Rate (OSR) registers are used to control bandwidth of an internal digital filter.
    /// Larger OSR valueleads to smaller filter bandwidth, less in-band noise and higher power consumption.It could be used to reach a
    /// good balance between noise and power. Four over sample ratio can be selected, 64, 128, 256 or 512.
    /// </summary>
    [Flags]
    public enum Oversampling : byte
    {
        /// <summary>
        /// Over sample rate of 512
        /// </summary>
        Rate512 = 0x00,

        /// <summary>
        /// Over sample rate of 256
        /// </summary>
        Rate256 = 0x40,

        /// <summary>
        /// Over sample rate of 128
        /// </summary>
        Rate128 = 0x80,

        /// <summary>
        /// Over sample rate of 64
        /// </summary>
        Rate64 = 0xC0
    }
}

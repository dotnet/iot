// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// Output data rate is controlled by ODR registers. Four data update frequencies can be selected: 10Hz, 50Hz,
    /// 100Hz and 200Hz.For most of compassing applications, we recommend 10 Hz for low power consumption.For
    /// gaming, the high update rate such as 100Hz or 200Hz can be used.
    /// </summary>
    [Flags]
    public enum OutputRate : byte
    {
        /// <summary>
        /// Output rate of 10 Hz
        /// </summary>
        Rate10Hz = 0x00,

        /// <summary>
        /// Output rate of 50 Hz
        /// </summary>
        Rate50Hz = 0x04,

        /// <summary>
        /// Output rate of 100 Hz
        /// </summary>
        Rate100Hz = 0x08,

        /// <summary>
        /// Output rate of 200 Hz
        /// </summary>
        Rate200Hz = 0x0C
    }
}

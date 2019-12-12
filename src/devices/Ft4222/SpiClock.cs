// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// Clock dividers of the system clock for the SPI module
    /// </summary>
    internal enum SpiClock
    {
        /// <summary>
        /// System Clock (ClockRate)
        /// </summary>
        DivideBy1 = 0,
        /// <summary>
        /// 1/2 System Clock (ClockRate)
        /// </summary>
        DivideBy2,
        /// <summary>
        ///  1/4 System Clock (ClockRate)
        /// </summary>
        DivideBy4,
        /// <summary>
        /// 1/8 System Clock (ClockRate)
        /// </summary>
        DivideBy8,
        /// <summary>
        /// 1/16 System Clock (ClockRate)
        /// </summary>
        DivideBy16,
        /// <summary>
        /// 1/32 System Clock (ClockRate)
        /// </summary>
        DivideBy32,
        /// <summary>
        /// 1/64 System Clock (ClockRate)
        /// </summary>
        DivideBy64,
        /// <summary>
        /// 1/128 System Clock (ClockRate)
        /// </summary>
        DivideBy128,
        /// <summary>
        /// 1/256 System Clock (ClockRate)
        /// </summary>
        DivideBy256,
        /// <summary>
        /// 1/512 System Clock (ClockRate)
        /// </summary>
        DivideBy512,
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Defines the status bits of the status clear register (addr: 0x05)
    /// </summary>
    [Flags]
    public enum StatusClearBit : byte
    {
        /// <summary>
        /// Interrupt occured
        /// </summary>
        INTCLR = 1,

        /// <summary>
        /// Temperature output overflow occured for one or more pixel
        /// </summary>
        OVFCLR = 2,

        /// <summary>
        /// Thermistor output overflow occured
        /// </summary>
        OVFTHCLR = 3
    }
}

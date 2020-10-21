// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Defines the status bits of the status register (addr: 0x04)
    /// </summary>
    [Flags]
    public enum StatusFlagBit : byte
    {
        /// <summary>
        /// Interrupt occured
        /// </summary>
        INTF = 1,

        /// <summary>
        /// Temperature output overflow occured for one or more pixel
        /// </summary>
        OVF_IRS = 2,

        /// <summary>
        /// Thermistor output overflow occured
        /// Note: the bit is only menthioned in early versions of the reference specification.
        /// It is not clear whether this is a specification error or a change in a newer
        /// revision of the sensor.
        /// </summary>
        OVF_THS = 3
    }
}

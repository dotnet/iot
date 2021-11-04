// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ht1632
{
    /// <summary>
    /// Clock modes
    /// </summary>
    public enum ClockMode : byte
    {
        /// <summary>
        /// Set slave mode and clock source from external clock, the system clock input from OSC pin and synchronous signal input from SYN pin
        /// </summary>
        Slave = Command.Slave,

        /// <summary>
        /// Set master mode and clock source from on-chip RC oscillator, the system clock output to OSC pin and synchronous signal output to SYN pin (default)
        /// </summary>
        RcMaster = Command.RcMaster,

        /// <summary>
        /// Set master mode and clock source from external clock, the system clock input from OSC pin and synchronous signal output to SYN pin
        /// </summary>
        ExternalClockMaster = Command.ExternalClockMaster,
    }
}

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
        /// Set secondary mode and clock source from external clock, the system clock input from OSC pin and synchronous signal input from SYN pin
        /// </summary>
        Secondary = Command.Secondary,

        /// <summary>
        /// Set primary mode and clock source from on-chip RC oscillator, the system clock output to OSC pin and synchronous signal output to SYN pin (default)
        /// </summary>
        RcPrimary = Command.RcPrimary,

        /// <summary>
        /// Set primary mode and clock source from external clock, the system clock input from OSC pin and synchronous signal output to SYN pin
        /// </summary>
        ExternalClockPrimary = Command.ExternalClockPrimary,
    }
}

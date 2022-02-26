// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Lp55231
{
    /// <summary>
    /// Flags related to the REG_MISC register
    /// </summary>
    [Flags]
    public enum MiscFlags : byte
    {
        /// <summary>
        /// Use internal clock
        /// </summary>
        ClockSourceSelection = 0b0000_0001,

        /// <summary>
        /// Use external clock
        /// </summary>
        ExternalClockDetection = 0b0000_0010,

        /// <summary>
        /// Enable PWM cycle power save
        /// </summary>
        PwmCyclePowersaveEnable = 0b0000_0100,

        /// <summary>
        /// Charge mode gain low bit
        /// </summary>
        ChargeModeGainLowBit = 0b0000_1000,

        /// <summary>
        /// Charge mode gain high bit
        /// </summary>
        ChargeModeGainHighBit = 0b0001_0000,

        /// <summary>
        /// Enable power save mode
        /// </summary>
        PowersaveModeEnable = 0b0010_0000,

        /// <summary>
        /// Enable auto address increment
        /// </summary>
        AddressAutoIncrementEnable = 0b0100_0000
    }
}

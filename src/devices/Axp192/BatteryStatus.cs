// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// The battery status
    /// </summary>
    [Flags]
    public enum BatteryStatus
    {
        /// <summary>
        /// Empty value
        /// </summary>
        None = 0,

        /// <summary>AXP192 is overheated</summary>
        Overheated = 0b1000_000,

        /// <summary>Charging</summary>
        Charging = 0b0100_0000,

        /// <summary>Battery Connected</summary>
        BatteryConnected = 0b0010_0000,

        /// <summary>Battery Activation Mode</summary>
        BatteryActivationMode = 0b0000_1000,

        /// <summary>Charging Current Less Than Expected</summary>
        ChargingCurrentLessThanExpected = 0b0000_0100,

        /// <summary>Mode B, when not present, the Mode A (sleep) is activated</summary>
        ModeB = 0b0000_0010,
    }
}

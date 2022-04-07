// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// ADC Pin enabled
    /// </summary>
    [Flags]
    public enum AdcPinEnabled
    {
        /// <summary>Battery Voltage</summary>
        BatteryVoltage = 0b1000_0000,

        /// <summary>Battery Current</summary>
        BatteryCurrent = 0b0100_0000,

        /// <summary>AC in Voltage</summary>
        AcInVoltage = 0b0010_0000,

        /// <summary>AC in Current</summary>
        AcInCurrent = 0b0001_0000,

        /// <summary>Vbus Voltage</summary>
        VbusVoltage = 0b0000_1000,

        /// <summary>Vbus Current</summary>
        VbusCurrent = 0b0000_0100,

        /// <summary>APS Voltage</summary>
        ApsVoltage = 0b0000_0010,

        /// <summary>TS Pin</summary>
        TsPin = 0b0000_0001,

        /// <summary>None</summary>
        None = 0b0000_0000,

        /// <summary>All</summary>
        All = 0xFF,
    }
}

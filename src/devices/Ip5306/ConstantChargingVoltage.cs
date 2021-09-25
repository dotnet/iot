// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ip5306
{
    /// <summary>
    /// Constant charging voltage
    /// </summary>
    public enum ConstantChargingVoltage
    {
        /// <summary>42 milli Volt</summary>
        Vm42 = 0b0000_0011,

        /// <summary>28 milli Volt</summary>
        Vm28 = 0b0000_0010,

        /// <summary>14 milli Volt</summary>
        Vm14 = 0b0000_0001,

        /// <summary>No constant charging voltage</summary>
        None = 0b0000_0000,
    }
}

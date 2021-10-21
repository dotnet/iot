// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Shutdown battery pin function
    /// </summary>
    public enum ShutdownBatteryPinFunction
    {
        /// <summary>High resistance.</summary>
        HighResistance = 0b0000_0000,

        /// <summary>25% 1Hz flashing.</summary>
        Flashing1Hz = 0b0001_0000,

        /// <summary>25% 4Hz flashing</summary>
        Flashing4Hz = 0b0010_0000,

        /// <summary>Output low</summary>
        OutpuLow = 0b0011_0000,
    }
}

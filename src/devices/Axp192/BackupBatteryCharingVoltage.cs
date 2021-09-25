// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Backup battery charing voltage
    /// </summary>
    public enum BackupBatteryCharingVoltage
    {
        /// <summary>3.1 V</summary>
        V3_1 = 0b0000_0000,

        /// <summary>3.0 V</summary>
        V3_0 = 0b0010_0000,

        /// <summary>2.9 V (doc says 3.0V)</summary>
        V2_9 = 0b0100_0000,

        /// <summary>2.5 V</summary>
        V2_5 = 0b0110_0000,
    }
}

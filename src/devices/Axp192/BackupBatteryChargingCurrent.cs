// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Backup battery charging current.
    /// </summary>
    public enum BackupBatteryChargingCurrent
    {
        /// <summary>50 uA</summary>
        MicroAmperes50 = 0b0000_0000,

        /// <summary>100 uA</summary>
        MicroAmperes100 = 0b0000_0001,

        /// <summary>200 uA</summary>
        MicroAmperes200 = 0b0000_0010,

        /// <summary>400 uA</summary>
        MicroAmperes400 = 0b0000_0011,
    }
}

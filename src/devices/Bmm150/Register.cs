// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmp180
{
    /// <summary>
    /// Registers of the Bmm150.
    /// </summary>
    internal enum Bmp180Register
    {
        /// <summary>
        /// Trim extended register
        /// </summary>
        BMM150_DIG_X1 = 0x5D,

        /// <summary>
        /// Trim extended register
        /// </summary>
        BMM150_DIG_Z4_LSB = 0x62,

        /// <summary>
        /// Trim extended register
        /// </summary>
        BMM150_DIG_Z2_LSB = 0x68,

        /// <summary>
        /// WIA: Device ID
        /// </summary>
        WIA = 0x40,

        /// <summary>
        /// DATA_READY_STATUS: Page 25, data ready status
        /// </summary>
        DATA_READY_STATUS = 0x48,

        /// <summary>
        /// X-axis measurement data lower 8bit
        /// </summary>
        HXL = 0x42,

        /// <summary>
        /// X-axis measurement data higher 8bit
        /// </summary>
        HXH = 0x43,

        /// <summary>
        /// Y-axis measurement data lower 8bit
        /// </summary>
        HYL = 0x44,

        /// <summary>
        /// Y-axis measurement data higher 8bit
        /// </summary>
        HYH = 0x45,

        /// <summary>
        /// Z-axis measurement data lower 8bit
        /// </summary>
        HZL = 0x46,

        /// <summary>
        /// Z-axis measurement data higher 8bit
        /// </summary>
        HZH = 0x47,

        /// <summary>
        /// Power control address
        /// </summary>
        POWER_CONTROL_ADDR = 0x4B,

        /// <summary>
        /// Op mode address
        /// </summary>
        OP_MODE_ADDR = 0x4C,
    }
}

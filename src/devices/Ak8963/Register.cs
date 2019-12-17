// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Magnetometer
{
    /// <summary>
    /// Registers of the AK8963.
    /// </summary>
    internal enum Register
    {
        /// <summary>
        /// WIA: Device ID
        /// </summary>
        WIA = 0x00,

        /// <summary>
        /// INFO: Information
        /// </summary>
        INFO = 0x01,

        /// <summary>
        /// ST1: Status 1
        /// </summary>
        ST1 = 0x02,

        /// <summary>
        /// X-axis measurement data lower 8bit
        /// </summary>
        HXL = 0x03,

        /// <summary>
        /// X-axis measurement data higher 8bit
        /// </summary>
        HXH = 0x04,

        /// <summary>
        /// Y-axis measurement data lower 8bit
        /// </summary>
        HYL = 0x05,

        /// <summary>
        /// Y-axis measurement data higher 8bit
        /// </summary>
        HYH = 0x06,

        /// <summary>
        /// Z-axis measurement data lower 8bit
        /// </summary>
        HZL = 0x07,

        /// <summary>
        /// Z-axis measurement data higher 8bit
        /// </summary>
        HZH = 0x08,

        /// <summary>
        /// ST2: Status 2
        /// </summary>
        ST2 = 0x09,

        /// <summary>
        /// CNTL1: Control1
        /// </summary>
        CNTL = 0x0A,

        /// <summary>
        /// Do not access in theory but
        /// Used to reset the device
        /// </summary>
        RSV = 0x0B,

        /// <summary>
        /// ASTC: Self Test Control
        /// </summary>
        ASTC = 0x0C,

        /// <summary>
        /// Documentation says do not access
        /// This is a test register
        /// </summary>
        TS1 = 0x0D,

        /// <summary>
        /// Documentation says do not access
        /// This is a test register
        /// </summary>
        TS2 = 0x0E,

        /// <summary>
        /// I2CDIS: I2C Disable
        /// </summary>
        I2CDIS = 0x0F,

        /// <summary>
        /// Magnetic sensor X-axis sensitivity adjustment value
        /// </summary>
        ASAX = 0x10,

        /// <summary>
        /// Magnetic sensor Y-axis sensitivity adjustment value
        /// </summary>
        ASAY = 0x11,

        /// <summary>
        /// Magnetic sensor Z-axis sensitivity adjustment value
        /// </summary>
        ASAZ = 0x12,
    }
}

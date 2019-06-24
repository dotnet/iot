// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ak8963
{
    /// <summary>
    /// Registers of the AK8963. This class is public as 
    /// need to be accessed when this device is embedded into another one
    /// like in the MPU9250
    /// </summary>
    public enum Register
    {
        WIA = 0x00,
        INFO = 0x01,
        ST1 = 0x02,
        HXL = 0x03,
        HXH = 0x04,
        HYL = 0x05,
        HYH = 0x06,
        HZL = 0x07,
        HZH = 0x08,
        ST2 = 0x09,
        CNTL = 0x0A,
        // Do not access in theory
        // Used to reset the device
        RSV = 0x0B,
        ASTC = 0x0C,
        // Do not access
        TS1 = 0x0D,
        // Do not access
        TS2 = 0x0E,
        I2CDIS = 0x0F,
        ASAX = 0x10,
        ASAY = 0x11,
        ASAZ = 0x12,
    }
}

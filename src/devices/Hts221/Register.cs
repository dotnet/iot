// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Hts221
{
    internal enum Register : byte
    {
        WhoAmI = 0x0F, // WHO_AM_I
        ResolutionMode = 0x10, // AV_CONF
        Control1 = 0x20, // CTRL_REG1
        Control2 = 0x21, // CTRL_REG2
        Control3 = 0x22, // CTRL_REG3
        Status = 0x27, // STATUS_REG
        Humidity = 0x28, // 16-bit, HUMIDITY_OUT_L (0x28), HUMIDITY_OUT_H (0x29)
        Temperature = 0x2A, // 16-bit, TEMP_OUT_L (0x2A), TEMP_OUT_H (0x2B)

        // Calibration registers

        // Humidity actual value scaled by 2, point 0 and 1
        Humidity0rHx2 = 0x30, // UInt8, H0_rH_x2
        Humidity1rHx2 = 0x31, // UInt8, H1_rH_x2

        // Temperature actual value scaled by 8, point 0 and 1 (10 bit values)
        // 8 least significant bits
        Temperature0LsbDegCx8 = 0x32, // UInt8, T0_degC_x8
        Temperature1LsbDegCx8 = 0x33, // UInt8, T1_degC_x8
        // 2 most significant bits for both points: 0bXXXXAABB (AA=T1.9-8; BB=T0.9-8; X - reserved)
        Temperature0And1MsbDegCx8 = 0x35, // 2x UInt2, T1/T0 msb

        // Humidity raw value, point 0 and 1
        Humidity0Raw = 0x36, // Int16, H0_T0_OUT
        Humidity1Raw = 0x3A, // Int16, H1_T0_OUT

        // Temperature raw value, point 0 and 1
        Temperature0Raw = 0x3C, // Int16, T0_OUT
        Temperature1Raw = 0x3E, // Int16, T1_OUT
    }
}

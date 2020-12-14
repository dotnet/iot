// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lsm9Ds1
{
    // Register for Magnetometer
    internal enum RegisterM : byte
    {
        OffsetX = 0x05, // 16-bit, OFFSET_X_REG_H_M (0x05) OFFSET_X_REG_H_M (0x06)
        OffsetY = 0x07, // 16-bit, OFFSET_Y_REG_H_M (0x07) OFFSET_Y_REG_H_M (0x08)
        OffsetZ = 0x09, // 16-bit, OFFSET_Z_REG_H_M (0x09) OFFSET_Z_REG_H_M (0x0A)
        WhoAmI = 0x0F, // WHO_AM_I
        Control1 = 0x20, // CTRL_REG1_M
        Control2 = 0x21, // CTRL_REG2_M
        Control3 = 0x22, // CTRL_REG3_M
        Control4 = 0x23, // CTRL_REG4_M
        Control5 = 0x24, // CTRL_REG5_M
        Status = 0x27, // STATUS_REG_M
        OutX = 0x28, // 16-bit, OUT_X_L_M (0x28), OUT_X_H_M (0x29)
        OutY = 0x2A, // 16-bit, OUT_Y_L_M (0x2A), OUT_Y_H_M (0x2B)
        OutZ = 0x2C, // 16-bit, OUT_Z_L_M (0x2C), OUT_Z_H_M (0x2D)
        InterruptConfig = 0x30, // INT_CFG_M
        InterruptSource = 0x31, // INT_SRC_M
        InterruptThreshold = 0x32, // 16-bit, INT_THS_L (0x32), INT_THS_H (0x33)
    }
}

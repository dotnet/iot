// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lis3DhAccelerometer
{
    // Register for Accelerometer and Gyroscope
    internal enum Register : byte
    {
        // register modifiers
        I2cAutoIncrement = 0x80,

        CTRL_REG0 = 0x1E, // SDO_PULL_UP_DISCONNECT, 0, 0, 1, 0, 0, 0, 0
        TEMP_CFG_REG = 0x1F, // ADC_ENABLE, TEMP_ENABLE, 0, 0, ...
        CTRL_REG1 = 0x20,
        CTRL_REG2 = 0x21,
        CTRL_REG3 = 0x22,
        CTRL_REG4 = 0x23,
        CTRL_REG5 = 0x24,
        CTRL_REG6 = 0x25,
        REFERENCE = 0x26,
        STATUS_REG = 0x27,
        OUT_X_L = 0x28,
        OUT_X_H = 0x29,
        OUT_Y_L = 0x2A,
        OUT_Y_H = 0x2B,
        OUT_Z_L = 0x2C,
        OUT_Z_H = 0x2D,
    }
}

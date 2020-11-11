// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lsm9Ds1
{
    // Register for Accelerometer and Gyroscope
    internal enum RegisterAG : byte
    {
        AccelerometerActivityThreshold = 0x04, // ACT_THS
        AccelerometerInactivityDuration = 0x05, // ACT_DUR
        AccelerometerInteruptConfig = 0x06, // INT_GEN_CFG_XL
        AccelerometerInterruptThresholdX = 0x07, // INT_GEN_THS_X_XL
        AccelerometerInterruptThresholdY = 0x08, // INT_GEN_THS_Y_XL
        AccelerometerInterruptThresholdZ = 0x09, // INT_GEN_THS_Z_XL
        AccelerometerInterruptDuration = 0x0A, // INT_GEN_DUR_XL
        AngularRateReferenceValue = 0x0B, // REFERENCE_G
        Interrupt1Control = 0x0C, // INT1_CTRL
        Interrupt2Control = 0x0D, // INT2_CTRL
        WhoAmI = 0x0F, // WHO_AM_I
        AngularRateControl1 = 0x10, // CTRL_REG1_G
        AngularRateControl2 = 0x11, // CTRL_REG2_G
        AngularRateControl3 = 0x12, // CTRL_REG3_G
        AngularRateSignAndOrientation = 0x13, // ORIENT_CFG_G
        AngularRateInterruptSource = 0x14, // INT_GEN_SRC_G
        Temperature = 0x15, // 16-bit, OUT_TEMP_L (0x15), OUT_TEMP_H (0x16)
        Status1 = 0x17, // STATUS_REG
        AngularRateX = 0x18, // 16-bit, Pitch (X) Axis Output, OUT_X_G
        AngularRateY = 0x1A, // 16-bit, Roll (Y) Axis Output, OUT_Y_G
        AngularRateZ = 0x1C, // 16-bit, Yaw (Z) Axis Output, OUT_Z_G
        Control4 = 0x1E, // CTRL_REG4
        AccelerometerControl5 = 0x1F, // CTRL_REG5_XL
        AccelerometerControl6 = 0x20, // CTRL_REG6_XL
        AccelerometerControl7 = 0x21, // CTRL_REG7_XL
        Control8 = 0x22, // CTRL_REG8
        Control9 = 0x23, // CTRL_REG9
        Control10 = 0x24, // CTRL_REG10
        AccelerometerInterruptSource = 0x26, // INT_GEN_SRC_XL
        Status2 = 0x27, // STATUS_REG
        AccelerometerX = 0x28, // 16-bit, OUT_X_XL
        AccelerometerY = 0x2A, // 16-bit, OUT_Y_XL
        AccelerometerZ = 0x2C, // 16-bit, OUT_Z_XL
        FifoControl = 0x2E, // FIFO_CTRL
        FifoStatusControl = 0x2F, // FIFO_SRC
        AngularRateInterruptConfig = 0x30, // INT_GEN_CFG_G
        AngularRateInterruptThresholdX = 0x31, // 16-bit, INT_GET_THS_X_G
        AngularRateInterruptThresholdY = 0x33, // 16-bit, INT_GET_THS_Y_G
        AngularRateInterruptThresholdZ = 0x35, // 16-bit, INT_GET_THS_Z_G
        AngularRateInterruptDuration = 0x37, // INT_GEN_DUR_G
    }
}

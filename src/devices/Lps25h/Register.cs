// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Lps25h
{
    internal enum Register : byte
    {
        REF_P_XL = 0x08, // Int24, REF_P_XL (0x08), REF_P_L (0x09), REF_P_H (0x0A)
        WhoAmI = 0x0F, // WHO_AM_I
        ResolutionMode = 0x10, // RES_CONF
        Control1 = 0x20, // CTRL_REG1
        Control2 = 0x21, // CTRL_REG2
        Control3 = 0x22, // CTRL_REG3
        Control4 = 0x32, // CTRL_REG4
        InterruptConfig = 0x24, // INTERRUPT_CFG
        InterruptSource = 0x25, // INT_SOURCE
        StatusReg = 0x27, // STATUS_REG
        Pressure = 0x28, // 24-bit, PRESS_OUT_XL (0x28), PRESS_OUT_L (0x29), PRESS_OUT_H (0x2A)
        Temperature = 0x2B, // 16-bit, TEMP_OUT_L (0x2B), TEMP_OUT_H (0x2C)
        FifoControl = 0x2E, // FIFO_CTRL
        FifoStatus = 0x2F, // FIFO_STATUS
        PressureInterruptThreshold = 0x30, // 16-bit, THS_P_L (0x30), THS_P_H (0x31)
        PressureOffset = 0x39, // 16-bit, RDPS_L (0x39), RDPS_H (0x3A)
    }
}

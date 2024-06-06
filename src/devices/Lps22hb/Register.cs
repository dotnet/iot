// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lps22hb
{
    internal enum Register : byte
    {
        /// <summary>
        /// Interrupt register
        /// </summary>
        INTERRUPT_CFG = 0x0B,

        /// <summary>
        /// 16-bit Pressure threshold registers
        /// THS_P_L (0x0C), THS_P_H (0x0D)
        /// </summary>
        THS_P_L = 0x0C,

        /// <summary>
        /// Who am I
        /// </summary>
        WHO_AM_I = 0x0F,

        /// <summary>
        /// Control1
        /// </summary>
        CTRL_REG1 = 0x10,

        /// <summary>
        /// Control2
        /// </summary>
        CTRL_REG2 = 0x11,

        /// <summary>
        /// Control3
        /// </summary>
        CTRL_REG3 = 0x12,

        /// <summary>
        /// FIFO configuration register
        /// </summary>
        FIFO_CTRL = 0x14,

        /// <summary>
        /// Int24 Reference pressure registers
        /// REF_P_XL (0x15), REF_P_L (0x16), REF_P_H (0x17)
        /// </summary>
        REF_P_XL = 0x15,

        /// <summary>
        /// 16-bit Pressure offset registers
        /// RDPS_L (0x18), RDPS_H (0x19)
        /// </summary>
        RPDS_L = 0x18,

        /// <summary>
        /// Resolution register
        /// </summary>
        RES_CONF = 0x1A,

        /// <summary>
        /// Interrupt register
        /// </summary>
        INT_SOURCE = 0x25,

        /// <summary>
        /// FIFO status register
        /// </summary>
        FIFO_STATUS = 0x26,

        /// <summary>
        /// Status register
        /// </summary>
        STATUS = 0x27,

        /// <summary>
        /// 24-bit Pressure output registers
        /// PRESS_OUT_XL (0x28), PRESS_OUT_L (0x29), PRESS_OUT_H (0x2A)
        /// </summary>
        PRESS_OUT_XL = 0x28,

        /// <summary>
        /// 16-bit Temperature output registers
        /// TEMP_OUT_L (0x2B), TEMP_OUT_H (0x2C)
        /// </summary>
        TEMP_OUT_L = 0x2B,

        /// <summary>
        /// Filter reset register
        /// </summary>
        LPFP_RES = 0x33,
    }
}

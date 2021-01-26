// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// registeries used by QMC5883L
    /// </summary>
    internal enum Registers : byte
    {
        QMC_CONFIG_REG_1_ADDR = 0x09,
        QMC_CONFIG_REG_2_ADDR = 0x0a,

        QMC_X_LSB_REG_ADDR = 0x00,
        QMC_X_MSB_REG_ADDR = 0x01,
        QMC_Y_LSB_REG_ADDR = 0x02,
        QMC_Y_MSB_REG_ADDR = 0x03,
        QMC_Z_LSB_REG_ADDR = 0x04,
        QMC_Z_MSB_REG_ADDR = 0x05,
        QMC_STATUS_REG_ADDR = 0x06,
        QMC_TEMP_LSB_REG_ADDR = 0x07,
        QMC_TEMP_MSB_REG_ADDR = 0x08,
        QMC_RESET_REG_ADDR = 0x0B,
        CHIP_ID = 0x0D,
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hmc5883l
{
    /// <summary>
    /// Register of HMC5883L
    /// </summary>
    internal enum Register : byte
    {
        HMC_CONFIG_REG_A_ADDR = 0x00,
        HMC_CONFIG_REG_B_ADDR = 0x01,
        HMC_MODE_REG_ADDR = 0x02,
        HMC_X_MSB_REG_ADDR = 0x03,
        HMC_Z_MSB_REG_ADDR = 0x05,
        HMC_Y_MSB_REG_ADDR = 0x07,
        HMC_STATUS_REG_ADDR = 0x09
    }
}

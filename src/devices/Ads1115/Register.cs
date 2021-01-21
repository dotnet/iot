// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Registers of ADS1115
    /// </summary>
    internal enum Register : byte
    {
        ADC_CONVERSION_REG_ADDR = 0x00,
        ADC_CONFIG_REG_ADDR = 0x01,
        ADC_CONFIG_REG_LO_THRESH = 0x02,
        ADC_CONFIG_REG_HI_THRESH = 0x03
    }
}

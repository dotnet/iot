// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Registers of ADS1115
    /// </summary>
    internal enum Registers : byte
    {
        ADC_CONVERSION_REG_ADDR = 0x00,
        ADC_CONFIG_REG_ADDR = 0x01,
    }
}

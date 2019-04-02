// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Register of ADS1115
    /// </summary>

    public enum Config : ushort
    {
        ADS1015_REG_CONFIG_OS_MASK = (0x8000),       // Mask
        ADS1015_REG_CONFIG_OS_SINGLE = (0x8000),     // Write: Set to start a single-conversion
        ADS1015_REG_CONFIG_OS_BUSY = (0x0000),       // Read: Bit = 0 when conversion is in progress
        ADS1015_REG_CONFIG_OS_NOTBUSY = (0x8000),    // Read: Bit = 1 when device is not performing a conversion

        ADS1015_REG_CONFIG_MUX_MASK = (0x7000),      // Mask
        ADS1015_REG_CONFIG_MUX_DIFF_0_1 = (0x0000),  // Differential P = AIN0, N = AIN1 (default)
        ADS1015_REG_CONFIG_MUX_DIFF_0_3 = (0x1000),  // Differential P = AIN0, N = AIN3
        ADS1015_REG_CONFIG_MUX_DIFF_1_3 = (0x2000),  // Differential P = AIN1, N = AIN3
        ADS1015_REG_CONFIG_MUX_DIFF_2_3 = (0x3000),  // Differential P = AIN2, N = AIN3
        ADS1015_REG_CONFIG_MUX_SINGLE_0 = (0x4000),  // Single-ended AIN0
        ADS1015_REG_CONFIG_MUX_SINGLE_1 = (0x5000),  // Single-ended AIN1
        ADS1015_REG_CONFIG_MUX_SINGLE_2 = (0x6000),  // Single-ended AIN2
        ADS1015_REG_CONFIG_MUX_SINGLE_3 = (0x7000),  // Single-ended AIN3

        ADS1015_REG_CONFIG_PGA_MASK = (0x0E00),      // Mask
        ADS1015_REG_CONFIG_PGA_6_144V = (0x0000),    // +/-6.144V range = Gain 2/3
        ADS1015_REG_CONFIG_PGA_4_096V = (0x0200),    // +/-4.096V range = Gain 1
        ADS1015_REG_CONFIG_PGA_2_048V = (0x0400),    // +/-2.048V range = Gain 2 (default)
        ADS1015_REG_CONFIG_PGA_1_024V = (0x0600),    // +/-1.024V range = Gain 4
        ADS1015_REG_CONFIG_PGA_0_512V = (0x0800),    // +/-0.512V range = Gain 8
        ADS1015_REG_CONFIG_PGA_0_256V = (0x0A00),    // +/-0.256V range = Gain 16

        ADS1015_REG_CONFIG_MODE_MASK = (0x0100),     // Mask
        ADS1015_REG_CONFIG_MODE_CONTIN = (0x0000),   // Continuous conversion mode
        ADS1015_REG_CONFIG_MODE_SINGLE = (0x0100),   // Power-down single-shot mode (default)

        ADS1015_REG_CONFIG_DR_MASK = (0x00E0),       // Mask
        ADS1015_REG_CONFIG_DR_8SPS = (0x0000),       // 8 samples per second
        ADS1015_REG_CONFIG_DR_16SPS = (0x0020),      // 16 samples per second
        ADS1015_REG_CONFIG_DR_32SPS = (0x0040),      // 32 samples per second
        ADS1015_REG_CONFIG_DR_64SPS = (0x0060),      // 64 samples per second
        ADS1015_REG_CONFIG_DR_128SPS = (0x0080),     // 128 samples per second (default)
        ADS1015_REG_CONFIG_DR_250SPS = (0x00A0),     // 250 samples per second
        ADS1015_REG_CONFIG_DR_475SPS = (0x00C0),     // 475 samples per second
        ADS1015_REG_CONFIG_DR_860SPS = (0x00E0),     // 860 samples per second

        ADS1015_REG_CONFIG_CMODE_MASK = (0x0010),    // Mask
        ADS1015_REG_CONFIG_CMODE_TRAD = (0x0000),    // Traditional comparator with hysteresis (default)
        ADS1015_REG_CONFIG_CMODE_WINDOW = (0x0010),  // Window comparator

        ADS1015_REG_CONFIG_CPOL_MASK = (0x0008),     // Mask
        ADS1015_REG_CONFIG_CPOL_ACTVLOW = (0x0000),  // ALERT/RDY pin is low when active (default)
        ADS1015_REG_CONFIG_CPOL_ACTVHI = (0x0008),   // ALERT/RDY pin is high when active

        ADS1015_REG_CONFIG_CLAT_MASK = (0x0004),     // Mask
        ADS1015_REG_CONFIG_CLAT_NONLAT = (0x0000),   // Non-latching comparator (default)
        ADS1015_REG_CONFIG_CLAT_LATCH = (0x0004),    // Latching comparator, ALERT/RDY pin latches once asserted

        ADS1015_REG_CONFIG_CQUE_MASK = (0x0003),     // Mask
        ADS1015_REG_CONFIG_CQUE_1CONV = (0x0000),    // Assert ALERT/RDY after one conversions
        ADS1015_REG_CONFIG_CQUE_2CONV = (0x0001),    // Assert ALERT/RDY after two conversions
        ADS1015_REG_CONFIG_CQUE_4CONV = (0x0002),    // Assert ALERT/RDY after four conversions
        ADS1015_REG_CONFIG_CQUE_NONE = (0x0003),     // Disable the comparator and put ALERT/RDY in high state (default)   
    }
}

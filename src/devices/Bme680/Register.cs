// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bme680
{
    /// <summary>
    /// General control registers for the BME680.
    /// </summary>
    /// <remarks>
    /// See section 5.2 Memory map.
    /// </remarks>
    public enum Register : byte
    {
        CHIPID = 0xD0,
        CONTROL = 0x74,
        CONTROL_HUM = 0x72,

        DIG_H1_LSB = 0xE2,
        DIG_H1_MSB = 0xE3,
        DIG_H2_LSB = 0xE2,
        DIG_H2_MSB = 0xE1,
        DIG_H3 = 0xE4,
        DIG_H4 = 0xE5,
        DIG_H5 = 0xE6,
        DIG_H6 = 0xE7,
        DIG_H7 = 0xE8,

        DIG_T1 = 0xE9,
        DIG_T2 = 0x8A,
        DIG_T3 = 0x8C,

        DIG_P1_LSB = 0x8E,
        DIG_P2_LSB = 0x90,
        DIG_P3 = 0x92,
        DIG_P4_LSB = 0x94,
        DIG_P5_LSB = 0x96,
        DIG_P6 = 0x99,
        DIG_P7 = 0x98,
        DIG_P8_LSB = 0x9C,
        DIG_P9_LSB = 0x9E,
        DIG_P10 = 0xA0,

        HUMIDITYDATA_LSB = 0x26,
        HUMIDITYDATA_MSB = 0x25,

        PRESSUREDATA_MSB = 0x1F,
        PRESSUREDATA_LSB = 0x20,
        PRESSUREDATA_XLSB = 0x21,

        STATUS = 0x1D,

        TEMPDATA_MSB = 0x22,
        TEMPDATA_LSB = 0x23,
        TEMPDATA_XLSB = 0x24,
    }
}

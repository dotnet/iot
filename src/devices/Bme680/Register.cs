// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bme680
{
    // Adresses differ for SPI implementation but can be translated using mask, see bme680_get_regs in
    // the Bosch C driver: https://github.com/BoschSensortec/BME680_driver/blob/master/bme680.c
    internal enum Register : byte
    {
        PAR_T1 = 0xE9,
        PAR_T2 = 0x8A,
        PAR_T3 = 0x8C,
        PAR_P1 = 0x8E,
        PAR_P2 = 0x90,
        PAR_P3 = 0x92,
        PAR_P4 = 0x94,
        PAR_P5 = 0x96,
        PAR_P6 = 0x99,
        PAR_P7 = 0x98,
        PAR_P8 = 0x9C,
        PAR_P9 = 0x9E,
        PAR_P10 = 0xA0,
        
        PAR_H1_LSB = 0xE2,
        PAR_H1_MSB = 0xE3,
        PAR_H2_LSB = 0xE2,
        PAR_H2_MSB = 0xE1,
        PAR_H3 = 0xE4,
        PAR_H4 = 0xE5,
        PAR_H5 = 0xE6,
        PAR_H6 = 0xE7,
        PAR_H7 = 0xE8,
        
        PAR_GH1 = 0xED,
        PAR_GH2 = 0xEB,
        PAR_GH3 = 0xEE,

        CHIP_ID = 0xD0,
        CTRL_HUM = 0x72,
        CTRL_MEAS = 0x74,
        CONFIG = 0x75,
        RESET = 0xE0,

        PRESS = 0x1F,
        TEMP = 0x22,
        HUM = 0x25,
        GAS_RES = 0x2A,
        GAS_RANGE = 0x2B,

        RES_HEAT_VAL = 0x00,
        RES_HEAT_RANGE = 0x02,
        RANGE_SW_ERR = 0x04,
        
        RES_HEAT0 = 0x5A,
        GAS_WAIT0 = 0x64,
        CTRL_GAS_0 = 0x70,
        CTRL_GAS_1 = 0x71,

        MEAS_STATUS_0 = 0x1D,
        GAS_R_LSB = 0x2B
    }
}

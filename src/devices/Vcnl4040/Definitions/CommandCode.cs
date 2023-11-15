// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the register addresses / commands of the VCNL4040 device
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal enum CommandCode
    {
        ALS_CONF = 0x00,
        ALS_THDH = 0x01,
        ALS_THDL = 0x02,
        PS_CONF_1_2 = 0x03,
        PS_CONF_3_MS = 0x04,
        PS_CANC = 0x05,
        PS_THDL = 0x06,
        PS_THDH = 0x07,
        PS_Data = 0x08,
        ALS_Data = 0x09,
        White_Data = 0x0a,
        INT_Flag = 0x0b,
        ID = 0x0c
    }
}

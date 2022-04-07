// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ip5306
{
    internal enum Register
    {
        SYS_CTL0 = 0x00,
        SYS_CTL1 = 0x01,
        SYS_CTL2 = 0x02,

        Charger_CTL0 = 0x20,
        Charger_CTL1 = 0x21,
        Charger_CTL2 = 0x22,
        Charger_CTL3 = 0x23,

        CHG_DIG_CTL0 = 0x24,

        REG_READ0 = 0x70,
        REG_READ1 = 0x71,
        REG_READ2 = 0x72,
        REG_READ3 = 0x77,
    }
}

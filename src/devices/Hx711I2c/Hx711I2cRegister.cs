// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable

namespace Iot.Device.Hx711
{
    internal enum Hx711I2cRegister : byte
    {
        REG_DATA_GET_RAM_DATA = 0x66,
        REG_DATA_GET_CALIBRATION = 0x67,
        REG_DATA_GET_PEEL_FLAG = 0x69,
        REG_DATA_INIT_SENSOR = 0x70,
        REG_SET_CAL_THRESHOLD = 0x71,
        REG_SET_TRIGGER_WEIGHT = 0x72,

        REG_CLICK_RST = 0x73,
        REG_CLICK_CAL = 0x74,
    }
}

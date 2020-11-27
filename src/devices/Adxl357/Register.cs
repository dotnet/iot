// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Iot.Device.Adxl357
{
    internal enum Register : byte
    {
        DEV_MEMS_REG_ADDR = 0x01,
        DEV_ID_REG_ADDR = 0x02,
        DEV_VERSION_ID_REG_ADDR = 0x03,
        STATUS_REG_ADDR = 0x04,
        FIFO_ENTRY_REG_ADDR = 0x06,

        ACTION_ENABLE_REG_ADDR = 0x24,
        SET_THRESHOLD_REG_ADDR = 0x25,
        GET_ACTIVE_COUNT_REG_ADDR = 0x26,
        SET_INT_PIN_MAP_REG_ADDR = 0x2a,

        X_DATA_REG_ADDR = 0x08,
        FIFO_DATA_REG_ADDR = 0x11,

        POWER_CTR_REG_ADDR = 0x2d,

        TEMPERATURE_REG_ADDR = 0x06,
        FILTER_REG_ADDR = 0x28,

        SET_RANGE_REG_ADDR = 0x2c,
        RESET_REG_ADDR = 0x2f,
    }
}

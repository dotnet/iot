// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmxx80
{
    internal enum Bme680Mask : byte
    {
        BIT_H1_DATA_MSK = 0x0F,

        PWR_MODE = 0x03,
        HEAT_OFF = 0x08,
        RUN_GAS = 0x10,

        HUMIDITY_SAMPLING = 0x07,
        FILTER_COEFFICIENT = 0x1C,
        NB_CONV = 0x0F,

        GAS_RANGE = 0x0F,
        RH_RANGE = 0x30,
        RS_ERROR = 0xF0,

        GAS_MEASURING = 0x40,
        MEASURING = 0x20,
        GAS_VALID = 0x20,
        HEAT_STAB = 0x10
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pcf8563
{
    internal enum Register : byte
    {
        PCF_CTRL1_ADDR = 0x00,
        PCF_CTRL2_ADDR = 0x01,
        PCF_SEC_ADDR = 0x02,
        PCF_MIN_ADDR = 0x03,
        PCF_HOUR_ADDR = 0x04,
        PCF_DATE_ADDR = 0x05,
        PCF_DAY_ADDR = 0x06,
        PCF_MONTH_ADDR = 0x07,
        PCF_YEAR_ADDR = 0x08,
    }
}

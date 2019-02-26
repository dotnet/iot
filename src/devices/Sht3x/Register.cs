// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Sht3x
{
    /// <summary>
    /// SHT3x Register
    /// </summary>
    internal enum Register : ushort
    {
        SHT_MEAS = 0x24,
        SHT_RESET = 0x30A2,
        SHT_HEATER_ENABLE = 0x306D,
        SHT_HEATER_DISABLE = 0x3066
    }
}

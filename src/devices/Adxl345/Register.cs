// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Adxl345
{
    /// <summary>
    /// Register of ADXL345
    /// </summary>
    internal enum Register : byte
    {
        ADLX_POWER_CTL = 0x2D,
        ADLX_DATA_FORMAT = 0x31,
        ADLX_X0 = 0x32,
        ADLX_Y0 = 0x34,
        ADLX_Z0 = 0x36,
        ADLX_SPI_RW_BIT = 0x80,
        ADLX_SPI_MB_BIT = 0x40,
    }
}

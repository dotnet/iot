// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.UFire
{
    internal enum Command : byte
    {
        ISE_MEASURE_MV = 0x50,
        ISE_MEASURE_TEMP = 0x28,
        ISE_CALIBRATE_SINGLE = 0x14,
        ISE_CALIBRATE_LOW = 0x0A,
        ISE_CALIBRATE_HIGH = 0x08,
        ISE_MEMORY_WRITE = 0x04,
        ISE_MEMORY_READ = 0x02,
        ISE_I2C = 0x01,
    }
}

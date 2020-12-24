// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max31865
{
    internal enum Register : byte
    {
        ConfigurationRead = 0x00,

        ConfigurationWrite = 0x80,

        RTDMSB = 0x01,

        RTDLSB = 0x02,

        FaultSatus = 0x07
    }
}

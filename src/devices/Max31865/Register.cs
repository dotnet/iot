// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max31865
{
    /// <summary>
    /// Register of the MAX31865 RTD-to-Digital Converter
    /// </summary>
    internal enum Register : byte
    {
        ConfigurationRead = 0x00,
        ConfigurationWrite = 0x80,
        RTDMSB = 0x01,
        RTDLSB = 0x02,
        FaultStatus = 0x07
    }
}

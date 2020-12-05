// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ags01db
{
    internal enum Register : byte
    {
        ASG_DATA_MSB = 0x00,
        ASG_DATA_LSB = 0x02,
        ASG_VERSION_MSB = 0x0A,
        ASG_VERSION_LSB = 0x01
    }
}
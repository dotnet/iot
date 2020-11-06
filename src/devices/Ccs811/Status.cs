// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ccs811
{
    [Flags]
    internal enum Status
    {
        FW_MODE = 0b1000_0000,
        APP_ERASE = 0b0100_0000,
        APP_VERIFY = 0b0010_0000,
        APP_VALID = 0b0001_0000,
        DATA_READY = 0b0000_1000,
        ERROR = 0b0000_0001,
    }
}

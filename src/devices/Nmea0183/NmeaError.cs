// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable 1591
namespace Iot.Device.Nmea0183
{
    public enum NmeaError
    {
        None = 0,
        MessageToShort,
        InvalidChecksum,
        MessageToLong,
        NoSyncByte,
        PortClosed,
        MessageDelayed
    }
}

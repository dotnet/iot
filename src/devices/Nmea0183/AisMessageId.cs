// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// An identifier for a specific AIS message type
    /// </summary>
    public record AisMessageId(AisWarningType Type, uint Mmsi)
    {
    }
}

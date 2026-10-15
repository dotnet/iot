// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183.Sentences;

#pragma warning disable CS1591
public enum GnssIntegrity : byte
{
    NoIntegrityChecking = 0,
    Safe = 1,
    Caution = 2,
}

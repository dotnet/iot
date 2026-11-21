// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183.Sentences;

#pragma warning disable CS1591
/// <summary>
/// Source of the system time
/// </summary>
public enum SystemTimeSource : byte
{
    Gps = 0,
    Glonass = 1,
    RadioStation = 2,
    LocalCesiumClock = 3,
    LocalRubidiumClock = 4,
    LocalCrystalClock = 5,
}

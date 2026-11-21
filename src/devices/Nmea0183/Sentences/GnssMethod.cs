// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183.Sentences;

#pragma warning disable CS1591
public enum GnssMethod : byte
{
    NoFix = 0,
    GnssFix = 1,
    DgnssFix = 2,
    PreciseGnss = 3,
    RtkFixedInteger = 4,
    RtkFloat = 5,
    EstimatedDrMode = 6,
    ManualInput = 7,
    SimulateMode = 8,
}

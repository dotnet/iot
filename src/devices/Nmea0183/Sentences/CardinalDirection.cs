// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183.Sentences
{
    internal enum CardinalDirection : byte
    {
        None = 0,
        North = (byte)'N',
        South = (byte)'S',
        West = (byte)'W',
        East = (byte)'E',
    }
}

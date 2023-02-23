// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Position accuracy of the GNSS position. With today's GNSS receivers, anything other than High is strange.
    /// </summary>
    public enum PositionAccuracy
    {
        /// <summary>
        /// Low
        /// </summary>
        Low,

        /// <summary>
        /// High
        /// </summary>
        High
    }
}

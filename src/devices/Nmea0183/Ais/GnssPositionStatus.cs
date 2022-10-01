// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Status of the GNSS receiver of this target
    /// </summary>
    public enum GnssPositionStatus
    {
        /// <summary>
        /// GNSS valid
        /// </summary>
        CurrentGnssPosition,

        /// <summary>
        /// GNSS position not valid
        /// </summary>
        NotGnssPosition
    }
}

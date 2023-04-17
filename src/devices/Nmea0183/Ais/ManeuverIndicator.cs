// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Used to indicate that a vessel is performing special maneuvers
    /// </summary>
    public enum ManeuverIndicator
    {
        /// <summary>
        /// Not set (default)
        /// </summary>
        NotAvailable,

        /// <summary>
        /// Not doing anything special
        /// </summary>
        NoSpecialManeuver,

        /// <summary>
        /// Carrying out a special maneuver
        /// </summary>
        SpecialManeuver
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// The RAIM flag indicates whether Receiver Autonomous Integrity Monitoring is being used
    /// </summary>
    public enum Raim
    {
        /// <summary>
        /// Raim is not in use (default)
        /// </summary>
        NotInUse,

        /// <summary>
        /// Raim is in use
        /// </summary>
        InUse
    }
}

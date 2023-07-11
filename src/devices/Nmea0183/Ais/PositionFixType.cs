// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Type of position fix
    /// </summary>
    public enum PositionFixType
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined1,

        /// <summary>
        /// GPS
        /// </summary>
        Gps,

        /// <summary>
        /// Glonass
        /// </summary>
        Glonass,

        /// <summary>
        /// GPS and Glonass
        /// </summary>
        CombinedGpsAndGlonass,

        /// <summary>
        /// LoranC (Not in use any more)
        /// </summary>
        LoranC,

        /// <summary>
        /// Chayka, russian navigation system similar to Loran-C
        /// </summary>
        Chayka,

        /// <summary>
        /// Integrated navigation system, taking many sensors into account
        /// </summary>
        IntegratedNavigationSystem,

        /// <summary>
        /// The position was surveyed (for a fixed target)
        /// </summary>
        Surveyed,

        /// <summary>
        /// Galileo
        /// </summary>
        Galileo,

        /// <summary>
        /// Undefined
        /// </summary>
        Undefined2 = 15
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Turn direction, for Autopilot and Rudder operation
    /// </summary>
    public enum TurnDirection
    {
        /// <summary>
        /// No direction command given
        /// </summary>
        NoCommand = 0,

        /// <summary>
        /// Turn or turning to port (left)
        /// </summary>
        TurnToPort = 1,

        /// <summary>
        /// Turn or turning to starboard (right)
        /// </summary>
        TurnToStarboard = 2,
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Defines how safe a given target is in relation to typically our own vessel.
    /// Safe here means that there's little risk of a collision within the defined safety bounds (e.g. the ship is more than 5 minutes away)
    /// </summary>
    public enum AisSafetyState
    {
        /// <summary>
        /// It's not known whether this ship comes close
        /// </summary>
        Unknown,

        /// <summary>
        /// The ship is moving away from us or it is very far away
        /// </summary>
        Safe,

        /// <summary>
        /// There's an imminent risk of collision with this ship
        /// </summary>
        Dangerous,

        /// <summary>
        /// The other target is lost, meaning there was no recent position update from that ship.
        /// </summary>
        Lost,

        /// <summary>
        /// The other target is so far away or the relative speed is so low, that (T)CPA calculations wouldn't be meaningful.
        /// </summary>
        FarAway
    }
}

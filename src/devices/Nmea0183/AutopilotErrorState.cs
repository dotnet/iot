// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// State of autopilot.
    /// Not all errors are serious problems, some just mean it is inactive, because it has nothing to do.
    /// </summary>
    public enum AutopilotErrorState
    {
        /// <summary>
        /// State is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// The autopilot has no route to follow.
        /// </summary>
        NoRoute,

        /// <summary>
        /// The current route has no or incomplete waypoints. Normally this resolves itself after a few seconds, when the nav software
        /// continues transmitting data.
        /// </summary>
        WaypointsWithoutPosition,

        /// <summary>
        /// Operating as slave, RMB input sentence is present, route is known
        /// </summary>
        OperatingAsSlave,

        /// <summary>
        /// The input route contains duplicate waypoints. This causes confusion and is therefore considered an error.
        /// </summary>
        RouteWithDuplicateWaypoints,

        /// <summary>
        /// A route is present
        /// </summary>
        RoutePresent,

        /// <summary>
        /// We're just having a target waypoint, but no route
        /// </summary>
        DirectGoto,

        /// <summary>
        /// The next waypoint is invalid.
        /// </summary>
        InvalidNextWaypoint,

        /// <summary>
        /// Full self-controlled operation
        /// </summary>
        OperatingAsMaster
    }
}

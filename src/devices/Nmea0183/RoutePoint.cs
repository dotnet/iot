// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// A point along a route. This is used to construct <see cref="RoutePart"/> and <see cref="Waypoint"/> sentences
    /// </summary>
    public sealed class RoutePoint : IEquatable<RoutePoint>
    {
        /// <summary>
        /// Creates a new route point.
        /// </summary>
        /// <param name="routeName">Name of the route</param>
        /// <param name="indexInRoute">The index of this point</param>
        /// <param name="totalPointsInRoute">The total number of points on this route</param>
        /// <param name="waypointName">The name of this waypoint</param>
        /// <param name="position">The position of the waypoint</param>
        /// <param name="bearingToNextWaypoint">The direction to the next waypoint (optional, will be calculated when the route is constructed)</param>
        /// <param name="distanceToNextWaypoint">The distance to the next waypoint (optional, will be calculated when the route is constructed)</param>
        /// <remarks>
        /// Route and point names should use ASCII characters only. Some devices may understand the extended ASCII code page (values > 127),
        /// but this is rare. Most devices will silently truncate any names longer than 10 chars.
        /// </remarks>
        public RoutePoint(string routeName, int indexInRoute, int totalPointsInRoute, string waypointName, GeographicPosition position,
            Angle? bearingToNextWaypoint, Length? distanceToNextWaypoint)
        {
            RouteName = routeName;
            IndexInRoute = indexInRoute;
            TotalPointsInRoute = totalPointsInRoute;
            WaypointName = waypointName;
            Position = position;
            BearingToNextWaypoint = bearingToNextWaypoint;
            DistanceToNextWaypoint = distanceToNextWaypoint;
        }

        /// <summary>
        /// The name of the route
        /// </summary>
        public string RouteName
        {
            get;
            internal set;
        }

        /// <summary>
        /// The index within the route
        /// </summary>
        public int IndexInRoute
        {
            get;
            internal set;
        }

        /// <summary>
        /// The total number of points on the route
        /// </summary>
        public int TotalPointsInRoute
        {
            get;
            internal set;
        }

        /// <summary>
        /// The name of this point
        /// </summary>
        public string WaypointName
        {
            get;
        }

        /// <summary>
        /// The position of this point
        /// </summary>
        public GeographicPosition Position
        {
            get;
        }

        /// <summary>
        /// True bearing from this waypoint to the next
        /// </summary>
        public Angle? BearingToNextWaypoint
        {
            get;
            set;
        }

        /// <summary>
        /// The distance to the next waypoint
        /// </summary>
        public Length? DistanceToNextWaypoint
        {
            get;
            set;
        }

        /// <summary>
        /// Two points are considered equal if the name and the position are equal. The other properties are NMEA-internals and are
        /// not directly related to the function of the waypoint for the user
        /// </summary>
        public bool Equals(RoutePoint? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return WaypointName == other.WaypointName && Equals(Position, other.Position);
        }

        /// <summary>
        /// Equality comparer
        /// </summary>
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || (obj is RoutePoint other && Equals(other));
        }

        /// <summary>
        /// Standard hash function
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(WaypointName, Position);
        }
    }
}

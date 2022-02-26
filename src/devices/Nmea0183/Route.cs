// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iot.Device.Common;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// High-level representation of a route (a list of waypoints)
    /// </summary>
    public sealed class Route
    {
        private List<RoutePoint> _routePoints;
        private RoutePoint? _nextPoint;

        /// <summary>
        /// Starts with an empty route
        /// </summary>
        /// <param name="name">Name of new route</param>
        public Route(String name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The route name must not be empty");
            }

            Name = name;
            _routePoints = new List<RoutePoint>();
            _nextPoint = null;
        }

        /// <summary>
        /// Construct a route from a list of points. This gets precedence over an externally defined route.
        /// </summary>
        /// <param name="name">Name of route</param>
        /// <param name="route">The new route</param>
        /// <param name="nextPoint">The next point on the route that is to be reached (optional, defaults to the first point)</param>
        /// <exception cref="ArgumentException">Different semantic errors with the definition of the route.</exception>
        public Route(string name, List<RoutePoint> route, RoutePoint? nextPoint = null)
        : this(name)
        {
            if (route == null || route.Count == 0)
            {
                throw new ArgumentException("Must provide a route with at least one element");
            }

            if (nextPoint == null)
            {
                nextPoint = route.First();
            }

            if (!route.Contains(nextPoint))
            {
                throw new ArgumentException("The next point must be part of the route");
            }

            if (route.Any(x => x.Position == null || string.IsNullOrWhiteSpace(x.WaypointName)))
            {
                throw new ArgumentException("Invalid route - No positions or no names given");
            }

            _routePoints = route;

            CalculateMetaData();
            CheckPointsUnique(route);

            _nextPoint = nextPoint;
        }

        /// <summary>
        /// Creates a route from an ordered set of points
        /// </summary>
        /// <param name="name">Name of the new route</param>
        /// <param name="points">Points of the route (ordered)</param>
        public Route(string name, params GeographicPosition[] points)
        : this(name)
        {
            for (var index = 0; index < points.Length; index++)
            {
                var pt = points[index];
                var routePt = new RoutePoint(name, index, points.Length, "WP" + index, pt, null, null);
                _routePoints.Add(routePt);
            }

            CalculateMetaData();
            // Would be weird if that fails now
            CheckPointsUnique(_routePoints);
        }

        /// <summary>
        /// Name of the route
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// True if the route is not empty
        /// </summary>
        public bool HasPoints
        {
            get
            {
                return _routePoints.Count > 0;
            }
        }

        /// <summary>
        /// Gets the first point of the route
        /// </summary>
        /// <exception cref="InvalidOperationException">The route is empty</exception>
        public RoutePoint StartPoint
        {
            get
            {
                return _routePoints.First();
            }
        }

        /// <summary>
        /// The next point on the route
        /// </summary>
        public RoutePoint? NextPoint
        {
            get
            {
                return _nextPoint;
            }
        }

        /// <summary>
        /// The points on the route
        /// </summary>
        public List<RoutePoint> Points
        {
            get
            {
                return _routePoints.ToList();
            }
        }

        /// <summary>
        /// Sets the next point on the route (i.e. to skip a missed waypoint)
        /// </summary>
        /// <param name="pt">The next point</param>
        /// <returns>True on success, false otherwise. This returns false if the given position is not part of the route.</returns>
        public bool SetNextPoint(RoutePoint pt)
        {
            return SetNextPoint(pt.Position);
        }

        /// <summary>
        /// Sets the next point on the route (i.e. to skip a missed waypoint)
        /// </summary>
        /// <param name="position">Position of next waypoint</param>
        /// <returns>True on success, false otherwise. This returns false if the given position is not part of the route.</returns>
        public bool SetNextPoint(GeographicPosition position)
        {
            var pt = _routePoints.FirstOrDefault(x => x.Position.EqualPosition(position));
            if (pt != null)
            {
                _nextPoint = pt;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds the given point to the end of the route
        /// </summary>
        /// <param name="pt">Point to insert</param>
        /// <exception cref="ArgumentException">Duplicate waypoint name, undefined position, etc.</exception>
        public void AddPoint(RoutePoint pt)
        {
            if (pt == null || pt.Position == null || string.IsNullOrWhiteSpace(pt.WaypointName))
            {
                throw new ArgumentException("Inserted point must have a position and a valid name");
            }

            var newRoute = _routePoints.ToList();
            newRoute.Add(pt);

            // throws if a duplicate was inserted
            CheckPointsUnique(newRoute);
            _routePoints = newRoute;
            CalculateMetaData();
        }

        private void CheckPointsUnique(List<RoutePoint> route)
        {
            var sorted = route.ToList();
            sorted.Sort(new UniqueSorting());
            // Throw away the result
        }

        /// <summary>
        /// Calculates distances and directions between the waypoints
        /// </summary>
        private void CalculateMetaData()
        {
            for (var index = 0; index < _routePoints.Count; index++)
            {
                var pt = _routePoints[index];
                // First, make sure the total length and other properties are copied correctly everywhere
                pt.RouteName = Name;
                pt.IndexInRoute = index;
                pt.TotalPointsInRoute = _routePoints.Count;
                if (index != _routePoints.Count - 1)
                {
                    GreatCircle.DistAndDir(pt.Position, _routePoints[index + 1].Position, out var distance, out var direction);
                    pt.DistanceToNextWaypoint = distance;
                    pt.BearingToNextWaypoint = direction;
                }
            }
        }

        private sealed class UniqueSorting : IComparer<RoutePoint>
        {
            /// <summary>
            /// This sorts the route by waypoint names. But we're actually not interested in that, we're only
            /// trying to make sure the names are unique, so this throws if the comparison returns 0
            /// </summary>
            public int Compare(RoutePoint? x, RoutePoint? y)
            {
                if (x == null)
                {
                    throw new ArgumentNullException(nameof(x));
                }

                if (y == null)
                {
                    throw new ArgumentNullException(nameof(y));
                }

                int result = String.Compare(x.WaypointName, y.WaypointName, StringComparison.Ordinal);
                if (result == 0)
                {
                    throw new ArgumentException("Names of points in route must be unique");
                }

                return result;
            }
        }
    }
}

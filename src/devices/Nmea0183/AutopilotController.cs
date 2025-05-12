// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// This class controls an auto pilot, given an input and an output stream.
    /// Depending on the input, it either refines the sequences to a higher resolution (many navigation programs will e.g. only
    /// output XTE messages with a cross track error accuracy of 0.1nm, which is useless for precise navigation) or create the
    /// sequences based on input waypoints.
    /// </summary>
    public sealed class AutopilotController : IDisposable
    {
        // Every nth iteration log the output (i.e. no route. This will repeat frequently, since normally
        // a specific state rests for longer)
        private const int LogSkip = 50;
        private readonly NmeaSinkAndSource _output;
        private readonly TimeSpan _loopTime = TimeSpan.FromMilliseconds(200);
        private readonly SentenceCache _cache;
        private readonly bool _ownsCache;

        private readonly ILogger _logger;
        private bool _threadRunning;
        private Thread? _updateThread;

        /// <summary>
        /// Last "origin" position. Used if the current route does not specify one.
        /// Assumed to be the position the user last hit "Goto" on the GPS, without explicitly defining a route.
        /// </summary>
        private RoutePoint? _currentOrigin;

        private RoutePoint? _knownNextWaypoint;
        private bool _selfNavMode;
        private RoutePoint? _manualNextWaypoint;

        private Route? _activeRoute;
        private HeadingAndDeclination? _activeDeviation;

        private PositionProvider _positionProvider;

        /// <summary>
        /// This class can control an autopilot, given an external input (of mainly WPT and RTE sentences)
        /// </summary>
        /// <param name="input">Input stream (GPS device and plotter)</param>
        /// <param name="output">Output stream (emits rmb, xte, vtg, bwc, bod)</param>
        /// <param name="cache">Sentence cache, optional</param>
        public AutopilotController(NmeaSinkAndSource input, NmeaSinkAndSource output, SentenceCache? cache = null)
        {
            _output = output;
            if (cache == null)
            {
                _ownsCache = true;
                _cache = new SentenceCache(input);
            }
            else
            {
                _ownsCache = false;
                _cache = cache;
            }

            _positionProvider = new PositionProvider(_cache);
            _threadRunning = false;
            _currentOrigin = null;
            _knownNextWaypoint = null;
            OperationState = AutopilotErrorState.Unknown;
            _selfNavMode = false;
            _manualNextWaypoint = null;
            _activeRoute = null;
            WaypointSwitchDistance = Length.FromMeters(200);
            _logger = this.GetCurrentClassLogger();
        }

        /// <summary>
        /// Returns true if the processing thread is running
        /// </summary>
        public bool Running
        {
            get
            {
                return _threadRunning && _updateThread != null && _updateThread.IsAlive;
            }
        }

        /// <summary>
        /// Name of the Source from which to take positions. Null to take any source (but this may cause side effects
        /// if multiple GPS devices are active, because they deliver slightly different data)
        /// </summary>
        public string? NmeaSourceName
        {
            get;
            set;
        }

        /// <summary>
        /// Current operating state of the autopilot controller
        /// </summary>
        public AutopilotErrorState OperationState
        {
            get;
            private set;
        }

        /// <summary>
        /// Use for testing purposes only
        /// </summary>
        internal SentenceCache SentenceCache
        {
            get
            {
                return _cache;
            }
        }

        /// <summary>
        /// Returns the next waypoint
        /// </summary>
        public RoutePoint? NextWaypoint
        {
            get;
            private set;
        }

        /// <summary>
        /// When routing ourselves (no RMB message as input), we switch to the next waypoint
        /// when closer than this distance or over the bisecting angle to the next leg
        /// </summary>
        public Length WaypointSwitchDistance
        {
            get;
            set;
        }

        /// <summary>
        /// Activates the given route.
        /// Note that it is cloned. The <see cref="Route.NextPoint"/> property is not updated automatically as the route progresses.
        /// Investigate <see cref="NextWaypoint"/> regularly instead.
        /// </summary>
        /// <param name="route">The new route</param>
        public void ActivateRoute(Route route)
        {
            _activeRoute = route;
            _manualNextWaypoint = route.NextPoint ?? route.StartPoint;
        }

        /// <summary>
        /// Disables a manually activated route.
        /// </summary>
        public void DisableActiveRoute()
        {
            _activeRoute = null;
            _manualNextWaypoint = null;
        }

        /// <summary>
        /// Starts the processing thread
        /// </summary>
        public void Start()
        {
            if (_threadRunning)
            {
                return;
            }

            _threadRunning = true;
            _updateThread = new Thread(Loop);
            _updateThread.Name = "Autopilot Control Loop";
            _updateThread.Start();
        }

        /// <summary>
        /// Stops the processing thread.
        /// </summary>
        public void Stop()
        {
            if (_updateThread != null)
            {
                _threadRunning = false;
                _updateThread.Join();
                _updateThread = null;
            }

            if (_ownsCache)
            {
                _cache.Clear();
            }

            _activeDeviation = null;
        }

        private void Loop()
        {
            int loops = 0;
            while (_threadRunning)
            {
                CalculateNewStatus(loops, DateTimeOffset.UtcNow);
                loops++;
                Thread.Sleep(_loopTime);
            }
        }

        /// <summary>
        /// Navigation loop.
        /// </summary>
        internal void CalculateNewStatus(int loops, DateTimeOffset now)
        {
            bool passedWp = false;
            RecommendedMinimumNavToDestination? currentLeg = null;
            if (_cache.TryGetLastSentence(RecommendedMinimumNavToDestination.Id, out RecommendedMinimumNavToDestination? currentLeg1)
                && currentLeg1.Valid)
            {
                passedWp = currentLeg1.Arrived;
                if (_selfNavMode)
                {
                    // Reset navigation
                    _manualNextWaypoint = null;
                    _selfNavMode = false;
                }

                OperationState = AutopilotErrorState.OperatingAsSlave;
                currentLeg = currentLeg1;
            }
            else
            {
                // So we have to test only one condition
                currentLeg = null;
            }

            if (_activeDeviation == null || loops % 100 == 0)
            {
                if (!_cache.TryGetLastSentence(HeadingAndDeclination.Id, out HeadingAndDeclination? deviation) ||
                    !deviation.Declination.HasValue)
                {
                    if (!_cache.TryGetLastSentence(RecommendedMinimumNavigationInformation.Id,
                        out RecommendedMinimumNavigationInformation? rmc) || !rmc.MagneticVariationInDegrees.HasValue)
                    {
                        if (loops % LogSkip == 0)
                        {
                            _logger.LogWarning("Autopilot: No magnetic variance");
                        }

                        return;
                    }

                    deviation = new HeadingAndDeclination(Angle.Zero, Angle.Zero, rmc.MagneticVariationInDegrees);
                }

                _activeDeviation = deviation;
            }

            if (_positionProvider.TryGetCurrentPosition(out var position, NmeaSourceName, false, out Angle track, out Speed sog, out Angle? heading, out _) && position != null)
            {
                string previousWayPoint = string.Empty;
                string nextWayPoint = string.Empty;

                if (currentLeg != null)
                {
                    previousWayPoint = currentLeg.PreviousWayPointName;
                    nextWayPoint = currentLeg.NextWayPointName;
                }

                List<RoutePoint>? currentRoute = null;
                if (_activeRoute != null)
                {
                    currentRoute = _activeRoute.Points;
                }

                RoutePoint? next;
                // This returns RoutePresent if at least one valid waypoint is in the list
                if (currentRoute == null && _positionProvider.TryGetCurrentRoute(out currentRoute) != AutopilotErrorState.RoutePresent)
                {
                    // No route. But if we have an RMB message, there could still be a current target (typically one that was
                    // directly selected with "Goto")
                    if (currentLeg == null)
                    {
                        OperationState = AutopilotErrorState.NoRoute;
                        return;
                    }

                    OperationState = AutopilotErrorState.DirectGoto;
                    next = new RoutePoint("Goto", 0, 1, currentLeg.NextWayPointName, currentLeg.NextWayPoint, null, null);
                }
                else if (currentLeg != null)
                {
                    // Better to compare by position rather than name, because the names (unless using identifiers) may
                    // not be unique.
                    next = currentRoute.FirstOrDefault(x => x.Position.EqualPosition(currentLeg.NextWayPoint));
                }
                else
                {
                    if (_manualNextWaypoint == null)
                    {
                        next = currentRoute.First();
                        _manualNextWaypoint = next;
                    }
                    else if (!HasPassedWaypoint(position, track, ref _manualNextWaypoint, currentRoute))
                    {
                        next = _manualNextWaypoint;
                    }
                    else
                    {
                        passedWp = true;
                        next = _manualNextWaypoint;
                        if (next == null) // reached end of route
                        {
                            currentRoute = null;
                        }
                    }

                    OperationState = AutopilotErrorState.OperatingAsMaster;
                }

                if (next != null && next.Position != null && (_knownNextWaypoint == null || next.Position.EqualPosition(_knownNextWaypoint.Position) == false))
                {
                    // the next waypoint changed. Set the new origin (if previous is undefined)
                    // This means that either the user has selected a new route or we moved to the next leg.
                    _knownNextWaypoint = next;
                    _currentOrigin = null;
                }

                RoutePoint? previous = null;
                if (currentRoute != null)
                {
                    previous = currentRoute.Find(x => x.WaypointName == previousWayPoint);
                }

                if (previous == null && next != null)
                {
                    if (_currentOrigin != null)
                    {
                        previous = _currentOrigin;
                    }
                    else
                    {
                        // Assume the current position is the origin
                        GreatCircle.DistAndDir(position, next.Position!, out Length distance, out Angle direction);
                        _currentOrigin = new RoutePoint("Goto", 1, 1, "Origin", position, direction,
                            distance);
                        previous = _currentOrigin;
                    }
                }
                else
                {
                    // We don't need that any more. Reinit when previous is null again
                    _currentOrigin = null;
                }

                if (next == null)
                {
                    // No position for next waypoint
                    OperationState = AutopilotErrorState.InvalidNextWaypoint;
                    NextWaypoint = null;
                    // Note: Possibly reached destination
                    return;
                }

                Length distanceToNext = Length.Zero;
                Length distanceOnTrackToNext = Length.Zero;
                Length crossTrackError = Length.Zero;
                Length distancePreviousToNext = Length.Zero;
                Angle bearingCurrentToDestination = Angle.Zero;
                Angle bearingOriginToDestination = Angle.Zero;
                GeographicPosition nextPosition = new GeographicPosition();
                Speed approachSpeedToWayPoint = Speed.Zero;

                if (next.Position != null)
                {
                    nextPosition = next.Position;
                    GreatCircle.DistAndDir(position, next.Position, out distanceToNext, out bearingCurrentToDestination);
                    approachSpeedToWayPoint = GreatCircle.CalculateVelocityTowardsTarget(next.Position, position, sog, track);

                    // Either the last waypoint or "origin"
                    if (previous != null && previous.Position != null)
                    {
                        GreatCircle.DistAndDir(previous.Position, next.Position, out distancePreviousToNext, out bearingOriginToDestination);
                        GreatCircle.CrossTrackError(previous.Position, next.Position, position, out crossTrackError, out distanceOnTrackToNext);
                    }
                }

                NextWaypoint = next;
                List<NmeaSentence> sentencesToSend = new List<NmeaSentence>();
                RecommendedMinimumNavToDestination rmb = new RecommendedMinimumNavToDestination(now,
                    crossTrackError, previousWayPoint, nextWayPoint, nextPosition, distanceToNext, bearingCurrentToDestination,
                    approachSpeedToWayPoint, passedWp);

                CrossTrackError xte = new CrossTrackError(crossTrackError);

                Angle variation = _activeDeviation.Declination.GetValueOrDefault(Angle.Zero);

                TrackMadeGood vtg = new TrackMadeGood(track, AngleExtensions.TrueToMagnetic(track, variation), sog);

                BearingAndDistanceToWayPoint bwc = new BearingAndDistanceToWayPoint(now, nextWayPoint, nextPosition, distanceToNext,
                    bearingCurrentToDestination, AngleExtensions.TrueToMagnetic(bearingCurrentToDestination, variation));

                BearingOriginToDestination bod = new BearingOriginToDestination(bearingOriginToDestination, AngleExtensions.TrueToMagnetic(
                    bearingOriginToDestination, variation), previousWayPoint, nextWayPoint);

                sentencesToSend.AddRange(new NmeaSentence[] { rmb, xte, vtg, bwc, bod });

                if (loops % 2 == 0)
                {
                    // Only send these once a second
                    IEnumerable<RoutePart> rte;
                    IEnumerable<Waypoint> wpt;
                    if (currentRoute == null || currentRoute.Count == 0)
                    {
                        currentRoute = new List<RoutePoint>();
                        if (_currentOrigin != null)
                        {
                            currentRoute.Add(_currentOrigin);
                        }

                        if (next.Position != null)
                        {
                            currentRoute.Add(next);
                        }
                    }

                    // This should actually always contain at least two points now (origin and current target)
                    if (currentRoute.Count > 0)
                    {
                        CreateRouteMessages(currentRoute, out rte, out wpt);
                        sentencesToSend.AddRange(wpt);
                        sentencesToSend.AddRange(rte);
                    }
                }

                _output.SendSentences(sentencesToSend);
            }
        }

        private bool HasPassedWaypoint(GeographicPosition position, Angle courseOverGround, ref RoutePoint? nextWaypoint, List<RoutePoint> currentRoute)
        {
            RoutePoint? previousWayPoint = null;
            RoutePoint? wayPointAfterNext = null;
            int idx = 0;

            if (nextWaypoint != null)
            {
                idx = currentRoute.IndexOf(nextWaypoint);
            }
            else
            {
                // Can't have passed a null waypoint
                return false;
            }

            if (idx < 0)
            {
                // This is weird
                return false;
            }

            if (idx == 0)
            {
                previousWayPoint = _currentOrigin;
            }
            else
            {
                previousWayPoint = currentRoute[idx - 1];
            }

            if (idx < currentRoute.Count - 2)
            {
                wayPointAfterNext = currentRoute[idx + 1];
            }

            GreatCircle.DistAndDir(position, nextWaypoint.Position, out var distanceToNext, out var angleToNext);
            if (distanceToNext < WaypointSwitchDistance)
            {
                _logger.LogInformation($"Reached waypoint {nextWaypoint.WaypointName}");
                nextWaypoint = wayPointAfterNext;
                return true;
            }

            if (previousWayPoint != null && wayPointAfterNext != null)
            {
                GreatCircle.CrossTrackError(previousWayPoint.Position, nextWaypoint.Position, position, out var crossTrackCurrentLeg,
                    out _);
                GreatCircle.CrossTrackError(nextWaypoint.Position, wayPointAfterNext.Position, position, out var crossTrackNextLeg,
                    out var distanceToAfterNext);
                Angle delta = AngleExtensions.Difference(courseOverGround, angleToNext);

                // We switch to the next leg if the cross track error to it is smaller than to the current leg.
                // This condition is obvious for the side of the route with the smaller angle (that's the one in which the
                // route bends at nextWaypoint), for the other side we need the additional condition that that waypoint
                // is no longer ahead of us. This is the case if the direction to it and our current track are pointing in
                // opposite directions
                if (crossTrackCurrentLeg > crossTrackNextLeg && Math.Abs(delta.Normalize(false).Degrees) > 90)
                {
                    _logger.LogInformation($"Reached waypoint {nextWaypoint.WaypointName}");
                    nextWaypoint = wayPointAfterNext;
                    return true;
                }
            }

            return false;
        }

        private bool CreateRouteMessages(List<RoutePoint> currentRoute, out IEnumerable<RoutePart> rte, out IEnumerable<Waypoint> wpt)
        {
            // empty route (but valid message)
            List<RoutePart> route = new List<RoutePart>() { new RoutePart(string.Empty, 1, 1, new List<string>()) };
            List<Waypoint> waypoints = new List<Waypoint>();
            if (currentRoute.Any() == false)
            {
                rte = route;
                wpt = waypoints;
                return false;
            }

            route.Clear();
            List<string> currentRouteElements = new List<string>();
            int totalElements = (int)Math.Ceiling(currentRoute.Count / 3.0);
            foreach (var pt in currentRoute)
            {
                currentRouteElements.Add(pt.WaypointName);
                // Add 3 points to each route message
                if (currentRouteElements.Count >= 3)
                {
                    route.Add(new RoutePart(pt.RouteName, totalElements, route.Count + 1, currentRouteElements));
                    currentRouteElements = new List<string>();
                }

                waypoints.Add(new Waypoint(pt.Position, pt.WaypointName));
            }

            if (currentRouteElements.Any())
            {
                // Remainder
                route.Add(new RoutePart(currentRoute[0].RouteName, totalElements, route.Count + 1, currentRouteElements));
            }

            wpt = waypoints;
            rte = route;
            return true;
        }

        /// <summary>
        /// Stops and disposes the component
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}

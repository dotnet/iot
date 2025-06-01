// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Provides high-level methods to obtain position and other aggregated data from NMEA sources.
    /// The class takes the best available data sets to generate the required output.
    /// A position can for instance be obtained from <see cref="PositionFastUpdate"/>, <see cref="GlobalPositioningSystemFixData"/> or
    /// <see cref="RecommendedMinimumNavigationInformation"/>, depending on whatever the GNSS receiver delivers.
    /// </summary>
    public class PositionProvider
    {
        private SentenceCache _cache;

        /// <summary>
        /// Create a position provider from a given data source.
        /// The data source is monitored for changes.
        /// </summary>
        /// <param name="dataSource">The data source to monitor</param>
        public PositionProvider(NmeaSinkAndSource dataSource)
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException(nameof(dataSource));
            }

            _cache = new SentenceCache(dataSource);
        }

        /// <summary>
        /// Create a position provider using an existing cache.
        /// The cache must be updated externally.
        /// </summary>
        /// <param name="cache">The cache to use</param>
        public PositionProvider(SentenceCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Provides access to the underlying sentence cache
        /// </summary>
        public SentenceCache Cache => _cache;

        /// <summary>
        /// Get the current position from the latest message containing any of the relevant data parts. This does not extrapolate the position
        /// if the last received message is old
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="track">Track (course over ground)</param>
        /// <param name="sog">Speed over ground</param>
        /// <param name="heading">Vessel Heading</param>
        /// <returns>True if a valid position is returned</returns>
        public bool TryGetCurrentPosition(out GeographicPosition? position, out Angle track, out Speed sog, out Angle? heading)
        {
            return TryGetCurrentPosition(out position, null, false, out track, out sog, out heading, out _);
        }

        /// <summary>
        /// Get the current position from the latest message containing any of the relevant data parts.
        /// If <paramref name="extrapolate"></paramref> is true, the speed and direction are used to extrapolate the position (many older
        /// GNSS receivers only deliver the position at 1Hz or less)
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="extrapolate">True to extrapolate the current position using speed and track</param>
        /// <param name="track">Track (course over ground)</param>
        /// <param name="sog">Speed over ground</param>
        /// <param name="heading">Vessel Heading</param>
        /// <returns>True if a valid position is returned</returns>
        public bool TryGetCurrentPosition(out GeographicPosition? position, bool extrapolate,
            out Angle track, out Speed sog, out Angle? heading)
        {
            return TryGetCurrentPosition(out position, null, extrapolate, out track, out sog, out heading, out _);
        }

        /// <summary>
        /// Get the current position from the latest message containing any of the relevant data parts.
        /// If <paramref name="extrapolate"></paramref> is true, the speed and direction are used to extrapolate the position (many older
        /// GNSS receivers only deliver the position at 1Hz or less)
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="source">Only look at this source (otherwise, if multiple sources provide a position, any is used)</param>
        /// <param name="extrapolate">True to extrapolate the current position using speed and track</param>
        /// <param name="track">Track (course over ground)</param>
        /// <param name="sog">Speed over ground</param>
        /// <param name="heading">Vessel Heading</param>
        /// <param name="messageTime">Time of the position report that was used</param>
        /// <returns>True if a valid position is returned</returns>
        public bool TryGetCurrentPosition(
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out GeographicPosition? position,
            String? source, bool extrapolate, out Angle track, out Speed sog, out Angle? heading,
            out DateTimeOffset messageTime)
        {
            return TryGetCurrentPosition(out position, source, extrapolate, out track, out sog, out heading,
                out messageTime, DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Get the current position from the latest message containing any of the relevant data parts.
        /// If <paramref name="extrapolate"></paramref> is true, the speed and direction are used to extrapolate the position (many older
        /// GNSS receivers only deliver the position at 1Hz or less)
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="source">Only look at this source (otherwise, if multiple sources provide a position, any is used)</param>
        /// <param name="extrapolate">True to extrapolate the current position using speed and track</param>
        /// <param name="track">Track (course over ground)</param>
        /// <param name="sog">Speed over ground</param>
        /// <param name="heading">Vessel Heading</param>
        /// <param name="messageTime">Time of the position report that was used</param>
        /// <param name="now">The current time (when working with data in the past, this may be the a time within that data set)</param>
        /// <returns>True if a valid position is returned</returns>
        public bool TryGetCurrentPosition(
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out GeographicPosition? position,
            String? source, bool extrapolate, out Angle track, out Speed sog, out Angle? heading, out DateTimeOffset messageTime, DateTimeOffset now)
        {
            messageTime = default;
            // Try to get any of the position messages
            var positionFastUpdate = (PositionFastUpdate?)_cache.GetLastSentence(source, PositionFastUpdate.Id);
            var globalPositioningSystemFixDataMessage = (GlobalPositioningSystemFixData?)_cache.GetLastSentence(source, GlobalPositioningSystemFixData.Id);
            var recommendedMinimumNavigationInformationMessage = (RecommendedMinimumNavigationInformation?)_cache.GetLastSentence(source, RecommendedMinimumNavigationInformation.Id);
            var trackMadeGoodMessage = (TrackMadeGood?)_cache.GetLastSentence(source, TrackMadeGood.Id);
            var headingTrueMessage = (HeadingTrue?)_cache.GetLastSentence(source, HeadingTrue.Id);
            TimeSpan age;

            List<(GeographicPosition, TimeSpan)> orderablePositions = new List<(GeographicPosition, TimeSpan)>();
            if (positionFastUpdate != null && positionFastUpdate.Position.ContainsValidPosition())
            {
                orderablePositions.Add((positionFastUpdate.Position, positionFastUpdate.AgeTo(now)));
                messageTime = positionFastUpdate.DateTime;
            }

            // Choose the best message we can, but if all of them are new, always use the same type
            if (positionFastUpdate == null || positionFastUpdate.Age > TimeSpan.FromSeconds(2))
            {
                if (globalPositioningSystemFixDataMessage != null && globalPositioningSystemFixDataMessage.Valid)
                {
                    orderablePositions.Add((globalPositioningSystemFixDataMessage.Position, globalPositioningSystemFixDataMessage.AgeTo(now)));
                    messageTime = globalPositioningSystemFixDataMessage.DateTime;
                }

                if (globalPositioningSystemFixDataMessage == null || globalPositioningSystemFixDataMessage.Age > TimeSpan.FromSeconds(2))
                {
                    if (recommendedMinimumNavigationInformationMessage != null && recommendedMinimumNavigationInformationMessage.Valid)
                    {
                        orderablePositions.Add((recommendedMinimumNavigationInformationMessage.Position, recommendedMinimumNavigationInformationMessage.AgeTo(now)));
                        messageTime = recommendedMinimumNavigationInformationMessage.DateTime;
                    }
                }
            }

            if (orderablePositions.Count == 0)
            {
                // No valid positions received
                position = null;
                track = Angle.Zero;
                sog = Speed.Zero;
                heading = null;
                messageTime = DateTimeOffset.MinValue;
                return false;
            }

            (position, age) = orderablePositions.OrderBy(x => x.Item2).Select(x => (x.Item1, x.Item2)).First();

            if (globalPositioningSystemFixDataMessage != null && globalPositioningSystemFixDataMessage.EllipsoidAltitude.HasValue)
            {
                // If we had seen a gga message, use its height, regardless of which other message provided the position
                position = new GeographicPosition(position.Latitude, position.Longitude, globalPositioningSystemFixDataMessage.EllipsoidAltitude.Value);
            }

            if (recommendedMinimumNavigationInformationMessage != null)
            {
                sog = recommendedMinimumNavigationInformationMessage.SpeedOverGround;
                track = recommendedMinimumNavigationInformationMessage.TrackMadeGoodInDegreesTrue;
            }
            else if (trackMadeGoodMessage != null)
            {
                sog = trackMadeGoodMessage.Speed;
                track = trackMadeGoodMessage.CourseOverGroundTrue;
            }
            else
            {
                sog = Speed.Zero;
                track = Angle.Zero;
                heading = null;
                messageTime = DateTimeOffset.MinValue;
                return false;
            }

            if (headingTrueMessage != null)
            {
                heading = headingTrueMessage.Angle;
            }
            else
            {
                heading = null;
            }

            if (extrapolate)
            {
                position = GreatCircle.CalcCoords(position, track, sog * age);
            }

            return true;
        }

        /// <summary>
        /// Returns the current route
        /// </summary>
        /// <param name="routeList">The list of points along the route</param>
        /// <returns>The state of the route received</returns>
        public AutopilotErrorState TryGetCurrentRoute(out List<RoutePoint> routeList)
        {
            routeList = new List<RoutePoint>();
            List<RoutePart>? segments = FindLatestCompleteRoute(out string routeName);
            if (segments == null)
            {
                return AutopilotErrorState.NoRoute;
            }

            List<string> wpNames = new List<string>();
            foreach (var segment in segments)
            {
                wpNames.AddRange(segment.WaypointNames);
            }

            // We've seen RTE messages, but no waypoints yet
            if (wpNames.Count == 0)
            {
                return AutopilotErrorState.WaypointsWithoutPosition;
            }

            if (wpNames.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                return AutopilotErrorState.RouteWithDuplicateWaypoints;
            }

            for (var index = 0; index < wpNames.Count; index++)
            {
                var name = wpNames[index];
                GeographicPosition? position = null;
                if (_cache.TryGetWayPoint(name, out var pt))
                {
                    position = pt.Position;
                }
                else
                {
                    // Incomplete route - need to wait for all wpt messages
                    return AutopilotErrorState.WaypointsWithoutPosition;
                }

                RoutePoint rpt = new RoutePoint(routeName, index, wpNames.Count, name, position, null, null);
                routeList.Add(rpt);
            }

            return AutopilotErrorState.RoutePresent;
        }

        private List<RoutePart>? FindLatestCompleteRoute(out string routeName)
        {
            List<RoutePart> routeSentences;
            if (!_cache.QueryActiveRouteSentences(out routeSentences))
            {
                routeName = "No route";
                return null;
            }

            routeName = string.Empty;
            RoutePart?[]? elements = null;

            // This is initially never 0 here
            while (routeSentences.Count > 0)
            {
                // Last initial sequence, take this as the header for what we combine
                var head = routeSentences.FirstOrDefault(x => x.Sequence == 1);
                if (head == null)
                {
                    routeName = "No complete route";
                    return null;
                }

                int numberOfSequences = head.TotalSequences;
                routeName = head.RouteName;

                elements = new RoutePart[numberOfSequences + 1]; // Use 1-based indexing
                bool complete = false;
                foreach (var sentence in routeSentences)
                {
                    if (sentence.RouteName == routeName && sentence.Sequence <= numberOfSequences)
                    {
                        // Iterate until we found one of each of the components of route
                        elements[sentence.Sequence] = sentence;
                        complete = true;
                        for (int i = 1; i <= numberOfSequences; i++)
                        {
                            if (elements[i] == null)
                            {
                                complete = false;
                            }
                        }

                        if (complete)
                        {
                            break;
                        }
                    }
                }

                if (complete)
                {
                    break;
                }

                // The sentence with the first header we found was incomplete - try the next (we're possibly just changing the route)
                routeSentences.RemoveRange(0, routeSentences.IndexOf(head) + 1);
            }

            List<RoutePart> ret = new List<RoutePart>();
            if (elements != null)
            {
                for (var index = 1; index < elements.Length; index++)
                {
                    var elem = elements[index];
                    if (elem == null)
                    {
                        // List is incomplete
                        return null;
                    }

                    ret.Add(elem);
                }
            }

            return ret.OrderBy(x => x.Sequence).ToList();
        }

        /// <summary>
        /// Returns the list of satellites in view
        /// </summary>
        /// <param name="totalNumberOfSatellites">Total number of satellites reported.
        /// This number might be larger than the number of elements in the list, as there might not be enough
        /// slots to transfer the whole status for all satellites</param>
        /// <returns></returns>
        public List<SatelliteInfo> GetSatellitesInView(out int totalNumberOfSatellites)
        {
            int maxSats = 0;
            if (!_cache.QuerySatellitesInView(out List<SatellitesInView> sentences))
            {
                totalNumberOfSatellites = 0;
                // Should rarely be the case
                return new List<SatelliteInfo>();
            }

            List<SatellitesInView> filtered = new List<SatellitesInView>();
            foreach (var s in sentences)
            {
                // We might be getting satellite status from more than one source
                if (!filtered.Any(x => x.Sequence == s.Sequence && x.TalkerId == s.TalkerId))
                {
                    filtered.Add(s);
                }

                if (maxSats < s.TotalSatellites)
                {
                    maxSats = s.TotalSatellites;
                }
            }

            var allsats = filtered.Select(x => x.Satellites);

            List<SatelliteInfo> ret = new();
            foreach (var list1 in allsats)
            {
                foreach (var s2 in list1)
                {
                    if (ret.All(x => x.Id != s2.Id))
                    {
                        ret.Add(s2);
                    }
                }
            }

            if (maxSats < ret.Count)
            {
                maxSats = ret.Count;
            }

            ret = ret.OrderBy(x => x.Id).ToList();
            totalNumberOfSatellites = maxSats;
            return ret;
        }
    }
}

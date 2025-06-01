// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Moq;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class PositionProviderTests
    {
        private readonly Mock<NmeaSinkAndSource> _sink;
        private readonly Mock<NmeaSinkAndSource> _dummySource;
        private readonly SentenceCache _cache;
        private readonly PositionProvider _provider;

        public PositionProviderTests()
        {
            _sink = new Mock<NmeaSinkAndSource>(MockBehavior.Strict, "Test");
            _dummySource = new Mock<NmeaSinkAndSource>(MockBehavior.Strict, "Dummy");
            _cache = new SentenceCache(_sink.Object);
            _provider = new PositionProvider(_cache);
        }

        [Fact]
        public void GetNothingEmptyRoute()
        {
            var sentence1 = new RoutePart("RT", 1, 1, new List<string>());
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);

            Assert.Equal(AutopilotErrorState.WaypointsWithoutPosition, _provider.TryGetCurrentRoute(out _));
        }

        [Fact]
        public void GetCompleteRouteOneElement()
        {
            var sentence1 = new RoutePart("RT", 1, 1, new List<string>() { "A", "B" });
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);

            _cache.Add(new Waypoint(new GeographicPosition(), "A"));
            _cache.Add(new Waypoint(new GeographicPosition(), "B"));

            Assert.Equal(AutopilotErrorState.RoutePresent, _provider.TryGetCurrentRoute(out var route));
            Assert.Equal(2, route.Count);
            Assert.Equal("RT", route[0].RouteName);
            Assert.Equal(2, route[0].TotalPointsInRoute);
            Assert.Equal(0, route[0].IndexInRoute);
            Assert.Equal("A", route[0].WaypointName);
            Assert.Equal("B", route[1].WaypointName);
            Assert.Equal(1, route[1].IndexInRoute);
        }

        [Fact]
        public void GetCompleteRouteOneElement2()
        {
            // Even with three times the same message, this should return just one route
            var sentence1 = new RoutePart("RT", 1, 1, new List<string>() { "A", "B" });
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);
            var sentence2 = new Waypoint(new GeographicPosition(1, 2, 3), "A");
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence2);
            var sentence3 = new Waypoint(new GeographicPosition(2, 3, 4), "B");
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence3);

            Assert.Equal(AutopilotErrorState.RoutePresent, _provider.TryGetCurrentRoute(out var route));
            Assert.Equal(2, route.Count);
        }

        [Fact]
        public void GetCompleteRouteTwoElements()
        {
            // Even with three times the same message, this should return just one route
            var sentence1 = new RoutePart("RT", 2, 1, new List<string>() { "A", "B" });
            var sentence2 = new RoutePart("RT", 2, 2, new List<string>() { "C", "D" });
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence2);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);

            _cache.Add(new Waypoint(new GeographicPosition(), "A"));
            _cache.Add(new Waypoint(new GeographicPosition(), "B"));
            _cache.Add(new Waypoint(new GeographicPosition(), "C"));
            _cache.Add(new Waypoint(new GeographicPosition(), "D"));

            _provider.TryGetCurrentRoute(out var route);
            Assert.Equal(4, route.Count);
            Assert.Equal("RT", route[0].RouteName);
            Assert.Equal(4, route[0].TotalPointsInRoute);
            Assert.Equal(0, route[0].IndexInRoute);
            Assert.Equal("A", route[0].WaypointName);
            Assert.Equal("B", route[1].WaypointName);
            Assert.Equal(1, route[1].IndexInRoute);
        }

        [Fact]
        public void GetNewestRoute()
        {
            // The latest route is interesting
            var sentence1 = new RoutePart("RTOld", 1, 1, new List<string>() { "A", "B" });
            var sentence2 = new RoutePart("RTNew", 1, 1, new List<string>() { "C", "D" });
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence2);

            _cache.Add(new Waypoint(new GeographicPosition(), "A"));
            _cache.Add(new Waypoint(new GeographicPosition(), "B"));
            _cache.Add(new Waypoint(new GeographicPosition(), "C"));
            _cache.Add(new Waypoint(new GeographicPosition(), "D"));

            _provider.TryGetCurrentRoute(out var route);
            Assert.Equal(2, route.Count);
            Assert.Equal("RTNew", route[0].RouteName);
        }

        [Fact]
        public void FindOlderRouteIfNewIsIncomplete1()
        {
            // The latest route is interesting
            var sentence1 = new RoutePart("RTOld", 1, 1, new List<string>() { "A", "B" });
            var sentence2 = new RoutePart("RTNew", 1, 1, new List<string>() { "C", "D" });
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence2);

            _cache.Add(new Waypoint(new GeographicPosition(), "A"));
            _cache.Add(new Waypoint(new GeographicPosition(), "B"));
            // Not all waypoints for new route
            _cache.Add(new Waypoint(new GeographicPosition(), "D"));

            // This will skip the iteration
            Assert.Equal(AutopilotErrorState.WaypointsWithoutPosition, _provider.TryGetCurrentRoute(out _));
        }

        [Fact]
        public void FindOlderRouteIfNewIsIncomplete()
        {
            var sentence1 = new RoutePart("RTOld", 2, 1, new List<string>() { "A", "B" });
            var sentence1b = new RoutePart("RTOld", 2, 2, new List<string>()
            {
                "C"
            });
            var sentence2 = new RoutePart("RTNew", 3, 1, new List<string>() { "C", "D" });
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1b);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence2);

            _cache.Add(new Waypoint(new GeographicPosition(), "A"));
            _cache.Add(new Waypoint(new GeographicPosition(), "B"));
            _cache.Add(new Waypoint(new GeographicPosition(), "C"));
            _cache.Add(new Waypoint(new GeographicPosition(), "D"));

            // Only part 1 of the new route was transmitted - use the old one until we have all messages for the new route.
            _provider.TryGetCurrentRoute(out var route);
            Assert.Equal(3, route.Count);
            Assert.Equal("RTOld", route[0].RouteName);
        }

        [Fact]
        public void FillCacheAndTest()
        {
            using NmeaLogDataReader reader = new NmeaLogDataReader("Reader", TestDataHelper.GetResourceStream("Nmea-2021-08-25-16-25.txt"));
            _provider.Cache.MaxDataAge = TimeSpan.FromDays(10000);
            reader.OnNewSequence += (source, msg) =>
            {
                _cache.Add(msg);
            };
            reader.StartDecode();
            reader.StopDecode();

            Assert.True(_provider.TryGetCurrentPosition(out var position, false, out Angle track, out Speed sog, out Angle? heading));
            Assert.Equal(new GeographicPosition(57.055204999999994, 9.9178983333333335, 0), position);
            Assert.Equal(84.8, track.Degrees);
            Assert.Equal(2.7, sog.Knots);
            Assert.True(heading.HasValue);
            Assert.Equal(35.9, heading!.Value.Degrees);

            Assert.True(_cache.TryGetLastSentence(MeteorologicalComposite.Id, out MeteorologicalComposite? sentence));
            Assert.NotNull(sentence);
            Assert.Equal(26.6, sentence.WaterTemperature!.Value.DegreesCelsius);

            var sats = _provider.GetSatellitesInView(out int totalSats);
            Assert.Equal(16, sats.Count);
            Assert.Equal(18, totalSats);
            Assert.DoesNotContain(sats, x => x == null);
            Assert.Equal("02", sats[0].Id);

            Assert.Null(_cache.GetLastSentence(MeteorologicalComposite.Id, TimeSpan.Zero));
            Assert.NotNull(_cache.GetLastSentence(MeteorologicalComposite.Id, TimeSpan.FromDays(99999)));
        }
    }
}

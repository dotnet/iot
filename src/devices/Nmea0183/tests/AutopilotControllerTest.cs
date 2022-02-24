// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Moq;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public sealed class AutopilotControllerTest : IDisposable
    {
        private AutopilotController _autopilot;
        private Mock<NmeaSinkAndSource> _source;
        private Mock<NmeaSinkAndSource> _output;

        public AutopilotControllerTest()
        {
            _source = new Mock<NmeaSinkAndSource>(MockBehavior.Loose, "Input");
            _output = new Mock<NmeaSinkAndSource>(MockBehavior.Strict, "Output");
            _autopilot = new AutopilotController(_source.Object, _output.Object);
        }

        public void Dispose()
        {
            _autopilot.Dispose();
        }

        [Fact]
        public void DoesNothingWhenNoUsableInput()
        {
            _autopilot.CalculateNewStatus(0, DateTimeOffset.UtcNow);
        }

        [Fact]
        public void StatusAfterCtor()
        {
            Assert.False(_autopilot.Running);
            Assert.Null(_autopilot.NextWaypoint);
            Assert.Equal(AutopilotErrorState.Unknown, _autopilot.OperationState);
        }

        [Fact]
        public void StartAndStopThread()
        {
            _autopilot.Start();
            Assert.True(_autopilot.Running);
            _autopilot.Stop();
            Assert.False(_autopilot.Running);
        }

        [Fact]
        public void CalculationLoopWithExternalInput()
        {
            string[] inputSequences =
            {
                // Messages in a typical scenario, where an external GPS sends a route and navigates along it
                "$GPWPL,4729.0235,N,00929.0536,E,R1",
                "$GPWPL,4729.1845,N,00929.7746,E,R2",
                "$GPWPL,4729.1214,N,00930.2754,E,R3",
                "$GPWPL,4728.9218,N,00930.3359,E,R4",
                "$GPWPL,4728.8150,N,00929.9999,E,R5",
                "$GPBOD,244.8,T,242.9,M,R5,R4",
                "$GPRMB,A,0.50,L,R4,R5,4728.8150,N,00929.9999,E,0.737,201.6,-1.7,V,D",
                "$GPRMC,115615.000,A,4729.49810,N,00930.39910,E,1.600,38.500,240520,1.900,E,D",
                "$GPRTE,1,1,c,Route 008,R1,R2,R3,R4,R5",
                "$HCHDG,30.9,,,1.9,E",
                "$GPXTE,A,A,0.500,L,N,D",
                "$GPVTG,38.5,T,36.6,M,1.6,N,3.0,K,A"
            };

            // Similar to input, except with added accuracy and some additional messages
            // (even though quite a bit of the information is redundant across messages)
            string[] expectedOutput =
            {
                "$GPRMB,A,0.504,L,R4,R5,4728.81500,N,00929.99990,E,0.735,201.6,-1.5,V,D",
                "$GPXTE,A,A,0.504,L,N,D",
                "$GPVTG,38.5,T,36.6,M,1.6,N,3.0,K,A",
                "$GPBWC,183758.833,4728.81500,N,00929.99990,E,201.6,T,199.7,M,0.735,N,R5,D",
                "$GPBOD,244.9,T,243.0,M,R5,R4",
                "$GPWPL,4729.02350,N,00929.05360,E,R1",
                "$GPWPL,4729.18450,N,00929.77460,E,R2",
                "$GPWPL,4729.12140,N,00930.27540,E,R3",
                "$GPWPL,4728.92180,N,00930.33590,E,R4",
                "$GPWPL,4728.81500,N,00929.99990,E,R5",
                "$GPRTE,2,1,c,Route 008,R1,R2,R3",
                "$GPRTE,2,2,c,Route 008,R4,R5"
            };

            DateTimeOffset now = new DateTimeOffset(2020, 05, 31, 18, 37, 58, 833, TimeSpan.Zero);
            _output.Setup(x => x.SendSentences(It.IsNotNull<IEnumerable<NmeaSentence>>())).Callback<IEnumerable<NmeaSentence>>(
                outputSentence =>
                {
                    // Check that the messages that should be sent are equal to what's defined above
                    Assert.True(outputSentence.Any());
                    int index = 0;
                    foreach (var msg in outputSentence)
                    {
                        string txt = msg.ToNmeaParameterList();
                        txt = $"${TalkerId.GlobalPositioningSystem}{msg.SentenceId},{txt}";
                        Assert.Equal(expectedOutput[index], txt);
                        index++;
                    }

                    Assert.Equal(12, index);
                });

            ParseSequencesAndAddToCache(inputSequences);
            _autopilot.CalculateNewStatus(0, now);
        }

        [Fact]
        public void CalculationLoopWithExternalInputRmbOnly()
        {
            string[] inputSequences =
            {
                // Messages in a typical scenario, where an external GPS sends just an RMB, but no waypoints
                "$GPBOD,244.8,T,242.9,M,R5,R4",
                "$GPRMB,A,0.50,L,R4,R5,4728.8150,N,00929.9999,E,0.737,201.6,-1.7,V,D",
                "$GPRMC,115615.000,A,4729.49810,N,00930.39910,E,1.600,38.500,240520,1.900,E,D",
                "$HCHDG,30.9,,,1.9,E",
                "$GPXTE,A,A,0.500,L,N,D",
                "$GPVTG,38.5,T,36.6,M,1.6,N,3.0,K,A"
            };

            // Similar to input, except with added accuracy and some additional messages
            // (even though quite a bit of the information is redundant across messages)
            string[] expectedOutput =
            {
                // This becomes the origin, so cross track error is 0
                "$GPRMB,A,0.000,R,R4,R5,4728.81500,N,00929.99990,E,0.735,201.6,-1.5,V,D",
                "$GPXTE,A,A,0.000,R,N,D",
                "$GPVTG,38.5,T,36.6,M,1.6,N,3.0,K,A",
                "$GPBWC,183758.833,4728.81500,N,00929.99990,E,201.6,T,199.7,M,0.735,N,R5,D",
                "$GPBOD,201.6,T,199.7,M,R5,R4",
                "$GPWPL,4729.49810,N,00930.39910,E,Origin",
                "$GPWPL,4728.81500,N,00929.99990,E,R5",
                "$GPRTE,1,1,c,Goto,Origin,R5"
            };

            DateTimeOffset now = new DateTimeOffset(2020, 05, 31, 18, 37, 58, 833, TimeSpan.Zero);
            _output.Setup(x => x.SendSentences(It.IsNotNull<IEnumerable<NmeaSentence>>())).Callback<IEnumerable<NmeaSentence>>(
                outputSentence =>
                {
                    // Check that the messages that should be sent are equal to what's defined above
                    Assert.True(outputSentence.Any());
                    int index = 0;
                    foreach (var msg in outputSentence)
                    {
                        string txt = msg.ToNmeaParameterList();
                        txt = $"${TalkerId.GlobalPositioningSystem}{msg.SentenceId},{txt}";
                        Assert.Equal(expectedOutput[index], txt);
                        index++;
                    }
                });

            ParseSequencesAndAddToCache(inputSequences);
            _autopilot.CalculateNewStatus(0, now);
        }

        private void ParseSequencesAndAddToCache(IEnumerable<string> inputSequences)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            foreach (var seq in inputSequences)
            {
                var decoded = TalkerSentence.FromSentenceString(seq, out var error)!;
                Assert.Equal(NmeaError.None, error);
                Assert.NotNull(decoded);
                var s = decoded.TryGetTypedValue(ref now)!;
                _autopilot.SentenceCache.Add(s);
            }
        }

        [Fact]
        public void FullAutoRouting()
        {
            var testRoute = new List<RoutePoint>();
            testRoute.Add(new RoutePoint("R1", 0, 3, "A1", new GeographicPosition(1, 0, 0), Angle.Zero, Length.Zero));
            testRoute.Add(new RoutePoint("R1", 1, 3, "A2", new GeographicPosition(1.001, 0, 0), Angle.FromDegrees(10), Length.Zero));
            testRoute.Add(new RoutePoint("R1", 2, 3, "A3", new GeographicPosition(1.002, 0.001, 0), Angle.Zero, Length.Zero));
            Route rt = new Route("TEST", testRoute);
            _autopilot.ActivateRoute(rt);
            SetPositionAndTrack(new GeographicPosition(0.9, 0, 0), Angle.Zero);

            bool outputGenerated = false;
            _output.Setup(x => x.SendSentences(It.IsNotNull<IEnumerable<NmeaSentence>>())).Callback<IEnumerable<NmeaSentence>>(
                outputSentence =>
                {
                    Assert.True(outputSentence.Any());
                    outputGenerated = true;
                });

            _autopilot.CalculateNewStatus(0, DateTimeOffset.UtcNow);
            Assert.Equal(AutopilotErrorState.OperatingAsMaster, _autopilot.OperationState);
            Assert.Equal(testRoute[0], _autopilot.NextWaypoint);
            Assert.True(outputGenerated);

            outputGenerated = false;
            SetPositionAndTrack(new GeographicPosition(0.9001, 0.001, 0), Angle.FromDegrees(5));
            _autopilot.CalculateNewStatus(1, DateTimeOffset.UtcNow);

            Assert.Equal(AutopilotErrorState.OperatingAsMaster, _autopilot.OperationState);
            Assert.Equal(testRoute[0], _autopilot.NextWaypoint);
            Assert.True(outputGenerated);

            // Run over first waypoint (on the left of the track)
            outputGenerated = false;
            SetPositionAndTrack(new GeographicPosition(1.0005, -0.001, 0), Angle.FromDegrees(5));
            _autopilot.CalculateNewStatus(2, DateTimeOffset.UtcNow);

            Assert.Equal(AutopilotErrorState.OperatingAsMaster, _autopilot.OperationState);
            Assert.Equal(testRoute[1], _autopilot.NextWaypoint);
            Assert.True(outputGenerated);
        }

        private void SetPositionAndTrack(GeographicPosition position, Angle track)
        {
            _autopilot.SentenceCache.Add(new RecommendedMinimumNavigationInformation(DateTimeOffset.Now, NavigationStatus.Valid,
                position, Speed.FromMetersPerSecond(10), track, Angle.FromDegrees(-2)));
        }
    }
}

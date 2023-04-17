// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Moq;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public sealed class MessageRouterTests : IDisposable
    {
        private Mock<NmeaSinkAndSource> _route1;
        private Mock<NmeaSinkAndSource> _route2;
        private MessageRouter _router;

        public MessageRouterTests()
        {
            _route1 = new Mock<NmeaSinkAndSource>(MockBehavior.Strict, "1");
            _route2 = new Mock<NmeaSinkAndSource>(MockBehavior.Strict, "2");
            _router = new MessageRouter();
            _router.AddEndPoint(_route1.Object);
            _router.AddEndPoint(_route2.Object);
        }

        public void Dispose()
        {
            _router.Dispose();
            _route1.VerifyAll();
            _route2.VerifyAll();
        }

        [Fact]
        public void RoutingCanBeConstructed()
        {
            var route1 = new Mock<NmeaSinkAndSource>(MockBehavior.Strict, "1");
            var route2 = new Mock<NmeaSinkAndSource>(MockBehavior.Strict, "2");
            MessageRouter mr = new MessageRouter();
            Assert.Equal(2, mr.EndPoints.Count); // Already has the default internal end point and the logger
            mr.AddEndPoint(route1.Object);
            mr.AddEndPoint(route2.Object);
            Assert.Equal(4, mr.EndPoints.Count);
        }

        [Fact]
        public void NothingHappensWhenNoFilter()
        {
            _route1.Raise(x => x.OnNewSequence += null, _router, TestSentence());
            _router.SendSentence(_router, TestSentence());
        }

        [Fact]
        public void AddSimpleOutgoingFilter()
        {
            // This rule sends all messages from the local client to all (other) sinks
            FilterRule f = new FilterRule(MessageRouter.LocalMessageSource, TalkerId.Any, SentenceId.Any, new[] { "1", "2" }, false);
            _router.AddFilterRule(f);

            var sentence = TypedTestSentence();
            _route1.Setup(x => x.SendSentence(_router, sentence));
            _route2.Setup(x => x.SendSentence(_router, sentence));

            _router.SendSentence(_router, sentence);
        }

        [Fact]
        public void AddSimpleOutgoingFilterRawOnly()
        {
            // Configuring false for the last parameter here on the local outgoing rule is probably not useful, because
            // the local sender will typically provide typed messages.
            FilterRule f = new FilterRule(MessageRouter.LocalMessageSource, TalkerId.Any, SentenceId.Any, new[] { "1", "2" }, true);
            _router.AddFilterRule(f);
            _router.SendSentence(_router, TypedTestSentence());
        }

        [Fact]
        public void AddIncomingTalkerRule()
        {
            // These two rules define that messages with a YDGGA header (Navigation data from the NMEA2000 Gateway) shall be
            // discarded and GPGGA messages used instead. These (in my test case) contain elevation as well.
            FilterRule f1 = new FilterRule("*", new TalkerId('Y', 'D'), new SentenceId("GGA"), new List<string>(), false);
            FilterRule f2 = new FilterRule("*", new TalkerId('G', 'P'), new SentenceId("GGA"), new[] { "1", "2" }, false);

            _router.AddFilterRule(f1);
            _router.AddFilterRule(f2);

            NmeaSentence.OwnTalkerId = new TalkerId('Y', 'D');
            var gnssSentence = GnssSentence();
            // Discarded
            _route1.Raise(x => x.OnNewSequence += null, _route1.Object, gnssSentence);

            // Forwarded to all (including itself)
            NmeaSentence.OwnTalkerId = new TalkerId('G', 'P');

            gnssSentence = GnssSentence();

            _route1.Setup(x => x.SendSentence(_route2.Object, gnssSentence));
            _route2.Setup(x => x.SendSentence(_route2.Object, gnssSentence));

            _route2.Raise(x => x.OnNewSequence += null, _route2.Object, gnssSentence);
        }

        [Fact]
        public void DecodableMessageIsOnlyForwardedOnce()
        {
            FilterRule f1 = new FilterRule("*", new TalkerId('G', 'P'), new SentenceId("GGA"), new[] { "1" }, true);

            _router.AddFilterRule(f1);

            NmeaSentence.OwnTalkerId = new TalkerId('G', 'P');
            var sentence1 = GnssSentence();
            var sentence2 = GnssRawSentence();
            // Only the raw message should be forwarded to the other sink (here sending from 2 to 1)
            _route1.Setup(x => x.SendSentence(_route2.Object, sentence2));

            // Forwarded to all, but only once
            _route2.Raise(x => x.OnNewSequence += null, _route2.Object, sentence1);
            _route2.Raise(x => x.OnNewSequence += null, _route2.Object, sentence2);
        }

        [Theory]
        [InlineData(MessageRouter.LocalMessageSource)]
        [InlineData("1")]
        public void MessageLoopingDoesNotCauseStackOverflow(string target)
        {
            FilterRule f1 = new FilterRule("*", new TalkerId('G', 'P'), new SentenceId("GGA"), new[] { target }, true);

            _router.AddFilterRule(f1);

            NmeaSentence.OwnTalkerId = new TalkerId('G', 'P');
            var sentence2 = GnssRawSentence();
            // Only the raw message should be forwarded to the other sink (here sending from 2 to 1)
            if (target != MessageRouter.LocalMessageSource)
            {
                _route1.Setup(x => x.SendSentence(_route1.Object, sentence2));
            }

            _route1.Raise(x => x.OnNewSequence += null, _route1.Object, sentence2);
        }

        private RawSentence TestSentence()
        {
            // A time message
            return new RawSentence(TalkerId.GlobalPositioningSystem, new SentenceId("ZDA"),
                new string[] { "135302.036", "02", "02", "2020", "+01", "00" }, DateTimeOffset.UtcNow);
        }

        private HeadingMagnetic TypedTestSentence()
        {
            return new HeadingMagnetic(12.2);
        }

        private GlobalPositioningSystemFixData GnssSentence()
        {
            return new GlobalPositioningSystemFixData(DateTimeOffset.UtcNow, GpsQuality.DifferentialFix, new GeographicPosition(47.49, 9.48, 720),
                680, 2.4, 10);
        }

        private RawSentence GnssRawSentence()
        {
            return new RawSentence(TalkerId.GlobalPositioningSystem, new SentenceId("GGA"),
                "163824, 4728.7024, N, 00929.9665, E, 2, 12, 0.6, 398.5, M, 46.8, M,,".Split(new char[] { ',' }, StringSplitOptions.None),
                new DateTimeOffset(2020, 04, 26, 16, 38, 24, TimeSpan.Zero));
        }
    }
}

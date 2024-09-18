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
    public class SentenceCacheTest
    {
        private SentenceCache _cache;
        private Mock<NmeaSinkAndSource> _sink;
        private Mock<NmeaSinkAndSource> _dummySource;

        public SentenceCacheTest()
        {
            _sink = new Mock<NmeaSinkAndSource>(MockBehavior.Strict, "Test");
            _dummySource = new Mock<NmeaSinkAndSource>(MockBehavior.Strict, "Dummy");
            _cache = new SentenceCache(_sink.Object);
        }

        [Fact]
        public void CacheKeepsLastElement()
        {
            var sentence1 = new HeadingTrue(10.2);
            var sentence2 = new HeadingTrue(-1);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence2);

            Assert.Equal(sentence2, _cache.GetLastSentence(HeadingTrue.Id));
        }

        [Fact]
        public void ReturnsNullNoSuchElement()
        {
            var sentence1 = new HeadingTrue(10.2);
            _sink.Raise(x => x.OnNewSequence += null, _dummySource.Object, sentence1);

            Assert.Null(_cache.GetLastSentence(HeadingMagnetic.Id));
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using Moq;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// Extending this class with more unit tests would require Moq'ing the Firmata interface. This is not currently possible.
    /// Simulating on the stream level is probably not worth the effort - better get the simulator to work automatically.
    /// </summary>
    [Trait("feature", "firmata")]
    public sealed class ApiBehaviorTests : IDisposable
    {
        private Mock<Stream> _streamMock;
        private ArduinoBoard _board;

        public ApiBehaviorTests()
        {
            _streamMock = new Mock<Stream>();
            _board = new ArduinoBoard(_streamMock.Object);
        }

        public void Dispose()
        {
            _board.Dispose();
        }

        [Fact]
        public void InitializeWithStreamNoConnection()
        {
            var streamMock = new Mock<Stream>(MockBehavior.Strict);

            streamMock.Setup(x => x.WriteByte(255));
            streamMock.Setup(x => x.WriteByte(249));
            streamMock.Setup(x => x.Flush());
            streamMock.Setup(x => x.CanRead).Returns(true);
            streamMock.Setup(x => x.CanWrite).Returns(true);
            streamMock.Setup(x => x.Close());
            var board = new ArduinoBoard(streamMock.Object);
            Assert.Throws<TimeoutException>(() => board.FirmataVersion);
        }

        [Fact]
        public void TestStreamIsReadWrite()
        {
            _streamMock = new Mock<Stream>();
            _streamMock.Setup(x => x.CanRead).Returns(true);
            _streamMock.Setup(x => x.CanWrite).Returns(false);
            var board = new ArduinoBoard(_streamMock.Object);
            Assert.Throws<NotSupportedException>(() => board.FirmataVersion);
        }
    }
}

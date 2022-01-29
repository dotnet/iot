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
    public sealed class ApiBehaviorTests : IDisposable
    {
        private Stream _stream;
        private ArduinoBoard _board;

        public ApiBehaviorTests()
        {
            _stream = new MemoryStream();
            _board = new ArduinoBoard(_stream);
        }

        public void Dispose()
        {
            _board.Dispose();
        }

        [Fact]
        public void InitializeWithStreamNoConnection()
        {
            using var board = new ArduinoBoard(_stream);
            Assert.Throws<TimeoutException>(() => board.FirmataVersion);
        }

        [Fact]
        public void TestStreamIsReadWrite()
        {
            _stream = new MemoryStream(new byte[100], false);
            using var board = new ArduinoBoard(_stream);
            Assert.Throws<NotSupportedException>(() => board.FirmataVersion);
        }
    }
}

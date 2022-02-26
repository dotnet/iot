// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class NmeaUdpServerTest : IDisposable
    {
        private readonly NmeaUdpServer _server;

        public NmeaUdpServerTest()
        {
            _server = new NmeaUdpServer("Test", 10102);
        }

        public void Dispose()
        {
            _server.StopDecode();
            _server.Dispose();
        }

        [Fact]
        public void ServerCanStartAndStop()
        {
            _server.StartDecode();
            _server.StopDecode();
            _server.Dispose();
        }

        [Fact]
        public void ServerCanSendData()
        {
            _server.StartDecode();
            _server.SendSentence(new DepthBelowSurface(Length.FromMeters(10)));
        }

        [Fact]
        public void ServerDoesNotReceiveOwnData()
        {
            _server.StartDecode();
            bool fail = false;
            _server.OnNewSequence += (source, sentence) => fail = true;
            _server.SendSentence(new DepthBelowSurface(Length.FromMeters(10)));
            Assert.False(fail);
        }
    }
}

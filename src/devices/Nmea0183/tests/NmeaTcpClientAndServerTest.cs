// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    // This test is unreliable when run with netcoreapp3.1 on linux (might hang, or fail in a strange way)
#if NET5_0_OR_GREATER
    public class NmeaTcpClientAndServerTest
    {
        private List<NmeaSentence> _sentencesReceivedByServer;
        private List<NmeaSentence> _sentencesReceivedByClient;

        public NmeaTcpClientAndServerTest()
        {
            _sentencesReceivedByServer = new();
            _sentencesReceivedByClient = new();
        }

        [Fact]
        public void ClientCommunicatesWithServerOverTcp()
        {
            int portToUse = NetworkPortScanner.GetNextAvailableTcpPort(1027);
            using NmeaTcpServer server = new NmeaTcpServer("Server", IPAddress.Any, portToUse);

            server.OnNewSequence += ServerOnNewSequence;
            server.StartDecode();

            using NmeaTcpClient client = new NmeaTcpClient("Client", "localhost", portToUse);
            client.OnNewSequence += ClientOnNewSequence;
            client.StartDecode();

            int timeout = 50;
            while (!client.Connected)
            {
                Thread.Sleep(100);
                if (timeout-- < 0)
                {
                    throw new TimeoutException();
                }
            }

            Assert.True(client.Connected);
            Thread.Sleep(10);
            Assert.Equal("Client", client.InterfaceName);
            Assert.Equal("Server", server.InterfaceName);
            var clientSentence = new GlobalPositioningSystemFixData(DateTimeOffset.UtcNow, GpsQuality.DifferentialFix, new GeographicPosition(2, 3, 4), 100, 2, 12);
            var serverSentence = new DepthBelowSurface(Length.FromMeters(5.2));
            client.SendSentence(clientSentence);
            server.SendSentence(serverSentence);

            timeout = 100;
            while (_sentencesReceivedByClient.Count < 2 || _sentencesReceivedByServer.Count < 2)
            {
                Thread.Sleep(100);
                if (timeout-- < 0)
                {
                    throw new TimeoutException($"Only received {_sentencesReceivedByClient.Count} messages from server and {_sentencesReceivedByServer.Count} from client");
                }

                // Send again, a first message might be lost
                client.SendSentence(clientSentence);
                server.SendSentence(serverSentence);
            }

            client.StopDecode();
            server.StopDecode();

            Assert.False(client.Connected);

            // Two, because both a decoded and a raw sentence will be used in the callback
            Assert.True(_sentencesReceivedByClient.Count % 2 == 0);
            Assert.True(_sentencesReceivedByServer.Count % 2 == 0);

            Assert.Equal(_sentencesReceivedByClient[0].ToReadableContent(), serverSentence.ToReadableContent());
            Assert.Equal(_sentencesReceivedByServer[0].ToReadableContent(), clientSentence.ToReadableContent());

            Assert.Contains(_sentencesReceivedByClient, x => x.GetType() == typeof(RawSentence));
            Assert.Contains(_sentencesReceivedByServer, x => x.GetType() == typeof(RawSentence));
        }

        private void ClientOnNewSequence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            _sentencesReceivedByClient.Add(sentence);
        }

        private void ServerOnNewSequence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            _sentencesReceivedByServer.Add(sentence);
        }
    }
#endif
}

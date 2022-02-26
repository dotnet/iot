// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;

namespace Nmea.Simulator
{
    internal class Simulator
    {
        private static readonly TimeSpan UpdateRate = TimeSpan.FromMilliseconds(500);
        private Thread? _simulatorThread;
        private bool _terminate;
        private SimulatorData _activeData;
        private NmeaTcpServer? _server;

        public Simulator()
        {
            _activeData = new SimulatorData();
        }

        public static void Main(string[] args)
        {
            var sim = new Simulator();
            sim.StartServer();
        }

        private void StartServer()
        {
            _server = null;
            try
            {
                NmeaSentence.OwnTalkerId = new TalkerId('G', 'P');

                _terminate = false;
                _simulatorThread = new Thread(MainSimulator);
                _simulatorThread.Start();
                _server = new NmeaTcpServer("Server");
                _server.StartDecode();
                _server.OnNewSequence += (source, sentence) =>
                {
                    if (sentence is RawSentence)
                    {
                        Console.WriteLine($"Received message: {sentence.ToReadableContent()}");
                    }
                };

                Console.WriteLine("Waiting for connections. Press x to quit");
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.KeyChar == 'x')
                        {
                            break;
                        }
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (SocketException x)
            {
                Console.WriteLine($"There was a socket exception listening on the network: {x}");
            }
            finally
            {
                _server?.StopDecode();
                if (_simulatorThread != null)
                {
                    _terminate = true;
                    _simulatorThread?.Join();
                }

                _server?.Dispose();
            }
        }

        private void SendNewData()
        {
            try
            {
                var data = _activeData;
                RecommendedMinimumNavigationInformation rmc = new RecommendedMinimumNavigationInformation(DateTimeOffset.UtcNow,
                    NavigationStatus.Valid, data.Position,
                    data.Speed, data.Course, null);
                SendSentence(rmc);

                GlobalPositioningSystemFixData gga = new GlobalPositioningSystemFixData(
                    DateTimeOffset.UtcNow, GpsQuality.DifferentialFix, data.Position, data.Position.EllipsoidalHeight - 54,
                    2.5, 10);
                SendSentence(gga);

                TimeDate zda = new TimeDate(DateTimeOffset.UtcNow);
                SendSentence(zda);
            }
            catch (IOException x)
            {
                Console.WriteLine($"Error writing to the output stream: {x.Message}. Connection lost.");
            }
        }

        private void SendSentence(NmeaSentence sentence)
        {
            if (_server != null)
            {
                Console.WriteLine($"Sending {sentence.ToReadableContent()}");
                _server.SendSentence(sentence);
            }
        }

        private void MainSimulator()
        {
            while (!_terminate)
            {
                var newData = _activeData.Clone();
                GeographicPosition newPosition = GreatCircle.CalcCoords(newData.Position, _activeData.Course,
                    _activeData.Speed * UpdateRate);
                newData.Position = newPosition;
                _activeData = newData;

                SendNewData();
                Thread.Sleep(UpdateRate);
            }
        }

        private sealed class ParserData : IDisposable
        {
            public ParserData(TcpClient tcpClient, NmeaParser parser, Thread thread)
            {
                TcpClient = tcpClient;
                Parser = parser;
                Thread = thread;
                TerminateThread = false;
            }

            public TcpClient TcpClient { get; }
            public NmeaParser Parser { get; }
            public Thread Thread { get; }

            public bool TerminateThread
            {
                get;
                set;
            }

            public void Dispose()
            {
                TcpClient.Close();
                TcpClient.Dispose();
                TerminateThread = true;
                Parser.Dispose();
                Thread.Join();
            }
        }
    }
}

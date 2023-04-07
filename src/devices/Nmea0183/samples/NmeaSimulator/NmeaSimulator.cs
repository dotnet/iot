// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
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
        private NmeaTcpServer? _tcpServer;
        private NmeaUdpServer? _udpServer;

        public Simulator()
        {
            _activeData = new SimulatorData();
            ReplayFiles = new List<string>();
        }

        private List<string> ReplayFiles
        {
            get;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Simple GNSS simulator");
            Console.WriteLine("Usage: NmeaSimulator [options]");

            Console.WriteLine("Options are:");
            Console.WriteLine("--replay files    Plays back the given NMEA log file in real time (only with shifted timestamps). Wildcards supported.");
            Console.WriteLine();
            var sim = new Simulator();
            if (args.Length >= 2 && args[0] == "--replay")
            {
                var wildCards = args[1];
                FileInfo fi = new FileInfo(wildCards);
                string path = fi.DirectoryName ?? string.Empty;
                sim.ReplayFiles.AddRange(Directory.GetFiles(path, fi.Name));
            }

            sim.StartServer();
        }

        private void StartServer()
        {
            _tcpServer = null;
            _udpServer = null;
            try
            {
                NmeaSentence.OwnTalkerId = new TalkerId('G', 'P');

                _terminate = false;
                if (!ReplayFiles.Any())
                {
                    _simulatorThread = new Thread(MainSimulator);
                    _simulatorThread.Start();
                }
                else
                {
                    _simulatorThread = new Thread(FilePlayback);
                    _simulatorThread.Start();
                }

                _tcpServer = new NmeaTcpServer("TcpServer");
                _tcpServer.StartDecode();
                _tcpServer.OnNewSequence += OnNewSequenceFromServer;

                // Outgoing port is 10110, the incoming port is irrelevant (but we choose it differently here, so that a
                // receiver can bind to 10110 on the same computer)
                _udpServer = new NmeaUdpServer("UdpServer", 10111, 10110);
                _udpServer.StartDecode();
                _udpServer.OnNewSequence += OnNewSequenceFromServer;

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
                _tcpServer?.StopDecode();
                _udpServer?.StopDecode();
                if (_simulatorThread != null)
                {
                    _terminate = true;
                    _simulatorThread?.Join();
                }

                _tcpServer?.Dispose();
                _udpServer?.Dispose();
            }
        }

        // We're not really expecting input here.
        private void OnNewSequenceFromServer(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            if (sentence is RawSentence)
            {
                Console.WriteLine($"Received message: {sentence.ToReadableContent()} from {source.InterfaceName}");
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
            if (_tcpServer != null)
            {
                Console.WriteLine($"Sending {sentence.ToReadableContent()}");
                _tcpServer.SendSentence(sentence);
            }

            if (_udpServer != null)
            {
                _udpServer.SendSentence(sentence);
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

        private void FilePlayback()
        {
            NmeaLogDataReader rd = new NmeaLogDataReader("LogDataReader", ReplayFiles);
            rd.DecodeInRealtime = true;
            rd.OnNewSequence += (source, sentence) => SendSentence(sentence);
            rd.StartDecode();
            // Dummy thread, to keep code flow similar to standard case
            while (!_terminate)
            {
                Thread.Sleep(200);
            }

            rd.StopDecode();
            rd.Dispose();
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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Iot.Device;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Seatalk1;
using UnitsNet;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace Nmea.Simulator
{
    internal class Simulator
    {
        private static readonly TimeSpan UpdateRate = TimeSpan.FromMilliseconds(500);
        private readonly SimulatorArguments _arguments;
        private Thread? _simulatorThread;
        private bool _terminate;
        private SimulatorData _activeData;
        private NmeaTcpServer? _tcpServer;
        private NmeaUdpServer? _udpServer;
        private SeatalkToNmeaConverter? _seatalkInterface;
        private Random _random;

        public Simulator(SimulatorArguments arguments)
        {
            _activeData = new SimulatorData();
            ReplayFiles = new List<string>();
            _random = new Random();
            _arguments = arguments;
        }

        private List<string> ReplayFiles
        {
            get;
        }

        public static int Main(string[] args)
        {
            Console.WriteLine("Simple GNSS simulator");
            Console.WriteLine("Usage: NmeaSimulator [options]");

            var parser = new Parser(x =>
            {
                x.AutoHelp = true;
                x.AutoVersion = true;
                x.CaseInsensitiveEnumValues = true;
                x.ParsingCulture = CultureInfo.InvariantCulture;
                x.CaseSensitive = false;
                x.HelpWriter = Console.Out;
            });

            var parsed = parser.ParseArguments<SimulatorArguments>(args);
            if (parsed.Errors.Any())
            {
                Console.WriteLine("Error in command line");
                return 1;
            }

            if (parsed.Value.Debug)
            {
                Console.WriteLine("Waiting for debugger...");
                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(100);
                }
            }

            if (parsed.Value.Verbose)
            {
                LogDispatcher.LoggerFactory = new SimpleConsoleLoggerFactory(LogLevel.Trace);
            }

            var sim = new Simulator(parsed.Value);
            if (!string.IsNullOrWhiteSpace(parsed.Value.ReplayFiles))
            {
                var wildCards = parsed.Value.ReplayFiles;
                FileInfo fi = new FileInfo(wildCards);
                string path = fi.DirectoryName ?? string.Empty;
                sim.ReplayFiles.AddRange(Directory.GetFiles(path, fi.Name));
            }

            sim.StartServer();

            return 0;
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

                _tcpServer = new NmeaTcpServer("TcpServer", IPAddress.Any, _arguments.TcpPort);
                _tcpServer.StartDecode();
                _tcpServer.OnNewSequence += OnNewSequenceFromServer;

                // This code block tries to determine the broadcast address of the local master ethernet adapter.
                // This needs adjustment if the main adapter is a WIFI port.
                string broadcastAddress = "255.255.255.255";
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var a in interfaces)
                {
                    if (a.OperationalStatus == OperationalStatus.Up && a.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        var properties = a.GetIPProperties();
                        foreach (var unicast in properties.UnicastAddresses)
                        {
                            if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                byte[] ipBytes = unicast.Address.GetAddressBytes();
                                // A virtual address usually looks like a router address, that means it's last part is .1
                                if (ipBytes[3] == 1)
                                {
                                    continue;
                                }

                                byte[] maskBytes = unicast.IPv4Mask.GetAddressBytes();
                                for (int i = 0; i < ipBytes.Length; i++)
                                {
                                    // Make all bits 1 that are NOT set in the mask
                                    ipBytes[i] |= (byte)~maskBytes[i];
                                }

                                broadcastAddress = new IPAddress(ipBytes).ToString();
                            }
                        }

                        break;
                    }
                }

                // Outgoing port is 10110, the incoming port is irrelevant (but we choose it differently here, so that a
                // receiver can bind to 10110 on the same computer)
                _udpServer = new NmeaUdpServer("UdpServer", _arguments.UdpPort, _arguments.UdpPort, broadcastAddress);
                _udpServer.StartDecode();
                _udpServer.OnNewSequence += OnNewSequenceFromServer;

                // Also optionally directly connect to the Seatalk1 bus.
                // For simplicity, this example is not using a MessageRouter.
                if (!string.IsNullOrWhiteSpace(_arguments.SeatalkInterface))
                {
                    _seatalkInterface = new SeatalkToNmeaConverter("Seatalk1", _arguments.SeatalkInterface);
                    _seatalkInterface.OnNewSequence += SeatalkNewSequence;
                    _seatalkInterface.SentencesToTranslate.Add(SentenceId.Any);
                    _seatalkInterface.StartDecode();
                }

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
                _seatalkInterface?.StopDecode();
                _seatalkInterface?.Dispose();
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

        private void SeatalkNewSequence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            SendSentence(sentence);
        }

        // We're not really expecting input here.
        private void OnNewSequenceFromServer(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            if (sentence is RawSentence)
            {
                Console.WriteLine($"Received message: {sentence.ToReadableContent()} from {source.InterfaceName}");
            }

            if (_seatalkInterface != null)
            {
                _seatalkInterface.SendSentence(source, sentence);
            }
        }

        private void SendNewData()
        {
            try
            {
                var data = _activeData;
                RecommendedMinimumNavigationInformation rmc = new RecommendedMinimumNavigationInformation(DateTimeOffset.UtcNow,
                    NavigationStatus.Valid, data.Position,
                    data.SpeedOverGround, data.Course, null);
                SendSentence(rmc);

                GlobalPositioningSystemFixData gga = new GlobalPositioningSystemFixData(
                    DateTimeOffset.UtcNow, GpsQuality.DifferentialFix, data.Position, data.Position.EllipsoidalHeight - 54,
                    2.5, 10);
                SendSentence(gga);

                TimeDate zda = new TimeDate(DateTimeOffset.UtcNow);
                SendSentence(zda);

                WindSpeedAndAngle mwv = new WindSpeedAndAngle(data.WindDirectionRelative, data.WindSpeedRelative, true);
                SendSentence(mwv);

                WaterSpeedAndAngle vhw = new WaterSpeedAndAngle(null, null, data.SpeedTroughWater);
                SendSentence(vhw);

                var engineData = new EngineData(10000, EngineStatus.None | EngineStatus.MaintenanceNeeded | EngineStatus.LowCoolantLevel, 0, RotationalSpeed.FromRevolutionsPerMinute(2000), Ratio.FromPercent(20), TimeSpan.FromHours(1.5), Temperature.FromDegreesCelsius(60.6));
                // EngineRevolutions rv = new EngineRevolutions(TalkerId.ElectronicChartDisplayAndInformationSystem, engineData);
                // SendSentence(rv);
                SeaSmartEngineFast fast = new SeaSmartEngineFast(engineData);
                SendSentence(fast);
                SeaSmartEngineDetail detail = new SeaSmartEngineDetail(engineData);
                SendSentence(detail);

                GeographicPosition target = new GeographicPosition(47.54, 9.48, 0);
                GreatCircle.DistAndDir(data.Position, target, out var distance, out var direction);
                Speed vmg = Math.Cos(AngleExtensions.Difference(data.Course, direction).Radians) * data.SpeedOverGround;
                Length xtError = Length.FromNauticalMiles(0.2);
                var rmb = new RecommendedMinimumNavToDestination(zda.DateTime, xtError, "Start", "FH", target, distance, direction, vmg, false);
                SendSentence(rmb);

                var xte = new CrossTrackError(xtError);
                SendSentence(xte);
                // Test Seatalk message (understood by some OpenCPN plugins)
                ////RawSentence sentence = new RawSentence(new TalkerId('S', 'T'), new SentenceId("ALK"), new string[]
                ////{
                ////    "84", "86", "26", "97", "02", "00", "00", "00", "08"
                ////}, DateTimeOffset.UtcNow);
                ////SendSentence(sentence);
            }
            catch (IOException x)
            {
                Console.WriteLine($"Error writing to the output stream: {x.Message}. Connection lost.");
            }
        }

        private void SendSentence(NmeaSentence sentence)
        {
            Console.WriteLine($"Sending {sentence.ToReadableContent()}");
            if (_tcpServer != null)
            {
                _tcpServer.SendSentence(sentence);
            }

            if (_udpServer != null)
            {
                _udpServer.SendSentence(sentence);
            }

            if (_seatalkInterface != null)
            {
                _seatalkInterface.SendSentence(sentence);
            }
        }

        private void MainSimulator()
        {
            while (!_terminate)
            {
                var newData = _activeData.Clone();
                newData.SpeedOverGround = UnitMath.Clamp(newData.SpeedOverGround + Speed.FromKnots(_random.NextDouble() - 0.5), Speed.Zero, Speed.FromKnots(12));
                newData.SpeedTroughWater = UnitMath.Clamp(newData.SpeedOverGround + Speed.FromKnots(1.5), Speed.Zero, Speed.FromKnots(10.0));
                newData.WindSpeedRelative = UnitMath.Clamp(newData.WindSpeedRelative + Speed.FromKnots(_random.NextDouble() - 0.5), Speed.Zero, Speed.FromKnots(65));
                newData.WindDirectionRelative = newData.WindDirectionRelative + Angle.FromDegrees(_random.NextDouble() * 2.0);
                newData.WindDirectionRelative = newData.WindDirectionRelative.Normalize(true);
                GeographicPosition newPosition = GreatCircle.CalcCoords(newData.Position, newData.Course,
                    newData.SpeedOverGround * UpdateRate);
                newData.Position = newPosition;
                _activeData = newData;

                SendNewData();
                Thread.Sleep(UpdateRate);
            }
        }

        private void FilePlayback()
        {
            NmeaLogDataReader rd = new NmeaLogDataReader("LogDataReader", ReplayFiles);
            rd.Loop = _arguments.Loop;
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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Microsoft.Extensions.Logging;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Nmea0183.Samples
{
    internal class Program
    {
        private AutopilotStatus _currentAutopilotStatus = AutopilotStatus.Offline;
        private Angle _currentDesiredHeading = Angle.FromDegrees(110);
        private Angle _currentDesiredWindAngle = Angle.FromDegrees(350);
        private Dictionary<int, SwitchStatus> _switches = new Dictionary<int, SwitchStatus>();

        public static void Main(string[] args)
        {
            LogDispatcher.LoggerFactory = new SimpleConsoleLoggerFactory(LogLevel.Trace);
            var p = new Program();
            // p.UsingNeoM8Serial();
            // p.UsingNetwork();
            p.SimulateDigitalSwitch();
        }

        private void UsingSerial()
        {
            DateTimeOffset lastMessageTime = DateTimeOffset.UtcNow;
            using (var sp = new SerialPort("/dev/ttyS0"))
            {
                sp.NewLine = "\r\n";
                sp.Open();

                // Device streams continuously and therefore most of the time we would end up in the middle of the line
                // therefore ignore first line so that we align correctly
                sp.ReadLine();

                bool gotRmc = false;
                while (!gotRmc)
                {
                    string line = sp.ReadLine();
                    TalkerSentence? sentence = TalkerSentence.FromSentenceString(line, out _);

                    if (sentence == null)
                    {
                        continue;
                    }

                    object? typed = sentence.TryGetTypedValue(ref lastMessageTime);
                    if (typed == null)
                    {
                        Console.WriteLine($"Sentence identifier `{sentence.Id}` is not known.");
                    }
                    else if (typed is RecommendedMinimumNavigationInformation rmc)
                    {
                        gotRmc = true;

                        if (rmc.Position.ContainsValidPosition())
                        {
                            Console.WriteLine($"Your location: {rmc.Position}");
                        }
                        else
                        {
                            Console.WriteLine($"You cannot be located.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Sentence of type `{typed.GetType().FullName}` not handled.");
                    }
                }
            }
        }

        private void UsingNetwork()
        {
            try
            {
                // using (TcpClient client = new TcpClient("192.168.1.43", 10110))
                using (TcpClient client = new TcpClient("127.0.0.1", 10110))
                {
                    Console.WriteLine("Connected!");
                    var stream = client.GetStream();
                    bool closed = false;
                    using (NmeaParser parser = new Nmea0183Parser("Test", stream, stream))
                    {
                        parser.OnParserError += (source, msg, error) =>
                        {
                            Console.WriteLine($"Error while parsing message '{msg}': {error}");
                            if (error == NmeaError.PortClosed)
                            {
                                closed = true;
                            }
                        };
                        parser.OnNewSequence += ParserOnNewSequence;
                        parser.StartDecode();
                        while (!Console.KeyAvailable && !closed)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
            catch (SocketException x)
            {
                Console.WriteLine($"Error connecting to host: {x}");
            }
        }

        /// <summary>
        /// This sample uses an NMEA 2000 parser to simulate a Raymarine EV-type autopilot. It will connect to a TCP server and send messages that are expected by the autopilot.
        /// It will also listen for messages from the autopilot and update the current status accordingly.
        /// </summary>
        private void SimulateAutopilot()
        {
            try
            {
                using (NmeaTcpClient client = new NmeaTcpClient("Autopilot", "192.168.121.50", 1457, new Nmea2000YdwgParserFactory()))
                {
                    bool closed = false;
                    Console.WriteLine("Connected!");
                    {
                        client.OnParserError += (source, msg, error) =>
                        {
                            Console.WriteLine($"Error while parsing message '{msg}': {error}");
                            if (error == NmeaError.PortClosed)
                            {
                                closed = true;
                            }
                        };
                        client.OnNewSequence += ParserOnNewSequenceForAutopilot;
                        client.OnParserError += Client_OnOnParserError;
                        client.StartDecode();

                        bool exit = false;
                        int loop = 0;
                        while (!exit && !closed)
                        {
                            Thread.Sleep(100);
                            loop++;
                            if (Console.KeyAvailable)
                            {
                                var k = Console.ReadKey(true);
                                switch (k.Key)
                                {
                                    case ConsoleKey.Q:
                                        exit = true;
                                        break;
                                    case ConsoleKey.S:
                                        _currentAutopilotStatus = AutopilotStatus.Standby;
                                        break;
                                    case ConsoleKey.A:
                                        _currentAutopilotStatus = AutopilotStatus.Auto;
                                        break;
                                    case ConsoleKey.W:
                                        _currentAutopilotStatus = AutopilotStatus.Wind;
                                        break;
                                    case ConsoleKey.O:
                                        _currentAutopilotStatus = AutopilotStatus.Offline;
                                        break;
                                }

                                Console.WriteLine($"New status: {_currentAutopilotStatus}!");
                            }

                            if (_currentAutopilotStatus != AutopilotStatus.Offline && loop % 4 == 0)
                            {
                                SeatalkNgPilotStatus status = new SeatalkNgPilotStatus(_currentAutopilotStatus);
                                client.SendSentence(status);
                            }

                            if (_currentAutopilotStatus != AutopilotStatus.Offline)
                            {
                                if (loop % 3 == 1)
                                {
                                    var heading2 =
                                        new SeatalkNgPilotLockedHeading(null, _currentDesiredHeading);
                                    client.SendSentence(heading2);

                                    if (_currentAutopilotStatus == AutopilotStatus.Wind)
                                    {
                                        var wind = new SeatalkNgPilotWindStatus(_currentDesiredWindAngle,
                                            null);
                                        client.SendSentence(wind);
                                    }
                                }

                                if (loop % 3 == 2)
                                {
                                    var heading = new SeatalkNgPilotHeading(null, _currentDesiredHeading);
                                    client.SendSentence(heading);
                                    var rudder = new Rudder(Angle.FromDegrees(0), Angle.FromDegrees(0), 0, 0);
                                    client.SendSentence(rudder);
                                }

                                if (loop % 3 == 0)
                                {
                                    var vs = new VesselHeading(_currentDesiredHeading, null, null, true);
                                    client.SendSentence(vs);
                                }
                            }
                        }
                    }
                }
            }
            catch (SocketException x)
            {
                Console.WriteLine($"Error connecting to host: {x}");
            }
        }

        /// <summary>
        /// This sample uses an NMEA 2000 parser to simulate a Raymarine EV-type autopilot. It will connect to a TCP server and send messages that are expected by the autopilot.
        /// It will also listen for messages from the autopilot and update the current status accordingly.
        /// </summary>
        private void SimulateDigitalSwitch()
        {
            try
            {
                using (NmeaTcpClient client = new NmeaTcpClient("Switch", "192.168.101.50", 1457, new Nmea2000YdwgParserFactory()))
                {
                    bool closed = false;
                    Console.WriteLine("Connected!");
                    client.OnParserError += (source, msg, error) =>
                    {
                        Console.WriteLine($"Error while parsing message '{msg}': {error}");
                        if (error == NmeaError.PortClosed)
                        {
                            closed = true;
                        }
                    };
                    client.OnNewSequence += ParserOnNewSequenceForSwitch;
                    client.OnParserError += Client_OnOnParserError;
                    client.StartDecode();

                    bool exit = false;
                    int loop = 0;
                    _switches[0] = SwitchStatus.Off;
                    _switches[1] = SwitchStatus.Off;
                    _switches[2] = SwitchStatus.Off;
                    _switches[3] = SwitchStatus.Off;
                    while (!exit && !closed)
                    {
                        Thread.Sleep(500);
                        loop++;
                        if (Console.KeyAvailable)
                        {
                            var k = Console.ReadKey(true);
                            switch (k.Key)
                            {
                                case ConsoleKey.Q:
                                    exit = true;
                                    break;
                                case ConsoleKey.D1:
                                    _switches[0] = _switches[0] == SwitchStatus.On ? SwitchStatus.Off : SwitchStatus.On;
                                    break;
                                case ConsoleKey.D2:
                                    _switches[1] = _switches[1] == SwitchStatus.On ? SwitchStatus.Off : SwitchStatus.On;
                                    break;
                                case ConsoleKey.D3:
                                    _switches[2] = _switches[2] == SwitchStatus.On ? SwitchStatus.Off : SwitchStatus.On;
                                    break;
                                case ConsoleKey.D4:
                                    _switches[3] = _switches[3] == SwitchStatus.On ? SwitchStatus.Off : SwitchStatus.On;
                                    break;
                            }
                        }

                        byte dipswitch = 0x2;
                        var switchStatus = new BinarySwitchStatus(dipswitch, _switches);
                        client.SendSentence(switchStatus);
                        Thread.Sleep(100);
                        var switchStatus2 = new CzoneChannelState(dipswitch, _switches);
                        client.SendSentence(switchStatus2);
                        Thread.Sleep(100);
                        var switchStatus3 = new CzoneCircuitStatus(dipswitch, _switches);
                        client.SendSentence(switchStatus3);
                        ////Thread.Sleep(100);
                        ////var switchStatus4 = new CzoneModuleAnnounce(0x40);
                        ////client.SendSentence(switchStatus4);
                    }
                }
            }
            catch (SocketException x)
            {
                Console.WriteLine($"Error connecting to host: {x}");
            }
        }

        private void ParserOnNewSequenceForSwitch(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            if (sentence is GroupFunctionMessage gf)
            {
                Console.WriteLine($"Found a GroupFunctionMessage: {gf.ToReadableContent()}");
            }

            if (sentence is CzoneCircuitControl cc)
            {
                Console.WriteLine($"Found a CzoneCircuitControl: {cc.ToReadableContent()}");
                cc.ButtonOffset = 5;
                if (cc.NewStatus != SwitchStatus.NoAction)
                {
                    _switches[cc.Channel] = cc.NewStatus;
                }
            }
        }

        private void Client_OnOnParserError(NmeaSinkAndSource arg1, string arg2, NmeaError arg3)
        {
            Console.WriteLine($"Parser error: {arg2} type {arg3}");
        }

        private void ParserOnNewSequence(NmeaSinkAndSource parser, NmeaSentence sentence)
        {
            Console.WriteLine(sentence.ToReadableContent());
        }

        private void ParserOnNewSequenceForAutopilot(NmeaSinkAndSource parser, NmeaSentence sentence)
        {
            // Console.WriteLine(sentence.ToReadableContent());
            if (sentence is GroupFunctionMessage gf)
            {
                if (gf.Pgn == SeatalkNgPilotStatus.HexId && gf.ParameterConstantsMatch() &&
                    gf.Parameters[0].Constant == 1851 && gf.Function == GroupFunction.Command)
                {
                    int newMode = gf.Parameters[3].Value.GetValueOrDefault();
                    if (newMode == 0xFFFF && _currentAutopilotStatus == AutopilotStatus.Wind)
                    {
                        // Seen this when a tack is requested. However, the plotter offers the wrong tack
                        // direction right now.
                        // Submode is 4 for tack to starboard and 3 for tack to port. We can use this to determine the correct tack direction.
                        int subMode = gf.Parameters[4].Value.GetValueOrDefault();
                        if (subMode == 4 || subMode == 3)
                        {
                            if (_currentDesiredWindAngle.Abs().Degrees < 90) // tack
                            {
                                _currentDesiredWindAngle = (-_currentDesiredWindAngle).Normalize(true);
                            }
                            else
                            {
                                // gybe
                                _currentDesiredWindAngle = (Angle.FromDegrees(360) - _currentDesiredWindAngle).Normalize(true);
                            }
                        }
                    }
                    else
                    {
                        _currentAutopilotStatus = SeatalkNgPilotStatus.AutopilotStatusFromNumber(newMode);
                        Console.WriteLine($"New status was commanded: {_currentAutopilotStatus}!");
                        var reply = gf.CreateAck();
                        parser.SendSentence(reply);
                    }
                }
                else if (gf.Pgn == SeatalkNgPilotLockedHeading.HexId && gf.ParameterConstantsMatch() &&
                              gf.Parameters[0].Constant == 1851 && gf.Function == GroupFunction.Command)
                {
                    double newDirection = gf.Parameters[5].Value.GetValueOrDefault(); // New magnetic heading
                    _currentDesiredHeading = Angle.FromRadians(newDirection * 0.0001).ToUnit(AngleUnit.Degree);
                    Console.WriteLine($"Updated desired heading to {_currentDesiredHeading}");
                    var reply = gf.CreateAck();
                    parser.SendSentence(reply);
                }
                else if (gf.Pgn == SeatalkNgPilotWindStatus.HexId && gf.ParameterConstantsMatch() &&
                              gf.Parameters[0].Constant == 1851 && gf.Function == GroupFunction.Command)
                {
                    double newWindAngle = gf.Parameters[3].Value.GetValueOrDefault(); // New wind angle
                    _currentDesiredWindAngle = Angle.FromRadians(newWindAngle * 0.0001).ToUnit(AngleUnit.Degree);
                    Console.WriteLine($"Updated desired wind angle to {_currentDesiredWindAngle}");
                    var reply = gf.CreateAck();
                    parser.SendSentence(reply);
                }
                else if (gf.Pgn == 126720 && gf.ParameterConstantsMatch() && gf.Function == GroupFunction.Request)
                {
                    int prop = gf.Parameters[3].Value.GetValueOrDefault();
                    int command = gf.Parameters[4].Value.GetValueOrDefault();
                    Console.WriteLine($"Someone is requesting the following value: Proprietary ID {prop} and Command {command}");
                    if (prop == 108 && command == 38)
                    {
                        SeatalkNgPilotConfigurationValue value = new SeatalkNgPilotConfigurationValue()
                        {
                            Command = 38,
                            ProprietaryId = 108,
                            DateTime = DateTimeOffset.UtcNow,
                            Value = false,
                        };

                        parser.SendSentence(value);
                    }
                    else
                    {
                        var reply = gf.CreateNoAck(x => x.Description == "Command" ? 5 : null);
                        parser.SendSentence(reply);
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown Group function '{gf.Function}' message about {gf.Pgn}");
                }
            }
        }
    }
}

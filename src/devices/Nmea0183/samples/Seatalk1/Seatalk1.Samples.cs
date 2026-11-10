// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using System.IO.Ports;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Seatalk1;
using Iot.Device.Seatalk1.Messages;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Seatalk1Sample
{
    internal class Program
    {
        private NmeaParser? _seatalk;
        private AutoPilotRemoteController? _controller;

        internal static int Main(string[] args)
        {
            Console.WriteLine("Hello Seatalk1 Sample!");

            if (args.Length == 0)
            {
                Console.WriteLine("Error: Port not specified");
                return 1;
            }

            var p = new Program();
            p.Run(args);
            return 0;
        }

        public async void Run(string[] args)
        {
            LogDispatcher.LoggerFactory = new SimpleConsoleLoggerFactory(LogLevel.Trace);

            _seatalk = new Seatalk1Parser("Seatalk1", args[0]);

            _controller = new AutoPilotRemoteController("Autopilot", "Seatalk1");

            _seatalk.OnNewSequence += ParserOnNewMessageDecoded;

            _seatalk.StartDecode();

            WriteCurrentState();

            Angle windAngle = Angle.Zero;

            TurnDirection? directionConfirmation = null;

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        break;
                    }

                    Keystroke ks = new Keystroke();
                    switch (key.Key)
                    {
                        case ConsoleKey.A:
                            ks = new Keystroke(AutopilotButtons.MinusOne);
                            break;
                        case ConsoleKey.D:
                            ks = new Keystroke(AutopilotButtons.PlusOne);
                            break;
                        case ConsoleKey.Q:
                            ks = new Keystroke(AutopilotButtons.MinusTen);
                            break;
                        case ConsoleKey.E:
                            ks = new Keystroke(AutopilotButtons.PlusTen);
                            break;
                        case ConsoleKey.W:
                        {
                            if (_controller.SetStatus(AutopilotStatus.Auto, ref directionConfirmation))
                            {
                                Console.WriteLine("Autopilot set to AUTO mode");
                            }
                            else
                            {
                                Console.WriteLine("Setting AUTO mode FAILED!");
                            }

                            break;
                        }

                        case ConsoleKey.X:
                        {
                            // This is expected to fail if the AP has no wind data
                            if (_controller.SetStatus(AutopilotStatus.Wind, ref directionConfirmation))
                            {
                                Console.WriteLine("Autopilot set to WIND mode");
                            }
                            else
                            {
                                Console.WriteLine("Setting WIND mode FAILED!");
                            }

                            break;
                        }

                        case ConsoleKey.U:
                        {
                            _controller.TurnBy(Angle.FromDegrees(90), TurnDirection.TurnToStarboard);

                            break;
                        }

                        case ConsoleKey.Z:
                        {
                            _controller.TurnBy(Angle.FromDegrees(90), TurnDirection.TurnToPort);

                            break;
                        }

                        case ConsoleKey.K:
                            if (_controller.DeadbandMode == DeadbandMode.Automatic)
                            {
                                _controller.SetDeadbandMode(DeadbandMode.Minimal);
                            }
                            else
                            {
                                _controller.SetDeadbandMode(DeadbandMode.Automatic);
                            }

                            break;

                        case ConsoleKey.T:
                        {
                            // This is expected to fail if the AP has no wind data
                            if (_controller.SetStatus(AutopilotStatus.Track, ref directionConfirmation))
                            {
                                Console.WriteLine("Autopilot set to TRACK mode");
                                directionConfirmation = null;
                            }
                            else
                            {
                                Console.WriteLine("Setting TRACK mode incomplete!");
                                if (directionConfirmation == TurnDirection.TurnToPort)
                                {
                                    Console.WriteLine("Press T again to confirm a turn to Port");
                                }
                                else
                                {
                                    Console.WriteLine("Press T again to confirm a turn to Starboard");
                                }
                            }

                            break;
                        }

                        case ConsoleKey.S:
                            directionConfirmation = null;
                            if (_controller.SetStatus(AutopilotStatus.Standby, ref directionConfirmation))
                            {
                                Console.WriteLine("Autopilot set to STANDBY mode");
                            }

                            break;
                        case ConsoleKey.L:
                            if (key.Modifiers == ConsoleModifiers.Shift)
                            {
                                var intensity = new SetLampIntensity(DisplayBacklightLevel.Off);
                                _seatalk.SendSentence(intensity);
                            }
                            else
                            {
                                var intensity = new SetLampIntensity(DisplayBacklightLevel.Level3);
                                _seatalk.SendSentence(intensity);
                            }

                            break;

                    }

                    if (ks.ButtonsPressed != AutopilotButtons.None)
                    {
                        _seatalk.SendSentence(ks);
                    }
                }

                await Task.Delay(500);

                NavigationToWaypoint wp = new NavigationToWaypoint()
                {
                    BearingToDestination = Angle.FromDegrees(10),
                    CrossTrackError = Length.FromNauticalMiles(-0.11),
                    DistanceToDestination = Length.FromNauticalMiles(51.3),
                };

                _seatalk.SendSentence(wp);

                windAngle = windAngle + Angle.FromDegrees(1.5);
                // SendRandomWindAngle(windAngle);
                WriteCurrentState();
            }

            _seatalk.Dispose();
            _seatalk = null;

            Console.WriteLine("Program is terminating");
        }

        private void WriteCurrentState()
        {
            if (_controller != null)
            {
                Console.Write("\r");
                Console.Write(_controller.ToString());
            }
        }

        private void ParserOnNewMessageDecoded(NmeaSinkAndSource nmeaSinkAndSource, NmeaSentence nmeaSentence)
        {
            if (nmeaSentence is Keystroke keystroke)
            {
                Console.WriteLine();
                Console.WriteLine($"Pressed key(s): {keystroke}");
            }

            _controller?.SendSentence(nmeaSentence);
        }
    }
}

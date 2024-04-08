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
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Seatalk1;
using Iot.Device.Seatalk1.Messages;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Seatalk1Sample
{
    internal class Program
    {
        private SeatalkInterface? _seatalk;

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

            _seatalk = new SeatalkInterface(args[0]);

            _seatalk.MessageReceived += ParserOnNewMessageDecoded;

            _seatalk.StartDecode();

            var ctrl = _seatalk.GetAutopilotRemoteController();

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
                            if (ctrl.SetStatus(AutopilotStatus.Auto, ref directionConfirmation))
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
                            if (ctrl.SetStatus(AutopilotStatus.Wind, ref directionConfirmation))
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
                            await ctrl.TurnByAsync(Angle.FromDegrees(90), TurnDirection.Starboard, CancellationToken.None);

                            break;
                        }

                        case ConsoleKey.Z:
                        {
                            await ctrl.TurnByAsync(Angle.FromDegrees(90), TurnDirection.Port, CancellationToken.None);

                            break;
                        }

                        case ConsoleKey.K:
                            if (ctrl.DeadbandMode == DeadbandMode.Automatic)
                            {
                                ctrl.SetDeadbandMode(DeadbandMode.Minimal);
                            }
                            else
                            {
                                ctrl.SetDeadbandMode(DeadbandMode.Automatic);
                            }

                            break;

                        case ConsoleKey.T:
                        {
                            // This is expected to fail if the AP has no wind data
                            if (ctrl.SetStatus(AutopilotStatus.Track, ref directionConfirmation))
                            {
                                Console.WriteLine("Autopilot set to TRACK mode");
                                directionConfirmation = null;
                            }
                            else
                            {
                                Console.WriteLine("Setting TRACK mode incomplete!");
                                if (directionConfirmation == TurnDirection.Port)
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
                            if (ctrl.SetStatus(AutopilotStatus.Standby, ref directionConfirmation))
                            {
                                Console.WriteLine("Autopilot set to STANDBY mode");
                            }

                            break;
                        case ConsoleKey.L:
                            if (key.Modifiers == ConsoleModifiers.Shift)
                            {
                                _seatalk.SetLampIntensity(DisplayBacklightLevel.Off);
                            }
                            else
                            {
                                _seatalk.SetLampIntensity(DisplayBacklightLevel.Level3);
                            }

                            break;

                    }

                    if (ks.ButtonsPressed != AutopilotButtons.None)
                    {
                        _seatalk.SendMessage(ks);
                    }
                }

                await Task.Delay(500);

                NavigationToWaypoint wp = new NavigationToWaypoint()
                {
                    BearingToDestination = Angle.FromDegrees(10),
                    CrossTrackError = Length.FromNauticalMiles(-0.11),
                    DistanceToDestination = Length.FromNauticalMiles(51.3),
                };

                _seatalk.SendMessage(wp);

                windAngle = windAngle + Angle.FromDegrees(1.5);
                // SendRandomWindAngle(windAngle);
                WriteCurrentState();
            }

            _seatalk.Dispose();
            _seatalk = null;

            Console.WriteLine("Program is terminating");
        }

        private void SendRandomWindAngle(Angle angle)
        {
            var windMsg = new ApparentWindAngle(angle);
            _seatalk?.SendMessage(windMsg);
        }

        private void WriteCurrentState()
        {
            var ctrl = _seatalk?.GetAutopilotRemoteController();
            if (ctrl != null)
            {
                Console.Write("\r");
                Console.Write(ctrl.ToString());
            }
        }

        private void ParserOnNewMessageDecoded(SeatalkMessage obj)
        {
            if (obj is Keystroke keystroke)
            {
                Console.WriteLine();
                Console.WriteLine($"Pressed key(s): {keystroke}");
            }
        }
    }
}

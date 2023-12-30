// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using System.IO.Ports;
using Iot.Device.Common;
using Iot.Device.Seatalk1;
using Iot.Device.Seatalk1.Messages;
using Microsoft.Extensions.Logging;

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

        public void Run(string[] args)
        {
            LogDispatcher.LoggerFactory = new SimpleConsoleLoggerFactory(LogLevel.Trace);

            _seatalk = new SeatalkInterface(args[0]);

            _seatalk.MessageReceived += ParserOnNewMessageDecoded;

            var ctrl = _seatalk.GetAutopilotRemoteController();

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
                            if (ctrl.SetStatus(AutopilotStatus.Auto))
                            {
                                Console.WriteLine("Autopilot set to AUTO mode");
                            }
                            else
                            {
                                Console.WriteLine("Setting AUTO mode FAILED!");
                            }

                            break;
                        }

                        case ConsoleKey.L:
                        {
                            // Doesn't seem to do anything on the ST2000+, even though it is documented that remote-controlling the display backlight should work
                            ks = new Keystroke(AutopilotButtons.Disp);
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

                        case ConsoleKey.I:
                        {
                            // This is expected to fail if the AP has no wind data
                            if (ctrl.SetStatus(AutopilotStatus.Wind))
                            {
                                Console.WriteLine("Autopilot set to WIND mode");
                            }
                            else
                            {
                                Console.WriteLine("Setting WIND mode FAILED!");
                            }

                            break;
                        }

                        case ConsoleKey.S:
                            if (ctrl.SetStatus(AutopilotStatus.Standby))
                            {
                                Console.WriteLine("Autopilot set to STANDBY mode");
                            }

                            break;
                    }

                    if (ks.ButtonsPressed != AutopilotButtons.None)
                    {
                        _seatalk.SendMessage(ks);
                    }
                }

                Thread.Sleep(500);
                WriteCurrentState();
            }

            _seatalk.Dispose();
            _seatalk = null;

            Console.WriteLine("Program is terminating");
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

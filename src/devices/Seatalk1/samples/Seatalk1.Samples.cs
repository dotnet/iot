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

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                    else if (key.Key == ConsoleKey.A)
                    {
                        // For testing only
                        byte[] keyPlus1 = new byte[]
                        {
                            0x86, 0x11, 0x07, 0xf8
                        };

                        _seatalk.SendDatagram(keyPlus1);
                    }
                    else if (key.Key == ConsoleKey.B)
                    {
                        // For testing only
                        byte[] keyPlus1 = new byte[]
                        {
                            0x86, 0x11, 0x05, 0xfa
                            // 0xFF, 0xFF,
                        };

                        _seatalk.SendDatagram(keyPlus1);
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

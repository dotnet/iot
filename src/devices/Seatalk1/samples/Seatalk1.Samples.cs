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
        private CompassHeadingAndRudderPosition? _headingAndRudderPosition;
        private CompassHeadingAutopilotCourse? _autopilotCourse;

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

            SerialPort port1 = new SerialPort(args[0]);
            port1.BaudRate = 4800;
            port1.Parity = Parity.Even;
            port1.StopBits = StopBits.One;
            port1.DataBits = 8;
            port1.Open();

            Seatalk1Parser parser = new Seatalk1Parser(port1.BaseStream);
            parser.StartDecode();

            parser.NewMessageDecoded += ParserOnNewMessageDecoded;

            while (!Console.KeyAvailable)
            {
                Thread.Sleep(500);
            }

            parser.StopDecode();
            Console.ReadKey(true);

            Console.WriteLine("Program is terminating");
            parser.Dispose();
            port1.Close();
        }

        private void WriteCurrentState()
        {
            if (_headingAndRudderPosition == null || _autopilotCourse == null)
            {
                return;
            }

            Console.Write("\r");
            Console.Write($"MAG: {_headingAndRudderPosition.CompassHeading} TRK: {_autopilotCourse.AutoPilotCourse} " +
                          $"STAT: {_autopilotCourse.AutopilotStatus} RUDDER: {_autopilotCourse.RudderPosition} ALRT: {_autopilotCourse.Alarms}   ");
        }

        private void ParserOnNewMessageDecoded(SeatalkMessage obj)
        {
            if (obj is CompassHeadingAutopilotCourse ch)
            {
                _autopilotCourse = ch;
                WriteCurrentState();
            }
            else if (obj is CompassHeadingAndRudderPosition rb)
            {
                _headingAndRudderPosition = rb;
                WriteCurrentState();
            }
            else if (obj is Keystroke keystroke)
            {
                Console.WriteLine();
                Console.WriteLine($"Pressed key(s): {keystroke}");
            }
        }
    }
}

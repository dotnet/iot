// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Arduino;
using Iot.Device.Common;
using UnitsNet;

namespace BlinkingLed
{
    internal class Program
    {
        // The pin to which the LED is connected.
        private const int RedLed = 16;

        private readonly ArduinoBoard _board;
        private GpioController _gpioController;

        private Program(ArduinoBoard board)
        {
            _board = board;
            _gpioController = _board.CreateGpioController();
            // Put initialization code here
        }

        /// <summary>
        /// This is the main method. It will be called when the program starts.
        /// The command line arguments define the connection type to the microcontroller during debugging.
        /// When the code is compiled using the ArduinoCsCompiler, the arguments provided will be empty.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Program exit code</returns>
        public static int Main(string[] args)
        {
            RunTest();
            ArduinoBoard? board;
            string portName;
            Console.WriteLine("Starting...");
            if (args.Length > 0)
            {
                portName = args[0];
            }
            else
            {
                portName = string.Empty;
            }

            if (portName == "INET")
            {
                // pass "INET <IP-Address>" as command line parameter when the microcontroller is connected via WIFI
                IPAddress address = IPAddress.Loopback;
                if (args.Length > 1)
                {
                    address = IPAddress.Parse(args[1]);
                }

                if (!ArduinoBoard.TryConnectToNetworkedBoard(address, 27016, out board))
                {
                    Console.WriteLine($"Unable to connect to board at address {address}");
                    return 1;
                }
            }
            else
            {
                // Use a serial port name like "COM5" to connect via USB.
                string[] boards = SerialPort.GetPortNames();
                if (portName != string.Empty)
                {
                    boards = new string[]
                    {
                        portName
                    };
                }

                // This will attempt to connect to a board on the given serial port(s). If the program is later running
                // on the board itself (after uploading), this will return an object that emulates the same functionality
                // on the board itself. The parameters will be ignored in that case.
                if (!ArduinoBoard.TryFindBoard(boards, new List<int>() { 115200 }, out board))
                {
                    Console.WriteLine($"Unable to connect to board at any of {String.Join(", ", boards)}");
                    return 1;
                }
            }

            try
            {
                Console.WriteLine("Successfully connected");
                Program ws = new(board);
                return ws.Run();
            }
            finally
            {
                board.Dispose();
            }
        }

        private static void RunTest()
        {
            string v1 = ".NET 8.0.5";
            int idx1 = v1.LastIndexOf(" ", StringComparison.Ordinal);
            v1 = v1.Substring(idx1);
            Version hostVersion = Version.Parse(v1);
            Console.WriteLine(hostVersion);
        }

        /// <summary>
        /// This method shall contain your user code.
        /// Of course, you can move parts of your code to their own methods and/or classes.
        /// </summary>
        /// <returns>This method normally never returns. If you add code that lets the program end, compiler settings
        /// will define what happens if it does. By default, the application will restart.</returns>
        public int Run()
        {
            _gpioController.OpenPin(RedLed, PinMode.Output);

            // Loop forever.
            while (true)
            {
                // Setting the pin high turns the LED on
                _gpioController.Write(RedLed, PinValue.High);
                // Wait 500ms
                Thread.Sleep(500);
                // Setting the pin low turns off the LED
                _gpioController.Write(RedLed, PinValue.Low);
                Thread.Sleep(500);
            }
        }
    }
}

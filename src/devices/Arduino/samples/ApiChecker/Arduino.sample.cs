// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Iot.Device.Adc;
using Iot.Device.Arduino;
using Iot.Device.Arduino.Sample;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Common;
using Iot.Device.Board;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using UnitsNet;

namespace Arduino.Samples
{
    /// <summary>
    /// Test application for Arduino/Firmata protocol
    /// </summary>
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: Arduino.sample <PortName>");
                Console.WriteLine("i.e.: Arduino.sample COM4");
                return;
            }

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(lg =>
                {
                    lg.LogToStandardErrorThreshold = LogLevel.Error;
                });
            });

            // Statically register our factory. Note that this must be done before instantiation of any class that wants to use logging.
            LogDispatcher.LoggerFactory = loggerFactory;

            string portName = args[0];
            if (args.Length > 0)
            {
                portName = args[0];
            }
            else
            {
                portName = "INET";
            }

            if (portName == "INET")
            {
                IPAddress address = IPAddress.Loopback;
                if (args.Length > 1)
                {
                    try
                    {
                        var ip = Dns.GetHostAddresses(args[1], AddressFamily.InterNetwork);
                        if (ip.Any())
                        {
                            address = ip.First();
                        }
                    }
                    catch (SocketException x)
                    {
                        Console.WriteLine($"Unable to resolve address {args[0]}: {x.Message}");
                        return;
                    }
                }

                ConnectToSocket(address);
                return;
            }

            using (var port = new SerialPort(portName, 115200))
            {
                Console.WriteLine($"Connecting to Arduino on {portName}");
                try
                {
                    port.Open();
                }
                catch (UnauthorizedAccessException x)
                {
                    Console.WriteLine($"Could not open COM port: {x.Message} Possible reason: Arduino IDE connected or serial console open");
                    return;
                }

                ConnectWithStream(port.BaseStream);
            }
        }

        private static void ConnectWithStream(Stream stream)
        {
            ArduinoBoard board = new ArduinoBoard(stream);
            try
            {
                Console.WriteLine(
                    $"Connection successful. Firmware version: {board.FirmwareVersion}, Builder: {board.FirmwareName}");
                TestCases.Run(board);
            }
            catch (TimeoutException x)
            {
                Console.WriteLine($"No answer from board: {x.Message} ");
            }
            finally
            {
                stream.Close();
                board?.Dispose();
            }
        }

        private static void ConnectToSocket(IPAddress address)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(address, 27016);
            s.NoDelay = true;
            using (NetworkStream ns = new NetworkStream(s, true))
            {
                ConnectWithStream(ns);
            }
        }

        private static void BoardOnLogMessages(string message, Exception? exception)
        {
            Console.WriteLine("Log message: " + message);
            if (exception != null)
            {
                Console.WriteLine(exception);
            }
        }
    }
}

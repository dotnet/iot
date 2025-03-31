// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Iot.Device.Arduino;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    public class FirmataTestFixture : IDisposable
    {
        private NetworkStream? _networkStream;
        private Socket? _socket;

        public FirmataTestFixture()
        {
            try
            {
                var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddProvider(new DebuggerOutputLoggerProvider())
                        .SetMinimumLevel(LogLevel.Debug);
                });

                // Statically register our factory. Note that this must be done before instantiation of any class that wants to use logging.
                LogDispatcher.LoggerFactory = loggerFactory;
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(IPAddress.Loopback, 27016);
                _socket.NoDelay = true;
                _networkStream = new NetworkStream(_socket, true);
                Board = new ArduinoBoard(_networkStream);
                if (!(Board.FirmataVersion > new Version(1, 0)))
                {
                    // Actually not expecting to get here (but the above will throw a SocketException if the remote end is not there)
                    throw new NotSupportedException("Very old firmware found");
                }

                return;
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to connect to simulator, trying hardware...");
            }

            if (!ArduinoBoard.TryFindBoard(SerialPort.GetPortNames(), new List<int>() { 115200 }, out var board))
            {
                Board = null;
                return;
            }

            Board = board;
        }

        public ArduinoBoard? Board
        {
            get;
        }

        protected virtual void Dispose(bool disposing)
        {
            Board?.Dispose();
            if (_networkStream != null)
            {
                _networkStream.Dispose();
                _networkStream = null;
            }

            if (_socket != null)
            {
                _socket.Close();
                _socket.Dispose();
                _socket = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}

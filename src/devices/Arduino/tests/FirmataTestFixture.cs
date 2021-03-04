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
using Xunit;

namespace Arduino.Tests
{
    public class FirmataTestFixture : IDisposable
    {
        private NetworkStream? _networkStream;
        private Socket? _socket;

        public FirmataTestFixture()
        {
            try
            {
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

                Board.LogMessages += (x, y) =>
                {
                    Debug.WriteLine(x);
                    Console.WriteLine(x);
                };

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
            Board.LogMessages += (x, y) => Console.WriteLine(x);
        }

        public ArduinoBoard? Board
        {
            get;
        }

        /// <summary>
        /// Default settings for unit tests
        /// </summary>
        public CompilerSettings DefaultCompilerSettings
        {
            get
            {
                return new CompilerSettings()
                {
                    CreateKernelForFlashing = false, UseFlashForKernel = false, UseFlashForProgram = true,
                };
            }
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

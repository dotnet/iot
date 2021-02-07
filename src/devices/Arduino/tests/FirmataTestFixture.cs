using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using Iot.Device.Arduino;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    public class FirmataTestFixture : IDisposable
    {
        private NetworkStream? _networkStream;
        private Socket? _socket;

        public FirmataTestFixture()
        {
            var typeOfType = Type.GetType("System.Type", true)!;
            var attribute = (TargetFrameworkAttribute)typeOfType.Assembly.GetCustomAttribute(typeof(TargetFrameworkAttribute))!;
            var versionStr = attribute.FrameworkName.Substring(attribute.FrameworkName.IndexOf("=v", StringComparison.Ordinal) + 2);
            Version version = Version.Parse(versionStr);
            Assert.True(version >= new Version(5, 0));
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(IPAddress.Loopback, 27016);
                _socket.NoDelay = true;
                _networkStream = new NetworkStream(_socket, true);
                Board = new ArduinoBoard(_networkStream);
                Board.Initialize();
                Board.LogMessages += LogMessage;

                return;
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to connect to simulator, trying hardware...");
            }

            var b = ArduinoBoard.FindBoard(ArduinoBoard.GetSerialPortNames(), new List<int>() { 115200 });
            if (b == null)
            {
                throw new NotSupportedException("No board found");
            }

            Board = b;
            Board.LogMessages += LogMessage;
        }

        public ArduinoBoard Board
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
                    CreateKernelForFlashing = false, UseFlash = false
                };
            }
        }

        private void LogMessage(string x, Exception? y)
        {
            Console.WriteLine(x);
            Debug.WriteLine(x);
        }

        protected virtual void Dispose(bool disposing)
        {
            Board.Dispose();
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

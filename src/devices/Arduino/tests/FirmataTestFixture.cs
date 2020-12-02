using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Iot.Device.Arduino;

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
                _socket.Connect(IPAddress.Loopback, 27015);
                _socket.NoDelay = true;
                _networkStream = new NetworkStream(_socket, true);
                Board = new ArduinoBoard(_networkStream);
                Board.Initialize();
                Board.LogMessages += (x, y) => Console.WriteLine(x);

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
            Board.LogMessages += (x, y) => Console.WriteLine(x);
        }

        public ArduinoBoard Board
        {
            get;
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

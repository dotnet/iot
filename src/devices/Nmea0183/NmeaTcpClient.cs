// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// A TCP Server bidirectional sink and source. Provides NMEA sentences to each connected client.
    /// </summary>
    public class NmeaTcpClient : NmeaSinkAndSource
    {
        private readonly string _destination;
        private readonly int _port;

        private TcpClient? _client;
        private NmeaParser? _parser;
        private Thread? _connectionThread;
        private bool _terminated;
        private ILogger _logger;
        private bool _connectionActive;

        /// <summary>
        /// Creates a server with the given source name bound to the given local IP and port.
        /// This will not open the server yet. Use <see cref="StartDecode"/> to open the network port.
        /// </summary>
        /// <param name="name">Source name</param>
        /// <param name="destination">Remote host to connect to</param>
        /// <param name="port">The network port to use</param>
        public NmeaTcpClient(string name, string destination, int port = 10110)
        : base(name)
        {
            _destination = destination;
            _port = port;
            _connectionActive = false;
            RetryInterval = TimeSpan.FromSeconds(5);
            _logger = this.GetCurrentClassLogger();
        }

        /// <summary>
        /// Time between reconnection attempts. Default 5 seconds.
        /// </summary>
        public TimeSpan RetryInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Returns true if this client is connected
        /// </summary>
        public bool Connected => _client != null && _client.Connected && _connectionActive;

        /// <summary>
        /// Starts connecting to the server. A failure to connect will not cause an exception. Retries will be handled
        /// automatically.
        /// </summary>
        /// <exception cref="InvalidOperationException">The method was called twice</exception>
        public override void StartDecode()
        {
            if (_connectionThread != null)
            {
                throw new InvalidOperationException("Server already started");
            }

            _terminated = false;
            _connectionThread = new Thread(ConnectionWatcher);
            _connectionThread.Start();
        }

        private void ConnectionWatcher()
        {
            while (!_terminated && _connectionThread != null)
            {
                try
                {
                    var client = new TcpClient(_destination, _port);
                    _connectionActive = true;
                    _logger.LogInformation($"{InterfaceName}: Connected to {_destination}:{_port}");
                    var parser = new NmeaParser($"{InterfaceName}: Connected to {_destination}:{_port}", client.GetStream(), client.GetStream());
                    parser.OnNewSequence += OnSentenceReceivedFromServer;
                    parser.OnParserError += ParserOnParserError;
                    _client = client;
                    _parser = parser;
                    parser.StartDecode();

                    while (Connected && !_terminated)
                    {
                        Thread.Sleep(RetryInterval);
                    }

                    if (_parser != null)
                    {
                        _parser.Dispose();
                        _parser = null;
                    }

                    client.Dispose(); // Probably disconnected or we're going down
                }
                catch (SocketException)
                {
                    // Retry
                    Thread.Sleep(RetryInterval);
                    _connectionActive = false;
                }
            }
        }

        private void ParserOnParserError(NmeaSinkAndSource source, string message, NmeaError errorCode)
        {
            if (errorCode == NmeaError.PortClosed)
            {
                _connectionActive = false;
            }

            FireOnParserError(message, errorCode);
        }

        private void OnSentenceReceivedFromServer(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            DispatchSentenceEvents(sentence);
        }

        /// <summary>
        /// Sends the sentence to all our clients.
        /// If it is needed to make distinctions for what needs to be sent to which client, create
        /// multiple server instances. This will allow for proper filtering.
        /// </summary>
        /// <param name="source">The original source of the message, used i.e. for logging</param>
        /// <param name="sentence">The sentence to send</param>
        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            try
            {
                _parser?.SendSentence(source, sentence);
            }
            catch (IOException x)
            {
                FireOnParserError($"Error sending message to {InterfaceName}: {x.Message}", NmeaError.PortClosed);
            }
        }

        /// <inheritdoc />
        public override void StopDecode()
        {
            _terminated = true;
            if (_connectionThread != null)
            {
                _connectionThread.Join();
                _connectionThread = null;
            }

            // Just to make sure
            _parser = null;
            _client = null;
        }
    }
}

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
        private readonly Func<(string Destination, int Port)>? _discoveryFunc;
        private readonly INmeaParserFactory _parserFactory;

        private string? _destination;
        private int _port;
        private TcpClient? _client;
        private NmeaParser? _parser;
        private Thread? _connectionThread;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _connectionActive;

        /// <summary>
        /// Creates a client that connects to the given destination and port. If the connection is not
        /// possible or is lost intermittently, it will be retried repeatedly.
        /// You need to call <see cref="StartDecode"/> to actually start receiving data.
        /// </summary>
        /// <param name="name">Source name</param>
        /// <param name="destination">Remote host to connect to</param>
        /// <param name="port">The network port to use</param>
        public NmeaTcpClient(string name, string destination, int port = 10110)
        : this(name, destination, port, new Nmea0183ParserFactory())
        {
        }

        /// <summary>
        /// Creates a client that connects to the given destination and port. If the connection is not
        /// possible or is lost intermittently, it will be retried repeatedly.
        /// You need to call <see cref="StartDecode"/> to actually start receiving data.
        /// </summary>
        /// <param name="name">Source name</param>
        /// <param name="destination">Remote host to connect to</param>
        /// <param name="port">The network port to use</param>
        /// <param name="parserFactory">The parser to use for this connection</param>
        public NmeaTcpClient(string name, string destination, int port, INmeaParserFactory parserFactory)
            : base(name)
        {
            _destination = destination;
            _port = port;
            _parserFactory = parserFactory;
            _connectionActive = false;
            _cancellationTokenSource = new CancellationTokenSource();
            RetryInterval = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Creates a client that connects to a destination and port. This constructor
        /// allows specifying a discovery function that will be called to obtain the destination address/port before
        /// an attempt to connect.
        /// If the connection is not
        /// possible or is lost intermittently, it will be retried repeatedly.
        /// You need to call <see cref="StartDecode"/> to actually start receiving data.
        /// </summary>
        /// <param name="name">Source name</param>
        /// <param name="discoveryFunc">Function that should deliver the address of the destination.
        /// Returning an empty string will just cause another try later.</param>
        /// <param name="parserFactory">The parser to use</param>
        public NmeaTcpClient(string name, Func<(string Destination, int Port)> discoveryFunc,
            INmeaParserFactory parserFactory)
            : base(name)
        {
            _discoveryFunc = discoveryFunc;
            _parserFactory = parserFactory;
            _connectionActive = false;
            _cancellationTokenSource = new CancellationTokenSource();
            RetryInterval = TimeSpan.FromSeconds(5);
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

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                throw new InvalidOperationException("This client is already terminated");
            }

            _connectionThread = new Thread(ConnectionWatcher);
            _connectionThread.Start();
        }

        private void ConnectionWatcher()
        {
            while (!_cancellationTokenSource.IsCancellationRequested && _connectionThread != null)
            {
                if (_discoveryFunc != null)
                {
                    (_destination, _port) = _discoveryFunc();
                }

                try
                {
                    if (string.IsNullOrEmpty(_destination))
                    {
                        _cancellationTokenSource.Token.WaitHandle.WaitOne(RetryInterval);
                        continue;
                    }

                    var client = new TcpClient(_destination, _port);
                    _connectionActive = true;
                    Logger.LogInformation($"{InterfaceName}: Connected to {_destination}:{_port}");
                    var parser = _parserFactory.CreateParser($"{InterfaceName}: Connected to {_destination}:{_port}", client.GetStream(), client.GetStream());
                    parser.OnNewSequence += OnSentenceReceivedFromServer;
                    parser.OnParserError += ParserOnParserError;
                    _client = client;
                    _parser = parser;
                    parser.StartDecode();

                    while (Connected && !_cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource.Token.WaitHandle.WaitOne(RetryInterval);
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
                    _cancellationTokenSource.Token.WaitHandle.WaitOne(RetryInterval);
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
            Logger.LogInformation($"Tcp Client {InterfaceName} is terminating");
            _cancellationTokenSource.Cancel();
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

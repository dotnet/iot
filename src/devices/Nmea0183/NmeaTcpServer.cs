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
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// A TCP Server bidirectional sink and source. Provides NMEA sentences to each connected client.
    /// </summary>
    /// <remarks>
    /// Using this class in applications targeting .NET core 3.1 or earlier may cause deadlocks when closing the connection.
    /// It is recommended to target .NET 6.0 or above when using this class.
    /// </remarks>
    public class NmeaTcpServer : NmeaSinkAndSource
    {
        private readonly IPAddress _bindTo;
        private readonly int _port;
        private readonly List<NmeaSinkAndSource> _activeParsers;
        private readonly object _lock;
        private TcpListener? _server;
        private Thread? _serverThread;
        private Thread? _serverControlThread;
        private AutoResetEvent _serverControlEvent;
        private ConcurrentQueue<Task> _serverTasks;
        private bool _terminated;

        /// <summary>
        /// Creates a server with the given source name. The default network port is 10110.
        /// </summary>
        /// <param name="name">Source name</param>
        public NmeaTcpServer(string name)
        : this(name, IPAddress.Any, 10110)
        {
        }

        /// <summary>
        /// Creates a server with the given source name bound to the given local IP and port.
        /// This will not open the server yet. Use <see cref="StartDecode"/> to open the network port.
        /// </summary>
        /// <param name="name">Source name</param>
        /// <param name="bindTo">Network interface to bind to (Use <see cref="IPAddress.Any"/> to bind to all available interfaces</param>
        /// <param name="port">The network port to use</param>
        public NmeaTcpServer(string name, IPAddress bindTo, int port)
        : base(name)
        {
            _bindTo = bindTo;
            _port = port;
            _activeParsers = new List<NmeaSinkAndSource>();
            _lock = new object();
            _serverControlEvent = new AutoResetEvent(false);
            _serverTasks = new ConcurrentQueue<Task>();
        }

        /// <summary>
        /// Starts a network server with the settings provided by the constructor.
        /// </summary>
        /// <exception cref="InvalidOperationException">The server was already started</exception>
        /// <exception cref="SocketException">The network port is already in use</exception>
        public override void StartDecode()
        {
            if (_serverThread != null)
            {
                throw new InvalidOperationException("Server already started");
            }

            _terminated = false;
            _server = new TcpListener(_bindTo, _port);
            _server.Start();
            _serverThread = new Thread(ConnectionWatcher);
            _serverThread.Name = "Server connection watcher";
            _serverThread.Start();

            _serverControlThread = new Thread(ServerControl);
            _serverControlThread.Name = "Server control thread";
            _serverControlThread.Start();
        }

        private void ConnectionWatcher()
        {
            while (!_terminated && _server != null)
            {
                try
                {
                    var client = _server.AcceptTcpClient();
                    lock (_lock)
                    {
                        NmeaParser parser = new NmeaParser($"{InterfaceName}: {_activeParsers.Count}", client.GetStream(), client.GetStream());
                        parser.OnNewSequence += OnSentenceReceivedFromClient;
                        parser.OnParserError += ParserOnParserError;
                        parser.StartDecode();

                        _activeParsers.Add(parser);
                    }
                }
                catch (SocketException)
                {
                    // Ignore (probably going to close the socket)
                }
            }
        }

        private void ServerControl()
        {
            while (!_terminated)
            {
                _serverControlEvent.WaitOne();
                if (_serverTasks.TryDequeue(out Task? task))
                {
                    // Just wait for this to terminate
                    task.Wait();
                }
            }
        }

        private void ParserOnParserError(NmeaSinkAndSource source, string message, NmeaError errorCode)
        {
            if (errorCode == NmeaError.PortClosed)
            {
                Task t = Task.Run(() =>
                {
                    lock (_lock)
                    {
                        _activeParsers.Remove(source);
                    }

                    // Can't do this synchronously, as it would cause a deadlock
                    source.StopDecode();
                });
                _serverTasks.Enqueue(t);
                _serverControlEvent.Set();
            }

            FireOnParserError(message, errorCode);
        }

        private void OnSentenceReceivedFromClient(NmeaSinkAndSource source, NmeaSentence sentence)
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
            lock (_activeParsers)
            {
                // Clone, because in case of an error, the list may change
                var parsers = _activeParsers.ToList();
                foreach (var parser in parsers)
                {
                    try
                    {
                        parser.SendSentence(source, sentence);
                    }
                    catch (IOException x)
                    {
                        FireOnParserError($"Error sending message to {parser.InterfaceName}: {x.Message}", NmeaError.PortClosed);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void StopDecode()
        {
            _terminated = true;
            if (_server != null && _serverThread != null && _serverControlThread != null)
            {
                _server.Stop();
                _serverThread.Join();
                _serverControlEvent.Set();
                _serverControlThread.Join();
            }

            while (_serverTasks.TryDequeue(out Task? task))
            {
                // Just wait for this to terminate
                task.Wait();
            }

            lock (_lock)
            {
                foreach (var parser in _activeParsers)
                {
                    parser.StopDecode();
                    parser.Dispose();
                }

                _activeParsers.Clear();
            }

            _serverThread = null;
            _server = null;
            _serverControlThread = null;
        }
    }
}

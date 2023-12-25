// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Seatalk1
{
    /// <summary>
    /// Decodes a Seatalk1 Message stream
    /// </summary>
    public sealed class Seatalk1Parser : IDisposable
    {
        private readonly Stream _inputStream;
        private readonly BinaryReader _reader;
        private readonly object _lock;
        private readonly List<byte> _buffer;
        private readonly ILogger _logger;

        private Thread? _parserThread;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Seatalk1Parser"/> class.
        /// </summary>
        /// <param name="inputStream">The input stream. Must be non-null and readable</param>
        public Seatalk1Parser(Stream inputStream)
        {
            _lock = new object();
            _buffer = new List<byte>(20);
            _disposed = false;
            _logger = this.GetCurrentClassLogger();
            _inputStream = inputStream ?? throw new ArgumentNullException(nameof(inputStream));
            if (!inputStream.CanRead)
            {
                throw new ArgumentException("Input stream is not readable");
            }

            _reader = new BinaryReader(inputStream, Encoding.UTF8);
        }

        /// <summary>
        /// Starts the thread to decode packets
        /// </summary>
        /// <exception cref="InvalidOperationException">The parser is already running</exception>
        /// <exception cref="ObjectDisposedException">The parser has already been disposed. The parser cannot currently be restarted.</exception>
        public void StartDecode()
        {
            lock (_lock)
            {
                if (_parserThread != null && _parserThread.IsAlive)
                {
                    throw new InvalidOperationException("Parser thread already started");
                }

                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(Seatalk1Parser), "The parser has already been disposed, please reopen the source stream");
                }

                _cancellationTokenSource = new CancellationTokenSource();
                _parserThread = new Thread(Parser);
                _parserThread.Name = $"Seatalk1 Parser";
                _parserThread.Start();
            }
        }

        /// <summary>
        /// Terminates the decoding thread.
        /// </summary>
        public void StopDecode()
        {
            _cancellationTokenSource?.Cancel();

            lock (_lock)
            {
                _reader.Close();
                _inputStream.Close();
            }

            _parserThread?.Join();
        }

        private void Parser()
        {
            _logger.LogInformation("Seatalk1 decoder started");
            if (_cancellationTokenSource == null)
            {
                return;
            }

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    while (true)
                    {
                        byte nextByte = _reader.ReadByte();
                        _buffer.Add(nextByte);
                        int msgLen = GetLengthOfNextMessage();
                    }
                }
                catch (Exception x) when (x is EndOfStreamException || x is IOException)
                {
                    // Ignore
                    _logger.LogError(x, $"Seatalk Parser error: {x.Message}");
                }
            }
        }

        /// <summary>
        /// Returns the length of the message in the input buffer
        /// </summary>
        /// <returns>The length of the message (max 18 bytes), 0 if the message in the buffer is probably incomplete and -1 if
        /// the buffer contains invalid data or has lost sync.</returns>
        private int GetLengthOfNextMessage()
        {
            // The minimum message length is 3 bytes
            if (_buffer.Count < 3)
            {
                return 0;
            }


        }

        /// <inheritdoc />
        public void Dispose()
        {
            StopDecode();
        }
    }
}

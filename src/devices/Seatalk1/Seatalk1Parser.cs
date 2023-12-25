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
using Iot.Device.Seatalk1.Messages;
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

        private List<SeatalkMessage> _messageFactories;

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
            _messageFactories = new List<SeatalkMessage>()
            {
                new CompassHeadingAndRudderPosition(), new CompassHeadingAutopilotCourse(),
            };
        }

        public event Action<SeatalkMessage>? NewMessageDecoded;

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
                        var msg = GetTypeOfNextMessage(out int messageLength);
                        if (msg == null)
                        {
                            if (messageLength == 0)
                            {
                                break;
                            }
                            else
                            {
                                _logger.LogWarning($"Seatalk parser sync lost. Next message was 0x{_buffer[0]:X2}, now skipping");
                                _buffer.RemoveAt(0);
                                continue;
                            }
                        }

                        NewMessageDecoded?.Invoke(msg);
                    }
                }
                catch (Exception x) when (x is EndOfStreamException || x is IOException)
                {
                    // Ignore
                    _logger.LogError(x, $"Seatalk Parser error: {x.Message}");
                }

                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Returns the length of the message in the input buffer
        /// </summary>
        /// <param name="bytesInMessage">The number of bytes in the next message. Returns 0 if not enough data, -1 if sync lost (to many bytes, but no valid sentence)</param>
        /// <returns>An instance that can be used as factory for the next message or null if nothing can be decoded.</returns>
        private SeatalkMessage? GetTypeOfNextMessage(out int bytesInMessage)
        {
            // The minimum message length is 3 bytes
            if (_buffer.Count < 3)
            {
                bytesInMessage = 0;
                return null;
            }

            // The maximum length is 0xF + 3 = 18
            if (_buffer.Count > 18)
            {
                bytesInMessage = -1;
                return null;
            }

            foreach (var t in _messageFactories)
            {
                if (t.MatchesMessageType(_buffer))
                {
                    bytesInMessage = t.ExpectedLength;
                    return t.CreateNewMessage(_buffer);
                }
            }

            _logger.LogWarning($"Unable to decode message. Stream is out of sync or unknown message type 0x{_buffer[0]:X2}.");
            bytesInMessage = -1;
            return null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            StopDecode();
        }
    }
}

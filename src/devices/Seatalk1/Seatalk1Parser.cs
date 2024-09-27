// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Seatalk1.Messages;
using Microsoft.Extensions.Logging;
using UnitsNet;

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
                new CompassHeadingAndRudderPosition(),
                new CompassHeadingAutopilotCourse(),
                new CompassHeadingAutopilotCourseAlt(),
                new Keystroke(),
                new DeadbandSetting(),
                new SetLampIntensity(),
                new AutopilotCalibrationParameterMessage(),
                new ApparentWindAngle(),
                new ApparentWindSpeed(),
                new NavigationToWaypoint(),
                new CourseComputerStatus(),
                new TargetWaypointName(),
                new AutopilotWindStatus(),
                new SpeedTroughWater(),
            };

            MaxMessageLength = _messageFactories.Select(x => x.ExpectedLength).Max();
            if (MaxMessageLength > 18)
            {
                throw new InvalidOperationException("At least one message reports an expected length > 18 bytes. This is illegal");
            }
        }

        /// <summary>
        /// The maximum length of all known messages (used to detect when the parser lost sync)
        /// </summary>
        private int MaxMessageLength { get; set; }

        /// <summary>
        /// True if the input buffer is currently empty
        /// </summary>
        public bool IsBufferEmpty => _buffer.Count == 0;

        /// <summary>
        /// This event is fired when a new message was decoded
        /// </summary>
        public event Action<SeatalkMessage>? NewMessageDecoded;

        /// <summary>
        /// List of known messages
        /// </summary>
        internal List<SeatalkMessage> MessageTypes => _messageFactories;

        /// <summary>
        /// Register a new message type
        /// </summary>
        /// <param name="message">An instance of the new message type</param>
        /// <exception cref="ArgumentException">The definition of the message seems incorrect</exception>
        public void RegisterMessageType(SeatalkMessage message)
        {
            if (message.ExpectedLength > 18)
            {
                throw new ArgumentException("The maximum length of a message is 18 bytes", nameof(message));
            }

            _messageFactories.Add(message);
            MaxMessageLength = _messageFactories.Select(x => x.ExpectedLength).Max();
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

            bool isInSync = true;
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    while (true)
                    {
                        byte nextByte = _reader.ReadByte();
                        _buffer.Add(nextByte);
                        tryAgainWithoutFirstByte:
                        var msg = GetTypeOfNextMessage(_buffer, out int messageLength);
                        if (msg == null && !isInSync)
                        {
                            // In this case, we also need to check whether a substring of _buffer is a valid message (but still starting at index 0)
                            // This is because we have only chopped the first byte from an invalid message, but the remaining (up to 17) bytes may now
                            // form a valid message, but with a different length
                            for (int len = 3; len < _buffer.Count; len++)
                            {
                                msg = GetTypeOfNextMessage(_buffer.GetRange(0, len), out messageLength);
                                if (msg != null)
                                {
                                    break;
                                }
                            }
                        }

                        if (msg == null)
                        {
                            if (messageLength == 0)
                            {
                                continue;
                            }
                            else
                            {
                                if (isInSync)
                                {
                                    var bytesFound = BitConverter.ToString(_buffer.ToArray());
                                    _logger.LogWarning(
                                        $"Seatalk parser sync lost. Buffer contents: {bytesFound}, trying to resync");
                                }

                                _buffer.RemoveAt(0);
                                // We removed the first byte from the sequence, we need to try again before we add the next byte
                                isInSync = false;
                                goto tryAgainWithoutFirstByte;
                            }
                        }

                        NewMessageDecoded?.Invoke(msg);
                        _buffer.RemoveRange(0, messageLength);
                        if (_buffer.Count == 0)
                        {
                            isInSync = true;
                        }
                    }
                }
                catch (Exception x) when (x is EndOfStreamException || x is IOException || x is ObjectDisposedException)
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
        /// <param name="buffer">The data to decode</param>
        /// <param name="bytesInMessage">The number of bytes in the next message. Returns 0 if not enough data, -1 if sync lost (to many bytes, but no valid sentence)</param>
        /// <returns>An instance that can be used as factory for the next message or null if nothing can be decoded.</returns>
        internal SeatalkMessage? GetTypeOfNextMessage(IReadOnlyList<byte> buffer, out int bytesInMessage)
        {
            // The minimum message length is 3 bytes
            if (buffer.Count < 3)
            {
                bytesInMessage = 0;
                return null;
            }

            // The maximum length is 0xF + 3 = 18 or the maximum message length we know about
            if (buffer.Count > MaxMessageLength)
            {
                bytesInMessage = -1;
                return null;
            }

            foreach (var t in _messageFactories)
            {
                if (t.MatchesMessageType(buffer))
                {
                    bytesInMessage = t.ExpectedLength;
                    return t.CreateNewMessage(buffer);
                }
            }

            bytesInMessage = 0;
            return null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            StopDecode();
        }
    }
}

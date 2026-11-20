// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// A delegate for updating the current position
    /// </summary>
    /// <param name="position">The new position</param>
    /// <param name="track">The current true track</param>
    /// <param name="speed">The current speed over ground</param>
    public delegate void PositionUpdate(GeographicPosition position, Angle? track, Speed? speed);

    /// <summary>
    /// Parses Nmea 0183 Sequences
    /// </summary>
    public abstract class NmeaParser : NmeaSinkAndSource, IDisposable
    {
        private readonly object _lock;
        private Stream _dataSource;
        private Stream? _dataSink;
        private Thread? _parserThread;
        private CancellationTokenSource? _cancellationTokenSource;
        private StreamReader _reader;
        private Encoding _encoding;
        private Thread? _sendQueueThread;
        private BlockingCollection<NmeaSentence> _outQueue;
        private Exception? _ioExceptionOnSend;
        private DateTimeOffset _lastPacketTime;

        /// <summary>
        /// Creates a new instance of the NmeaParser, taking an input and an output stream
        /// </summary>
        /// <param name="interfaceName">Friendly name of this interface (used for filtering and eventually logging)</param>
        /// <param name="dataSource">Data source (may be connected to a serial port, a network interface, or whatever). It is recommended to use a blocking Stream,
        /// to prevent unnecessary polling</param>
        /// <param name="dataSink">Optional data sink, to send information. Can be null, and can be identical to the source stream</param>
        /// <param name="streamEncoding">The encoding of the underlying stream. Use <see cref="Raw8BitEncoding"/> if unsure.</param>
        protected NmeaParser(String interfaceName, Stream dataSource, Stream? dataSink, Encoding streamEncoding)
        : base(interfaceName)
        {
            _encoding = streamEncoding;
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _reader = new StreamReader(_dataSource, _encoding); // Nmea sentences are text
            _dataSink = dataSink;
            _lock = new object();
            _outQueue = new();
            ExclusiveTalkerId = TalkerId.Any;
            _ioExceptionOnSend = null;
            SuppressOutdatedMessages = true;
            LastPacketTime = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Set this to anything other than <see cref="TalkerId.Any"/> to receive only that specific ID from this parser
        /// </summary>
        public TalkerId ExclusiveTalkerId
        {
            get;
            set;
        }

        /// <summary>
        /// Time the last packet was received.
        /// This is writable to configure the initial value (defaults to current system time)
        /// </summary>
        public DateTimeOffset LastPacketTime
        {
            get => _lastPacketTime;
            set => _lastPacketTime = value;
        }

        /// <summary>
        /// True (the default) to suppress forwarding messages when a newer message with the same <see cref="SentenceId"/> and <see cref="TalkerId"/>
        /// is already in the outgoing queue. Only affects messages that have <see cref="NmeaSentence.ReplacesOlderInstance"/> set to true.
        /// Set to false to forward all messages.
        /// </summary>
        /// <remarks>
        /// Setting this to false may overflow send queues when more messages should be sent per time than the outgoing interface permits. This will result
        /// in massive message delivery delays and eventually a low memory situation.
        /// </remarks>
        public bool SuppressOutdatedMessages
        {
            get;
            set;
        }

        /// <summary>
        /// If true, the parser also accepts sentences in a log format (prefixed with a date and separated by |)
        /// </summary>
        public bool SupportLogReading
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the encoding of the underlying stream.
        /// By default, this uses a <see cref="Raw8BitEncoding"/>, a variant of the ASCII encoding.
        /// </summary>
        protected Encoding StreamEncoding
        {
            get => _encoding;
        }

        /// <summary>
        /// Source stream.
        /// </summary>
        protected Stream Source => _dataSource;

        /// <summary>
        /// Destination Stream. May be null to discard any output.
        /// </summary>
        protected Stream? Sink => _dataSink;

        /// <inheritdoc />
        public override void StartDecode()
        {
            lock (_lock)
            {
                if (_parserThread != null && _parserThread.IsAlive)
                {
                    throw new InvalidOperationException("Parser thread already started");
                }

                _ioExceptionOnSend = null;
                _cancellationTokenSource = new CancellationTokenSource();
                _parserThread = new Thread(Parser);
                _parserThread.Name = $"Nmea Parser for {InterfaceName}";
                _parserThread.Start();

                _sendQueueThread = new Thread(Sender);
                _sendQueueThread.Name = $"Nmea Sender for {InterfaceName}";
                _sendQueueThread.Start();
            }
        }

        private void Parser()
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                string? currentLine;
                try
                {
                    currentLine = _reader.ReadLine();
                    sw.Restart();
                }
                catch (IOException x)
                {
                    FireOnParserError(x.Message, NmeaError.PortClosed);
                    continue;
                }
                catch (ObjectDisposedException x)
                {
                    FireOnParserError(x.Message, NmeaError.PortClosed);
                    continue;
                }
                catch (OperationCanceledException x)
                {
                    FireOnParserError(x.Message, NmeaError.PortClosed);
                    continue;
                }
                catch (InvalidOperationException x)
                {
                    // NetworkStream throws this (rather than ObjectDisposedException) when the underlying
                    // stream has been disposed and is no longer readable (CanRead == false). This can
                    // happen during StopDecode while the parser thread is blocked inside ReadLine().
                    FireOnParserError(x.Message, NmeaError.PortClosed);
                    continue;
                }

                if (currentLine == null)
                {
                    try
                    {
                        if (_reader.EndOfStream)
                        {
                            FireOnParserError("End of stream detected.", NmeaError.PortClosed);
                        }
                    }
                    catch (Exception x) when (x is IOException || x is ObjectDisposedException || x is InvalidOperationException)
                    {
                        // Ignore here (already reported above)
                    }

                    continue; // Probably because the stream was closed.
                }

                if (SupportLogReading)
                {
                    if (currentLine.Contains("|"))
                    {
                        var splits = currentLine.Split(new char[]
                        {
                                '|'
                        }, StringSplitOptions.None);
                        if (splits.Length >= 3)
                        {
                            // The first column is the date, the second column the (original) data source
                            currentLine = splits[2]; // Raw message
                        }
                    }
                }

                TalkerSentence? sentence = ParseSentence(currentLine, out NmeaError error);
                if (sentence == null)
                {
                    // If error is none, but the return value is null, we just ignored that message.
                    if (error != NmeaError.None)
                    {
                        FireOnParserError($"Received invalid sentence {currentLine}: Error {error}.", error);
                    }

                    continue;
                }

                NmeaSentence? typed = sentence.TryGetTypedValue(ref _lastPacketTime);
                // Only test the messages that actually bring their own time, otherwise a wrong clock will cause this to get worse.
                if (typed != null && typed.Age > TimeSpan.FromSeconds(5) && (typed is TimeDate || typed is GlobalPositioningSystemFixData))
                {
                    FireOnParserError($"Message {typed} is already {typed.Age} old when it is processed", NmeaError.MessageDelayed);
                }

                DispatchSentenceEvents(typed);

                if (!(typed is RawSentence))
                {
                    // If we didn't dispatch it as raw sentence, do this as well
                    RawSentence raw = sentence.GetAsRawSentence(ref _lastPacketTime);
                    DispatchSentenceEvents(raw);
                }

                if (sw.ElapsedMilliseconds > 30)
                {
                    Logger.LogWarning($"Processing a message took {sw.ElapsedMilliseconds}ms");
                }
            }
        }

        /// <summary>
        /// Reads an input string and packs it into a TalkerSentence instance, which can then be further decoded.
        /// </summary>
        /// <param name="currentLine">The current line</param>
        /// <param name="error">An error message</param>
        /// <returns>A sentence object or null if the message was ignored or unparseable</returns>
        protected internal abstract TalkerSentence? ParseSentence(string currentLine, out NmeaError error);

        private void Sender()
        {
            while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                if (_outQueue.Count > 30)
                {
                    Logger.LogWarning($"Many items in send queue for '{InterfaceName}': {_outQueue.Count}");
                }

                if (_outQueue.TryTake(out var sentenceToSend, TimeSpan.FromSeconds(10)))
                {
                    if (sentenceToSend.ReplacesOlderInstance && SuppressOutdatedMessages)
                    {
                        // If there are other instances of the same message in the queue, we drop the current one (as it's not the newest)
                        // and continue processing.
                        // There's a glitch here, however: If we keep adding items quickly to the queue and processing it takes
                        // time, the newest items are never processed, thus some messages will _never_ be sent.
                        var newerInstance = _outQueue.FirstOrDefault(x => x.IsSameMessageAs(sentenceToSend));
                        if (newerInstance != null)
                        {
                            continue;
                        }
                    }

                    if (sentenceToSend.Valid == false)
                    {
                        continue;
                    }

                    try
                    {
                        FormatAndSendSentence(sentenceToSend);
                    }
                    catch (IOException x)
                    {
                        // Sink may be a network port
                        _ioExceptionOnSend = x;
                    }
                    catch (ObjectDisposedException)
                    {
                        // Probably going to close the port any moment
                    }
                }
            }
        }

        /// <summary>
        /// Formats and sends the sentence according to the underlying transport.
        /// </summary>
        /// <param name="sentence">The sentence to send</param>
        protected internal abstract void FormatAndSendSentence(NmeaSentence sentence);

        /// <inheritdoc />
        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            if (_ioExceptionOnSend != null)
            {
                // Rethrow exception from other thread (required for correct termination of stale network sockets)
                throw _ioExceptionOnSend;
            }

            try
            {
                _outQueue.Add(sentence);
            }
            catch (InvalidOperationException x)
            {
                // the queue is closed
                FireOnParserError($"{x.Message}", NmeaError.PortClosed);
            }
        }

        /// <inheritdoc />
        public override void StopDecode()
        {
            lock (_lock)
            {
                if (_parserThread != null && _parserThread.IsAlive && _cancellationTokenSource != null)
                {
                    _outQueue.CompleteAdding();
                    _cancellationTokenSource.Cancel();
                    _dataSource.Dispose();
                    _dataSink?.Dispose();
                    _reader.Dispose();
                    _parserThread.Join();

                    _sendQueueThread?.Join();
                    _cancellationTokenSource = null;

                    _parserThread = null;
                    _dataSource = null!;
                    _dataSink = null!;
                    _reader = null!;
                    _sendQueueThread = null;
                }
            }
        }
    }
}

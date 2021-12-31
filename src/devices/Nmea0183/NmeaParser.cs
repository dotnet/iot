// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
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
    /// Parses Nmea Sequences
    /// </summary>
    public class NmeaParser : NmeaSinkAndSource, IDisposable
    {
        private readonly object _lock;
        private Stream _dataSource;
        private Stream? _dataSink;
        private Thread? _parserThread;
        private CancellationTokenSource? _cancellationTokenSource;
        private StreamReader _reader;
        private Raw8BitEncoding _encoding;
        private Thread? _sendQueueThread;
        private ConcurrentQueue<NmeaSentence> _outQueue;
        private AutoResetEvent _outEvent;
        private Exception? _ioExceptionOnSend;

        /// <summary>
        /// Creates a new instance of the NmeaParser, taking an input and an output stream
        /// </summary>
        /// <param name="interfaceName">Friendly name of this interface (used for filtering and eventually logging)</param>
        /// <param name="dataSource">Data source (may be connected to a serial port, a network interface, or whatever). It is recommended to use a blocking Stream,
        /// to prevent unnecessary polling</param>
        /// <param name="dataSink">Optional data sink, to send information. Can be null, and can be identical to the source stream</param>
        public NmeaParser(String interfaceName, Stream dataSource, Stream? dataSink)
        : base(interfaceName)
        {
            _encoding = new Raw8BitEncoding();
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _reader = new StreamReader(_dataSource, _encoding); // Nmea sentences are text
            _dataSink = dataSink;
            _lock = new object();
            _outQueue = new();
            _outEvent = new AutoResetEvent(false);
            ExclusiveTalkerId = TalkerId.Any;
            _ioExceptionOnSend = null;
        }

        /// <summary>
        /// Set this to anything other than <see cref="TalkerId.Any"/> to receive only that specific ID from this parser
        /// </summary>
        public TalkerId ExclusiveTalkerId
        {
            get;
            set;
        }

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
            while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                string? currentLine;
                try
                {
                    currentLine = _reader.ReadLine();
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

                if (currentLine == null)
                {
                    try
                    {
                        if (_reader.EndOfStream)
                        {
                            FireOnParserError("End of stream detected.", NmeaError.PortClosed);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // Ignore here (already reported above)
                    }

                    Thread.Sleep(10); // to prevent busy-waiting
                    continue; // Probably because the stream was closed.
                }

                TalkerSentence? sentence = TalkerSentence.FromSentenceString(currentLine, ExclusiveTalkerId, out var error);
                if (sentence == null)
                {
                    // If error is none, but the return value is null, we just ignored that message.
                    if (error != NmeaError.None)
                    {
                        FireOnParserError($"Received invalid sentence {currentLine}: Error {error}.", error);
                    }

                    continue;
                }

                NmeaSentence? typed = sentence.TryGetTypedValue();
                // Only test the messages that actually bring their own time, otherwise a wrong clock will cause this to get worse.
                if (typed != null && typed.Age > TimeSpan.FromSeconds(5) && (typed is TimeDate || typed is GlobalPositioningSystemFixData))
                {
                    FireOnParserError($"Message {typed} is already {typed.Age} old when it is processed", NmeaError.MessageDelayed);
                }

                DispatchSentenceEvents(typed);

                if (!(typed is RawSentence))
                {
                    // If we didn't dispatch it as raw sentence, do this as well
                    RawSentence raw = sentence.GetAsRawSentence();
                    DispatchSentenceEvents(raw);
                }
            }
        }

        private void Sender()
        {
            while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                if (_outQueue.TryDequeue(out var sentenceToSend))
                {
                    if (sentenceToSend.ReplacesOlderInstance)
                    {
                        // If there are other instances of the same message in the queue, we drop the current one (as it's not the newest)
                        // and continue processing.
                        var newerInstance = _outQueue.FirstOrDefault(x => x.SentenceId == sentenceToSend.SentenceId && x.TalkerId == sentenceToSend.TalkerId);
                        if (newerInstance != null)
                        {
                            continue;
                        }
                    }

                    TalkerSentence ts = new TalkerSentence(sentenceToSend);
                    string dataToSend = ts.ToString() + "\r\n";
                    byte[] buffer = _encoding.GetBytes(dataToSend);

                    ////if (InterfaceName == "Handheld")
                    ////{
                    ////    Console.Write($"--> {InterfaceName} ({_outQueue.Count}): {dataToSend}");
                    ////}

                    try
                    {
                        _dataSink?.Write(buffer, 0, buffer.Length);
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

                if (_outQueue.IsEmpty)
                {
                    WaitHandle.WaitAny(new WaitHandle[] { _outEvent, _cancellationTokenSource.Token.WaitHandle });
                }
            }
        }

        /// <inheritdoc />
        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            if (_ioExceptionOnSend != null)
            {
                // Rethrow exception from other thread (required for correct termination of stale network sockets)
                throw _ioExceptionOnSend;
            }

            _outQueue.Enqueue(sentence);
            _outEvent?.Set();
        }

        /// <inheritdoc />
        public override void StopDecode()
        {
            lock (_lock)
            {
                if (_parserThread != null && _parserThread.IsAlive && _cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    _dataSource.Dispose();
                    _dataSink?.Dispose();
                    _reader.Dispose();
                    _parserThread.Join();

                    _sendQueueThread?.Join();
                    _cancellationTokenSource = null;
                    _outEvent.Dispose();

                    _parserThread = null;
                    _dataSource = null!;
                    _dataSink = null!;
                    _reader = null!;
                    _sendQueueThread = null;
                    _outEvent = null!;
                }
            }
        }
    }
}

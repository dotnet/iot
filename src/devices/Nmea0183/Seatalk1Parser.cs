// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Seatalk1.Messages;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Seatalk1
{
    /// <summary>
    /// Decodes a Seatalk1 Message stream into Nmea0183 sentences.
    /// </summary>
    public sealed class Seatalk1Parser : NmeaParser
    {
        private const Parity DefaultParity = Parity.Even;
        private readonly string _portName;
        private readonly Stream _inputStream;
        private readonly BinaryReader _reader;
        private readonly object _lock;
        private readonly List<byte> _buffer;

        private List<SeatalkMessage> _messageFactories;
        private SerialPort _port;
        private bool _bufferIsInSync;

        /// <summary>
        /// Initializes a new instance of the <see cref="Seatalk1Parser"/> class.
        /// </summary>
        /// <param name="interfaceName">The name of this interface.</param>
        /// <param name="portName">The name of the serial port to use. E.g. "COM5" or "/dev/ttyAMA2"</param>
        public Seatalk1Parser(string interfaceName, string portName)
        : base(interfaceName, new MemoryStream(), null, new Raw8BitEncoding())
        {
            _portName = portName;
            _lock = new object();
            _buffer = new List<byte>(20);

            _port = new SerialPort(portName);
            _port.BaudRate = 4800;
            _port.Parity = DefaultParity; // Can be anything but none, but "Mark" or "Space" won't have the desired effect on linux, causing garbage to be sent
            _port.StopBits = StopBits.One;
            _port.DataBits = 8;
            _port.Open();
            _bufferIsInSync = true;

            try
            {
                _inputStream = _port.BaseStream;
                if (!_inputStream.CanRead)
                {
                    throw new ArgumentException("Cannot read from the input port");
                }
            }
            finally
            {
                _port.Close();
            }

            _reader = new BinaryReader(_inputStream, Encoding.UTF8);
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
                new DeviceIdentification(),
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
        /// Terminates the decoding thread.
        /// </summary>
        public override void StopDecode()
        {
            lock (_lock)
            {
                _reader.Close();
                _inputStream.Close();
            }

            _buffer.Clear(); // Because clients could wait for an empty buffer

            base.StopDecode();
        }

        protected override string? ReadNextLine()
        {
            // I think we would just need to return either null or something else. The
            // parser will then call GetTypeOfNextMessage to see if a message can be decoded from the buffer.
            byte nextByte = _reader.ReadByte();
            _buffer.Add(nextByte);
            return _buffer.Count > 0 ? BitConverter.ToString(_buffer.ToArray()) : null;
        }

        protected internal override TalkerSentence? ParseSentence(string currentLine, out NmeaError error)
        {
            SeatalkMessage? msg = null;
            while (true)
            {
                msg = GetTypeOfNextMessage(_buffer, out int messageLength);
                if (msg == null && !_bufferIsInSync)
                {
                    // In this case, we also need to check whether a substring of _buffer is a valid message (but still starting at index 0)
                    // This is because we have only chopped the first byte from an invalid message, but the remaining (up to 17) bytes may now
                    // form a valid message, but with a different length
                    for (int len = 3; len < _buffer.Count; len++)
                    {
                        msg = GetTypeOfNextMessage(_buffer.GetRange(0, len), out messageLength);
                        if (msg != null)
                        {
                            _buffer.RemoveRange(0, len);
                            break;
                        }
                    }
                }
                else if (msg != null)
                {
                    _buffer.RemoveRange(0, messageLength);
                }

                if (msg == null)
                {
                    if (messageLength == 0)
                    {
                        error = NmeaError.NoSyncByte;
                        break;
                    }
                    else
                    {
                        if (_bufferIsInSync)
                        {
                            var bytesFound = BitConverter.ToString(_buffer.ToArray());
                            Logger.LogWarning(
                                $"Seatalk parser sync lost. Buffer contents: {bytesFound}, trying to resync");
                        }

                        _buffer.RemoveAt(0);
                        // We removed the first byte from the sequence, we need to try again before we add the next byte
                        _bufferIsInSync = false;
                        continue;
                    }
                }

                if (_buffer.Count == 0)
                {
                    _bufferIsInSync = true;
                }

                break;
            }

            if (msg != null)
            {
                error = NmeaError.None;
                TalkerSentence ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data);
            }

            error = NmeaError.MessageDropped;
            return null;
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

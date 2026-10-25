// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Seatalk1.Messages;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Seatalk1
{
    /// <summary>
    /// Decodes a Seatalk1 Message stream into Nmea0183 sentences.
    /// </summary>
    public class Seatalk1Parser : NmeaParser
    {
        private const Parity DefaultParity = Parity.Even;
        private readonly string _portName;
        private readonly object _lock;
        private readonly List<byte> _buffer;

        private List<SeatalkMessage> _messageFactories;
        private ISerialPort _port;
        private bool _bufferIsInSync;

        /// <summary>
        /// Initializes a new instance of the <see cref="Seatalk1Parser"/> class.
        /// </summary>
        /// <param name="interfaceName">The name of this interface.</param>
        /// <param name="portName">The name of the serial port to use. E.g. "COM5" or "/dev/ttyAMA2"</param>
        public Seatalk1Parser(string interfaceName, string portName)
            : this(interfaceName, new SerialPortAdapter(portName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Seatalk1Parser"/> class.
        /// </summary>
        /// <param name="interfaceName">The name of this interface.</param>
        /// <param name="port">Serial transport abstraction for production or tests.</param>
        public Seatalk1Parser(string interfaceName, ISerialPort port)
        : base(interfaceName, new MemoryStream(), null, new Raw8BitEncoding())
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _portName = _port.PortName;
            _lock = new object();
            _buffer = new List<byte>(20);

            _port.BaudRate = 4800;
            _port.Parity = DefaultParity; // Can be anything but none, but "Mark" or "Space" won't have the desired effect on linux, causing garbage to be sent
            _port.StopBits = StopBits.One;
            _port.DataBits = 8;
            _port.Open();
            _bufferIsInSync = true;

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
                _port.Close();
            }

            _buffer.Clear(); // Because clients could wait for an empty buffer

            base.StopDecode();
        }

        /// <summary>
        /// Reads the next line from the input stream. This is overridden to read a single byte at a time, because Seatalk messages are not line-based.
        /// It returns the next at most 18 bytes as a hex string, which will later be parsed into a message.
        /// The parser will call GetTypeOfNextMessage to see if a message can be decoded from the buffer.
        /// </summary>
        /// <returns>The next line as a string, or null if the end of the stream is reached or no data is available.</returns>
        protected internal override string? ReadNextLine()
        {
            // I think we would just need to return either null or something else. The
            // parser will then call GetTypeOfNextMessage to see if a message can be decoded from the buffer.
            int nextByte = _port.ReadByte();
            if (nextByte == -1)
            {
                return null;
            }

            _buffer.Add((byte)nextByte);
            var contents = _buffer.Take(18).ToArray();
            return contents.Length > 0 ? String.Join(',', contents.Select(x => x.ToString("X2", CultureInfo.InvariantCulture))) : null;
        }

        /// <summary>
        /// Does the final parsing of the message into a TalkerSentence.
        /// This is called after ReadNextLine has returned a line.
        /// </summary>
        /// <param name="currentLine">The message to decode</param>
        /// <param name="error">The error encountered during parsing, if any</param>
        /// <returns>The parsed TalkerSentence, or null if parsing failed (not enough data, not in sync, etc.)</returns>
        protected internal override TalkerSentence? ParseSentence(string currentLine, out NmeaError error)
        {
            IEnumerable<byte>? msg = null;
            while (true)
            {
                byte[] toTest = Convert.FromHexString(currentLine.Replace(",", string.Empty));
                msg = GetDataForNextMessage(toTest, out int messageLength);
                if (msg == null && !_bufferIsInSync)
                {
                    // In this case, we also need to check whether a substring of currentLine is a valid message (but still starting at index 0)
                    // This is because we have only chopped the first byte from an invalid message, but the remaining (up to 17) bytes may now
                    // form a valid message, but with a different length. GetDataForNextMessage only returns non-null when the message
                    // length matches exactly.
                    for (int len = 3; len < toTest.Length; len++)
                    {
                        msg = GetDataForNextMessage(toTest.Take(len).ToList(), out messageLength);
                        if (msg != null)
                        {
                            currentLine = BitConverter.ToString(msg.ToArray());
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
                TalkerSentence ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id,
                    currentLine.Split(new[] { '-', ',' }));
                return ts;
            }

            error = NmeaError.None; // Probably just not yet a complete message
            return null;
        }

        internal IReadOnlyList<byte>? GetDataForNextMessage(IReadOnlyList<byte> input, out int bytesInMessage)
        {
            if (input.Count < 3)
            {
                bytesInMessage = 0;
                return null;
            }

            if (input.Count > MaxMessageLength)
            {
                bytesInMessage = 0;
                return null;
            }

            foreach (var t in _messageFactories)
            {
                if (t.MatchesMessageType(input))
                {
                    bytesInMessage = t.ExpectedLength;
                    // At this point, we just need the bytes corresponding to a message.
                    // We decode the content later. MatchesMessageType returns true only if the
                    // input length is exactly the number of bytes expected for the message. It may
                    // not be longer.
                    return input;
                }
            }

            bytesInMessage = 0;
            return null;
        }

        /// <inheritdoc/>
        protected internal override void FormatAndSendSentence(NmeaSentence sentence)
        {
            if (sentence.SentenceId == SeatalkMessage.Id && sentence.TalkerId == TalkerId.Seatalk)
            {
                // Implementation for formatting and sending the sentence
                if (sentence is SeatalkMessage seatalkMessage)
                {
                    byte[] datagram = seatalkMessage.CreateDatagram();
                    bool success = SendDatagram(datagram);
                    if (!success)
                    {
                        Logger.LogWarning($"Failed to send Seatalk message: {seatalkMessage.GetType().Name}");
                    }
                }
                else
                {
                    // It's a message starting with STALK but not a SeatalkMessage, this is very unexpected
                    Logger.LogError($"Cannot send sentence of type {sentence.GetType().Name} as Seatalk message");
                }
            }
        }

        private static int BitCount(int b)
        {
            int count = 0;
            while (b != 0)
            {
                count++;
                b &= (b - 1); // walking through all the bits which are set to one
            }

            return count;
        }

        /// <summary>
        /// This calculates the correct parity for each byte in the datagram.
        /// </summary>
        /// <param name="data">The datagram to send (first byte is the command byte, remainder are the arguments)</param>
        /// <returns>A list of bytes with parity information</returns>
        /// <remarks>
        /// We need to send the first byte (the command byte) with a parity of "mark" (=1), all the remaining bytes with a parity of "space" (=0),
        /// but since Linux doesn't seem to properly support Mark or Space parity settings (on the raspberry pi, that setting results in no parity bit to be sent)
        /// we cheat and count what the parity bit should be and use even or odd to achieve the desired result.
        /// </remarks>
        internal static List<(byte B, Parity P, int Index)> CalculateParityForEachByte(byte[] data)
        {
            bool isCommandByte = true;
            List<(byte, Parity, int)> ret = new();
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                bool isEven = BitCount(b) % 2 == 0;
                Parity parityToSend;
                if (isEven)
                {
                    if (isCommandByte)
                    {
                        // command byte so far is even, we need the parity bit to be 1, so use odd parity
                        // (because the parity setting "odd" means that the data bits including the parity bit count to odd)
                        parityToSend = Parity.Odd;
                    }
                    else
                    {
                        parityToSend = Parity.Even;
                    }
                }
                else
                {
                    if (isCommandByte)
                    {
                        parityToSend = Parity.Even;
                    }
                    else
                    {
                        parityToSend = Parity.Odd;
                    }
                }

                ret.Add((data[i], parityToSend, i));
                isCommandByte = false;
            }

            return ret;
        }

        /// <summary>
        /// Synchronously send a datagram on a Seatalk1 bus. This is a blocking call, and will wait until the bus is free to send the datagram.
        /// </summary>
        /// <param name="data">The datagram</param>
        /// <returns>
        /// True on success, false otherwise. May fail e.g. if the bus is busy all the time.
        /// Todo: This doesn't do any checks for collisions just yet.
        /// </returns>
        private bool SendDatagram(byte[] data)
        {
            // First calculate the required parity setting for each byte.
            var dataWithParity = CalculateParityForEachByte(data);

            // Only start sending if the buffer is empty. This should hopefully avoid to many collisions
            while (_buffer.Count > 0)
            {
                // Sending a message takes 1 - 2ms, so wait a bit (knowing that this sleep isn't precise)
                Thread.Sleep(1);
            }

            // Only one thread at a time, please!
            try
            {
                lock (_lock)
                {
                    foreach (var e in dataWithParity)
                    {
                        // The many flushes here appear to be necessary to make sure the parity correctly applies to the next byte we send (and only to the next)
                        if (_port.Parity != e.P)
                        {
                            _port.BaseStream.Flush();
                            _port.Parity = e.P;
                            _port.BaseStream.Flush();
                        }

                        _port.Write(data, e.Index, 1);
                    }

                    _port.BaseStream.Flush();
                    _port.Parity = DefaultParity;
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _port?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

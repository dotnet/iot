// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Arduino
{
    internal delegate void AnalogPinValueUpdated(int pin, uint rawValue);

    /// <summary>
    /// Low-level communication layer for the firmata protocol. Creates the binary command stream for the different commands and returns back results.
    /// </summary>
    internal sealed class FirmataDevice : IDisposable
    {
        private const byte FIRMATA_PROTOCOL_MAJOR_VERSION = 2;
        private const byte FIRMATA_PROTOCOL_MINOR_VERSION = 5; // 2.5 works, but 2.6 is recommended
        private const int FIRMATA_INIT_TIMEOUT_SECONDS = 2;
        internal static readonly TimeSpan DefaultReplyTimeout = TimeSpan.FromMilliseconds(3000);

        private byte _firmwareVersionMajor;
        private byte _firmwareVersionMinor;
        private Version _actualFirmataProtocolVersion;
        private Version _firmwareVersion;

        private int _lastRequestId;

        private string _firmwareName;
        private Stream? _firmataStream;
        private Thread? _inputThread;
        private List<SupportedPinConfiguration> _supportedPinConfigurations;
        private BlockingConcurrentBag<byte[]> _pendingResponses;
        private List<PinValue> _lastPinValues;
        private Dictionary<int, uint> _lastAnalogValues;
        private object _lastPinValueLock;
        private object _lastAnalogValueLock;
        private object _synchronisationLock;
        private Queue<byte> _dataQueue;
        private StringBuilder _lastRawLine;

        private CommandError _lastCommandError;

        private int _i2cSequence;

        /// <summary>
        /// Event used when waiting for answers (i.e. after requesting firmware version)
        /// </summary>
        private AutoResetEvent _dataReceived;

        private ILogger _logger;

        public event PinChangeEventHandler? DigitalPortValueUpdated;

        public event AnalogPinValueUpdated? AnalogPinValueUpdated;

        public event Action<string, Exception?>? OnError;

        public event Action<ReplyType, byte[]>? OnSysexReply;

        private long _bytesTransmitted = 0;

        private int _numberOfConsecutiveI2cWrites = 0;

        private bool _systemVariablesSupported = false;

        public FirmataDevice(List<SupportedMode> supportedModes)
        {
            _firmwareVersionMajor = 0;
            _firmwareVersionMinor = 0;
            _firmwareVersion = new Version(0, 0);
            _actualFirmataProtocolVersion = new Version(0, 0);
            _firmataStream = null;
            InputThreadShouldExit = false;
            _dataReceived = new AutoResetEvent(false);
            _supportedPinConfigurations = new List<SupportedPinConfiguration>();
            _synchronisationLock = new object();
            _lastPinValues = new List<PinValue>();
            _lastPinValueLock = new object();
            _lastAnalogValues = new Dictionary<int, uint>();
            _lastAnalogValueLock = new object();
            _dataQueue = new Queue<byte>(1024);
            _pendingResponses = new BlockingConcurrentBag<byte[]>();
            _lastRequestId = 1;
            _lastCommandError = CommandError.None;
            _firmwareName = string.Empty;
            _lastRawLine = new StringBuilder();
            SupportedModes = supportedModes;
            _i2cSequence = 0;
            _logger = this.GetCurrentClassLogger();
        }

        internal List<SupportedPinConfiguration> PinConfigurations
        {
            get
            {
                return _supportedPinConfigurations;
            }
        }

        internal List<SupportedMode> SupportedModes { get; set; }

        internal long BytesTransmitted => _bytesTransmitted;

        internal bool InputThreadShouldExit { get; set; }

        public void Open(Stream stream)
        {
            lock (_synchronisationLock)
            {
                if (_firmataStream != null)
                {
                    throw new InvalidOperationException("The device is already open");
                }

                _firmataStream = stream;
                if (_firmataStream.CanRead && _firmataStream.CanWrite)
                {
                    StartListening();
                }
                else
                {
                    throw new NotSupportedException("Need a read-write stream to the hardware device");
                }
            }
        }

        private void StartListening()
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            if (_inputThread != null && _inputThread.IsAlive)
            {
                return;
            }

            InputThreadShouldExit = false;

            _inputThread = new Thread(InputThread);
            _inputThread.Name = "Firmata input thread";
            _inputThread.Start();
        }

        private void ProcessInput()
        {
            if (_dataQueue.Count == 0)
            {
                FillQueue();
            }

            if (_dataQueue.Count == 0)
            {
                // Still no data? (End of stream or stream closed)
                return;
            }

            int data = _dataQueue.Dequeue();

            // OnError?.Invoke($"0x{data:X}", null);
            byte b = (byte)(data & 0x00FF);
            byte upper_nibble = (byte)(data & 0xF0);
            byte lower_nibble = (byte)(data & 0x0F);

            /*
             * the relevant bits in the command depends on the value of the data byte. If it is less than 0xF0 (start sysex), only the upper nibble identifies the command
             * while the lower nibble contains additional data
             */
            FirmataCommand command = (FirmataCommand)((data < ((ushort)FirmataCommand.START_SYSEX) ? upper_nibble : b));

            // determine the number of bytes remaining in the message
            int bytes_remaining = 0;
            bool isMessageSysex = false;
            switch (command)
            {
                default: // command not understood
                    char c = (char)data;
                    _lastRawLine.Append(c);
                    if (c == '\n')
                    {
                        OnError?.Invoke(_lastRawLine.ToString().Trim(), null);
                        OnSysexReply?.Invoke(ReplyType.AsciiData, Encoding.Unicode.GetBytes(_lastRawLine.ToString()));
                        _lastRawLine.Clear();
                    }

                    return;
                case FirmataCommand.END_SYSEX: // should never happen
                    return;

                // commands that require 2 additional bytes
                case FirmataCommand.DIGITAL_MESSAGE:
                case FirmataCommand.ANALOG_MESSAGE:
                case FirmataCommand.SET_PIN_MODE:
                case FirmataCommand.PROTOCOL_VERSION:
                    bytes_remaining = 2;
                    break;

                // commands that require 1 additional byte
                case FirmataCommand.REPORT_ANALOG_PIN:
                case FirmataCommand.REPORT_DIGITAL_PIN:
                    bytes_remaining = 1;
                    break;

                // commands that do not require additional bytes
                case FirmataCommand.SYSTEM_RESET:
                    // do nothing, as there is nothing to reset
                    return;

                case FirmataCommand.START_SYSEX:
                    // this is a special case with no set number of bytes remaining
                    isMessageSysex = true;
                    _lastRawLine.Clear();
                    break;
            }

            // read the remaining message while keeping track of elapsed time to timeout in case of incomplete message
            List<byte> message = new List<byte>();
            int bytes_read = 0;
            Stopwatch timeout_start = Stopwatch.StartNew();
            while (bytes_remaining > 0 || isMessageSysex)
            {
                if (_dataQueue.Count == 0)
                {
                    int timeout = 10;
                    while (!FillQueue() && timeout-- > 0)
                    {
                        Thread.Sleep(5);
                    }

                    if (timeout == 0)
                    {
                        // Synchronisation problem: The remainder of the expected message is missing
                        _lastCommandError = CommandError.Timeout;
                        return;
                    }
                }

                data = _dataQueue.Dequeue();
                // OnError?.Invoke($"0x{data:X}", null);
                // if no data was available, check for timeout
                if (data == 0xFFFF)
                {
                    // get elapsed seconds, given as a double with resolution in nanoseconds
                    TimeSpan elapsed = timeout_start.Elapsed;

                    if (elapsed > DefaultReplyTimeout)
                    {
                        _lastCommandError = CommandError.Timeout;
                        return;
                    }

                    continue;
                }

                timeout_start.Restart();

                // if we're parsing sysex and we've just read the END_SYSEX command, we're done.
                if (isMessageSysex && (data == (short)FirmataCommand.END_SYSEX))
                {
                    break;
                }

                message.Add((byte)(data & 0xFF));
                ++bytes_read;
                --bytes_remaining;
            }

            // process the message
            switch (command)
            {
                // ignore these message types (they should not be in a reply)
                default:
                case FirmataCommand.REPORT_ANALOG_PIN:
                case FirmataCommand.REPORT_DIGITAL_PIN:
                case FirmataCommand.SET_PIN_MODE:
                case FirmataCommand.END_SYSEX:
                case FirmataCommand.SYSTEM_RESET:
                    return;
                case FirmataCommand.PROTOCOL_VERSION:
                    if (_actualFirmataProtocolVersion.Major != 0)
                    {
                        // Firmata sends this message automatically after a device reset (if you press the reset button on the arduino)
                        // If we know the version already, this is unexpected.
                        _lastCommandError = CommandError.DeviceReset;
                        OnError?.Invoke("The device was unexpectedly reset. Please restart the communication.", null);
                    }

                    _actualFirmataProtocolVersion = new Version(message[0], message[1]);
                    _logger.LogInformation($"Received protocol version: {_actualFirmataProtocolVersion}.");
                    _dataReceived.Set();

                    return;

                case FirmataCommand.ANALOG_MESSAGE:
                    // report analog commands store the pin number in the lower nibble of the command byte, the value is split over two 7-bit bytes
                    {
                        int channel = lower_nibble;
                        uint value = (uint)(message[0] | (message[1] << 7));
                        // This must work
                        int pin = _supportedPinConfigurations.First(x => x.AnalogPinNumber == channel).Pin;
                        lock (_lastAnalogValueLock)
                        {
                            _lastAnalogValues[pin] = value;
                        }

                        AnalogPinValueUpdated?.Invoke(channel, value);
                    }

                    break;

                case FirmataCommand.DIGITAL_MESSAGE:
                    // digital messages store the port number in the lower nibble of the command byte, the port value is split over two 7-bit bytes
                    // Each port corresponds to 8 pins
                    {
                        int offset = lower_nibble * 8;
                        ushort pinValues = (ushort)(message[0] | (message[1] << 7));
                        if (offset + 7 >= _lastPinValues.Count)
                        {
                            _logger.LogError($"Firmware reported an update for port {lower_nibble}, but there are only {_supportedPinConfigurations.Count} pins");
                            break;
                        }

                        lock (_lastPinValueLock)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                PinValue oldValue = _lastPinValues[i + offset];
                                int mask = 1 << i;
                                PinValue newValue = (pinValues & mask) == 0 ? PinValue.Low : PinValue.High;
                                if (newValue != oldValue)
                                {
                                    PinEventTypes eventTypes = newValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling;
                                    _lastPinValues[i + offset] = newValue;
                                    // TODO: The callback should not be within the lock
                                    DigitalPortValueUpdated?.Invoke(this, new PinValueChangedEventArgs(eventTypes, i + offset));
                                }
                            }
                        }
                    }

                    break;

                case FirmataCommand.START_SYSEX:
                    // a sysex message must include at least one extended-command byte
                    if (bytes_read < 1)
                    {
                        _lastCommandError = CommandError.InvalidArguments;
                        return;
                    }

                    // retrieve the raw data array & extract the extended-command byte
                    byte[] raw_data = message.ToArray();
                    FirmataSysexCommand sysCommand = (FirmataSysexCommand)(raw_data[0]);
                    int index = 0;
                    ++index;
                    --bytes_read;

                    switch (sysCommand)
                    {
                        case FirmataSysexCommand.REPORT_FIRMWARE:
                            // See https://github.com/firmata/protocol/blob/master/protocol.md
                            // Byte 0 is the command (0x79) and can be skipped here, as we've already interpreted it
                            {
                                _firmwareVersionMajor = raw_data[1];
                                _firmwareVersionMinor = raw_data[2];
                                _firmwareVersion = new Version(_firmwareVersionMajor, _firmwareVersionMinor);
                                int stringLength = (raw_data.Length - 3) / 2;
                                Span<byte> bytesReceived = stackalloc byte[stringLength];
                                ReassembleByteString(raw_data, 3, stringLength * 2, bytesReceived);

                                _firmwareName = Encoding.ASCII.GetString(bytesReceived);
                                _logger.LogDebug($"Received Firmware name {_firmwareName}");
                                _dataReceived.Set();
                            }

                            return;

                        case FirmataSysexCommand.STRING_DATA:
                            {
                                // condense back into 1-byte data
                                int stringLength = (raw_data.Length - 1) / 2;
                                Span<byte> bytesReceived = stackalloc byte[stringLength];
                                ReassembleByteString(raw_data, 1, stringLength * 2, bytesReceived);

                                string message1 = Encoding.UTF8.GetString(bytesReceived);
                                int idxNull = message1.IndexOf('\0');
                                if (message1.Contains("%") && idxNull > 0) // C style printf formatters
                                {
                                    message1 = message1.Substring(0, idxNull);
                                    string message2 = PrintfFromByteStream(message1, bytesReceived, idxNull + 1);
                                    OnError?.Invoke(message2, null);
                                }
                                else
                                {
                                    OnError?.Invoke(message1, null);
                                }
                            }

                            break;

                        case FirmataSysexCommand.CAPABILITY_RESPONSE:
                            {
                                _supportedPinConfigurations.Clear();
                                int idx = 1;
                                SupportedPinConfiguration currentPin = new SupportedPinConfiguration(0);
                                int pin = 0;
                                while (idx < raw_data.Length)
                                {
                                    int mode = raw_data[idx++];
                                    if (mode == 0x7F)
                                    {
                                        _supportedPinConfigurations.Add(currentPin);
                                        currentPin = new SupportedPinConfiguration(++pin);
                                        continue;
                                    }

                                    int resolution = raw_data[idx++];
                                    SupportedMode? sm = SupportedModes.FirstOrDefault(x => x.Value == mode);
                                    if (sm == SupportedMode.AnalogInput)
                                    {
                                        currentPin.PinModes.Add(SupportedMode.AnalogInput);
                                        currentPin.AnalogInputResolutionBits = resolution;
                                    }
                                    else if (sm == SupportedMode.Pwm)
                                    {
                                        currentPin.PinModes.Add(SupportedMode.Pwm);
                                        currentPin.PwmResolutionBits = resolution;
                                    }
                                    else if (sm == null)
                                    {
                                        sm = new SupportedMode((byte)mode, $"Unknown mode {mode}");
                                        currentPin.PinModes.Add(sm);
                                    }
                                    else
                                    {
                                        currentPin.PinModes.Add(sm);
                                    }
                                }

                                // Add 8 entries, so that later we do not need to check whether a port (bank) is complete
                                _lastPinValues = new PinValue[_supportedPinConfigurations.Count + 8].ToList();
                                _dataReceived.Set();
                                // Do not add the last instance, should also be terminated by 0xF7
                            }

                            break;

                        case FirmataSysexCommand.ANALOG_MAPPING_RESPONSE:
                            {
                                // This needs to have been set up previously
                                if (_supportedPinConfigurations.Count == 0)
                                {
                                    return;
                                }

                                int idx = 1;
                                int pin = 0;
                                while (idx < raw_data.Length)
                                {
                                    if (raw_data[idx] != 127)
                                    {
                                        _supportedPinConfigurations[pin].AnalogPinNumber = raw_data[idx];
                                    }

                                    idx++;
                                    pin++;
                                }

                                _dataReceived.Set();
                            }

                            break;

                        case FirmataSysexCommand.EXTENDED_ANALOG:
                            // report analog commands store the pin number in the lower nibble of the command byte, the value is split over two 7-bit bytes
                            {
                                int channel = raw_data[1];
                                uint value = (uint)(raw_data[2] | (raw_data[3] << 7));
                                // This must work
                                int pin = _supportedPinConfigurations.First(x => x.AnalogPinNumber == channel).Pin;
                                lock (_lastAnalogValueLock)
                                {
                                    _lastAnalogValues[pin] = value;
                                }

                                AnalogPinValueUpdated?.Invoke(channel, value);
                            }

                            break;

                        case FirmataSysexCommand.I2C_REPLY:
                            _lastCommandError = CommandError.None;
                            _pendingResponses.Add(raw_data);
                            break;

                        case FirmataSysexCommand.SPI_DATA:
                            _lastCommandError = CommandError.None;
                            _pendingResponses.Add(raw_data);
                            break;

                        default:
                            // we pass the data forward as-is for any other type of sysex command
                            _lastCommandError = CommandError.None;
                            _pendingResponses.Add(raw_data);
                            OnSysexReply?.Invoke(ReplyType.SysexCommand, raw_data);
                            break;
                    }

                    break;
            }
        }

        /// <summary>
        /// Send a command that does not generate a reply.
        /// This method must only be used for commands that do not generate a reply. It must not be used if only the caller is not
        /// interested in the answer.
        /// </summary>
        /// <param name="sequence">The command sequence to send</param>
        public void SendCommand(FirmataCommandSequence sequence)
        {
            if (!sequence.Validate())
            {
                throw new ArgumentException("The command sequence is invalid", nameof(sequence));
            }

            lock (_synchronisationLock)
            {
                if (_firmataStream == null)
                {
                    throw new ObjectDisposedException(nameof(FirmataDevice));
                }

                _firmataStream.Write(sequence.Sequence.ToArray());
                _bytesTransmitted += sequence.Sequence.Count;
                _firmataStream.Flush();
            }
        }

        /// <summary>
        /// Send a command and wait for a reply
        /// </summary>
        /// <param name="sequence">The command sequence, typically starting with <see cref="FirmataCommand.START_SYSEX"/> and ending with <see cref="FirmataCommand.END_SYSEX"/></param>
        /// <param name="timeout">A non-default timeout</param>
        /// <param name="isMatchingAck">A callback function that should return true if the given reply is the one this command should wait for. The default is true, because asynchronous replies
        /// are rather the exception than the rule</param>
        /// <param name="error">An error code in case of failure</param>
        /// <returns>The raw sequence of sysex reply bytes. The reply does not include the START_SYSEX byte, but it does include the terminating END_SYSEX byte. The first byte is the
        /// <see cref="FirmataSysexCommand"/> command number of the corresponding request</returns>
        public byte[] SendCommandAndWait(FirmataCommandSequence sequence, TimeSpan timeout, Func<FirmataCommandSequence, byte[], bool> isMatchingAck, out CommandError error)
        {
            if (!sequence.Validate())
            {
                throw new ArgumentException("The command sequence is invalid", nameof(sequence));
            }

            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            _firmataStream.Write(sequence.Sequence.ToArray(), 0, sequence.Sequence.Count);
            _bytesTransmitted += sequence.Sequence.Count;
            _firmataStream.Flush();

            byte[]? response;
            if (!_pendingResponses.TryRemoveElement(x => isMatchingAck(sequence, x!), timeout, out response))
            {
                throw new TimeoutException("Timeout waiting for command answer");
            }

            error = _lastCommandError;
            return response ?? throw new InvalidOperationException("Got a null reply"); // should not happen in our case
        }

        /// <summary>
        /// Send a set of command and wait for a reply
        /// </summary>
        /// <param name="sequences">The command sequences to send, typically starting with <see cref="FirmataCommand.START_SYSEX"/> and ending with <see cref="FirmataCommand.END_SYSEX"/></param>
        /// <param name="timeout">A non-default timeout</param>
        /// <param name="isMatchingAck">A callback function that should return true if the given reply is the one this command should wait for. The default is true, because asynchronous replies
        /// are rather the exception than the rule</param>
        /// <param name="errorFunc">A callback that determines a possible error in the reply message</param>
        /// <param name="error">An error code in case of failure</param>
        /// <returns>The raw sequence of sysex reply bytes. The reply does not include the START_SYSEX byte, but it does include the terminating END_SYSEX byte. The first byte is the
        /// <see cref="FirmataSysexCommand"/> command number of the corresponding request</returns>
        public bool SendCommandsAndWait(IList<FirmataCommandSequence> sequences, TimeSpan timeout, Func<FirmataCommandSequence, byte[], bool> isMatchingAck,
            Func<FirmataCommandSequence, byte[], CommandError> errorFunc, out CommandError error)
        {
            if (sequences.Any(s => s.Validate() == false))
            {
                throw new ArgumentException("At least one command sequence is invalid", nameof(sequences));
            }

            if (sequences.Count > 127)
            {
                // Because we only have 7 bits for the sequence counter.
                throw new ArgumentException("At most 127 sequences can be chained together", nameof(sequences));
            }

            if (isMatchingAck == null)
            {
                throw new ArgumentNullException(nameof(isMatchingAck));
            }

            error = CommandError.None;
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            Dictionary<FirmataCommandSequence, bool> sequencesWithAck = new();
            foreach (FirmataCommandSequence s in sequences)
            {
                sequencesWithAck.Add(s, false);
                _firmataStream.Write(s.InternalSequence, 0, s.Length);
            }

            _firmataStream.Flush();

            byte[]? response;
            do
            {
                foreach (KeyValuePair<FirmataCommandSequence, bool> s2 in sequencesWithAck)
                {
                    if (s2.Value == false && _pendingResponses.TryRemoveElement(x => isMatchingAck(s2.Key, x!), timeout, out response))
                    {
                        CommandError e = CommandError.None;
                        if (response == null)
                        {
                            error = CommandError.Aborted;
                        }
                        else if (_lastCommandError != CommandError.None)
                        {
                            error = _lastCommandError;
                        }
                        else if ((e = errorFunc(s2.Key, response)) != CommandError.None)
                        {
                            error = e;
                        }

                        sequencesWithAck[s2.Key] = true;
                        break;
                    }
                }
            }
            while (sequencesWithAck.Any(x => x.Value == false));

            return sequencesWithAck.All(x => x.Value);
        }

        /// <summary>
        /// Replaces the first occurrence of search in input with replace.
        /// </summary>
        private static string ReplaceFirst(String input, string search, string replace)
        {
            int idx = input.IndexOf(search, StringComparison.InvariantCulture);
            string output = input.Remove(idx, search.Length);
            output = output.Insert(idx, replace);
            return output;
        }

        /// <summary>
        /// Simulates a printf C statement.
        /// Note that the word size on the arduino is 16 bits, so any argument not specifying an l prefix is considered to
        /// be 16 bits only.
        /// </summary>
        /// <param name="fmt">Format string (with %d, %x, etc)</param>
        /// <param name="bytesReceived">Total bytes received</param>
        /// <param name="startOfArguments">Start of arguments (first byte of formatting parameters)</param>
        /// <returns>A formatted string</returns>
        private string PrintfFromByteStream(string fmt, in Span<byte> bytesReceived, int startOfArguments)
        {
            string output = fmt;
            while (output.Contains("%"))
            {
                int idxPercent = output.IndexOf('%');
                string type = output[idxPercent + 1].ToString();
                if (type == "l")
                {
                    type += output[idxPercent + 2];
                }

                switch (type)
                {
                    case "lx":
                    {
                        Int32 arg = BitConverter.ToInt32(bytesReceived.ToArray(), startOfArguments);
                        output = ReplaceFirst(output, "%" + type, arg.ToString("x"));
                        startOfArguments += 4;
                        break;
                    }

                    case "x":
                    {
                        Int16 arg = BitConverter.ToInt16(bytesReceived.ToArray(), startOfArguments);
                        output = ReplaceFirst(output, "%" + type, arg.ToString("x"));
                        startOfArguments += 2;
                        break;
                    }

                    case "d":
                    {
                        Int16 arg = BitConverter.ToInt16(bytesReceived.ToArray(), startOfArguments);
                        output = ReplaceFirst(output, "%" + type, arg.ToString());
                        startOfArguments += 2;
                        break;
                    }

                    case "ld":
                    {
                        Int32 arg = BitConverter.ToInt32(bytesReceived.ToArray(), startOfArguments);
                        output = ReplaceFirst(output, "%" + type, arg.ToString());
                        startOfArguments += 4;
                        break;
                    }
                }
            }

            return output;
        }

        private bool FillQueue()
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            try
            {
                Span<byte> rawData = stackalloc byte[512];

                int bytesRead = _firmataStream.Read(rawData);
                for (int i = 0; i < bytesRead; i++)
                {
                    _dataQueue.Enqueue(rawData[i]);
                }
            }
            catch (TimeoutException x)
            {
                _logger.LogWarning(x, "Input stream reported timeout - likely and incorrectly configured driver and thus ignoring.");
            }

            return _dataQueue.Count > 0;
        }

        private void InputThread()
        {
            while (!InputThreadShouldExit)
            {
                try
                {
                    ProcessInput();
                }
                catch (Exception ex)
                {
                    // If the exception happens because the stream was closed, don't print an error
                    if (!InputThreadShouldExit)
                    {
                        _logger.LogError(ex, $"Error in parser: {ex.Message}");
                        OnError?.Invoke($"Firmata protocol error: Parser exception {ex.Message}", ex);
                    }
                }
            }
        }

        public Version QueryFirmataVersion()
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            // Try a few times (because we have to make sure the receiver's input queue is properly synchronized and the device
            // has properly booted)
            for (int i = 0; i < 20; i++)
            {
                lock (_synchronisationLock)
                {
                    _dataReceived.Reset();
                    _firmataStream.WriteByte((byte)FirmataCommand.PROTOCOL_VERSION);
                    _firmataStream.Flush();
                    bool result = _dataReceived.WaitOne(TimeSpan.FromSeconds(FIRMATA_INIT_TIMEOUT_SECONDS));
                    if (result == false)
                    {
                        // Attempt to send a SYSTEM_RESET command
                        _firmataStream.WriteByte(0xFF);
                        Thread.Sleep(20);
                        continue;
                    }

                    if (_actualFirmataProtocolVersion.Major == 0)
                    {
                        // The device may be resetting itself as part of opening the serial port (this is the typical
                        // behavior of the Arduino Uno, but not of most newer boards)
                        Thread.Sleep(100);
                        continue;
                    }

                    return _actualFirmataProtocolVersion;
                }
            }

            throw new TimeoutException("Timeout waiting for firmata version");
        }

        internal Version QuerySupportedFirmataVersion()
        {
            return new Version(FIRMATA_PROTOCOL_MAJOR_VERSION, FIRMATA_PROTOCOL_MINOR_VERSION);
        }

        internal Version QueryFirmwareVersion(out string firmwareName)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            // Try 3 times (because we have to make sure the receiver's input queue is properly synchronized)
            for (int i = 0; i < 3; i++)
            {
                lock (_synchronisationLock)
                {
                    _dataReceived.Reset();
                    _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                    _firmataStream.WriteByte((byte)FirmataSysexCommand.REPORT_FIRMWARE);
                    _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                    bool result = _dataReceived.WaitOne(TimeSpan.FromSeconds(FIRMATA_INIT_TIMEOUT_SECONDS));
                    if (result == false || _firmwareVersionMajor == 0)
                    {
                        // Wait a bit until we try again.
                        Thread.Sleep(100);
                        continue;
                    }

                    firmwareName = _firmwareName;
                    return new Version(_firmwareVersionMajor, _firmwareVersionMinor);
                }
            }

            throw new TimeoutException("Timeout waiting for firmata firmware version");
        }

        internal void QueryCapabilities()
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _dataReceived.Reset();
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.CAPABILITY_QUERY);
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                bool result = _dataReceived.WaitOne(DefaultReplyTimeout);
                if (result == false)
                {
                    throw new TimeoutException("Timeout waiting for device capabilities");
                }

                _dataReceived.Reset();
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.ANALOG_MAPPING_QUERY);
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                result = _dataReceived.WaitOne(DefaultReplyTimeout);
                if (result == false)
                {
                    throw new TimeoutException("Timeout waiting for PWM port mappings");
                }
            }
        }

        private void StopThread()
        {
            InputThreadShouldExit = true;
            if (_inputThread != null)
            {
                _inputThread.Join();
                _inputThread = null;
            }
        }

        private T PerformRetries<T>(int numberOfRetries, Func<T> operation)
        {
            Exception? lastException = null;
            while (numberOfRetries-- > 0)
            {
                try
                {
                    T result = operation();
                    return result;
                }
                catch (TimeoutException x)
                {
                    lastException = x;
                    OnError?.Invoke("Timeout waiting for answer. Retries possible.", x);
                    Thread.Sleep(20);
                }
            }

            throw new TimeoutException("Timeout waiting for answer. Aborting. ", lastException);
        }

        internal void SetPinMode(int pin, SupportedMode mode)
        {
            byte firmataMode = mode.Value;
            FirmataCommandSequence s = new FirmataCommandSequence(FirmataCommand.SET_PIN_MODE);
            s.WriteByte((byte)pin);
            s.WriteByte((byte)firmataMode);
            for (int i = 0; i < 3; i++)
            {
                SendCommand(s);

                if (GetPinMode(pin) == firmataMode)
                {
                    return;
                }
            }

            throw new TimeoutException($"Unable to set Pin mode to {firmataMode}.");
        }

        internal byte GetPinMode(int pinNumber)
        {
            FirmataCommandSequence getPinModeSequence = new FirmataCommandSequence(FirmataCommand.START_SYSEX);
            getPinModeSequence.WriteByte((byte)FirmataSysexCommand.PIN_STATE_QUERY);
            getPinModeSequence.WriteByte((byte)pinNumber);
            getPinModeSequence.WriteByte((byte)FirmataCommand.END_SYSEX);

            return PerformRetries(3, () =>
            {
                byte[] response = SendCommandAndWait(getPinModeSequence, DefaultReplyTimeout, (sequence, bytes) =>
                {
                    return bytes.Length >= 4 && bytes[1] == pinNumber;
                }, out _);

                // The mode is byte 4
                if (response.Length < 4)
                {
                    throw new InvalidOperationException("Not enough data in reply");
                }

                if (response[1] != pinNumber)
                {
                    throw new InvalidOperationException(
                        "The reply didn't match the query (another port was indicated)");
                }

                return (response[2]);
            });
        }

        /// <summary>
        /// Enables digital pin reporting for all ports (one port has 8 pins)
        /// </summary>
        internal void EnableDigitalReporting()
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            int numPorts = (int)Math.Ceiling(PinConfigurations.Count / 8.0);
            lock (_synchronisationLock)
            {
                for (byte i = 0; i < numPorts; i++)
                {
                    _firmataStream.WriteByte((byte)(0xD0 + i));
                    _firmataStream.WriteByte(1);
                    _firmataStream.Flush();
                }
            }
        }

        public PinValue ReadDigitalPin(int pinNumber)
        {
            lock (_lastPinValueLock)
            {
                return _lastPinValues[pinNumber];
            }
        }

        internal void WriteDigitalPin(int pin, PinValue value)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            FirmataCommandSequence writeDigitalPin = new FirmataCommandSequence(FirmataCommand.SET_DIGITAL_VALUE);
            writeDigitalPin.WriteByte((byte)pin);
            writeDigitalPin.WriteByte(value == PinValue.High ? (byte)1 : (byte)0);

            SendCommand(writeDigitalPin);
        }

        public void SendI2cConfigCommand()
        {
            FirmataCommandSequence i2cConfigCommand = new();
            i2cConfigCommand.WriteByte((byte)FirmataSysexCommand.I2C_CONFIG);
            i2cConfigCommand.WriteByte(0);
            i2cConfigCommand.WriteByte(0);
            i2cConfigCommand.WriteByte((byte)FirmataCommand.END_SYSEX);
            SendCommand(i2cConfigCommand);
        }

        public void WriteReadI2cData(int slaveAddress,  ReadOnlySpan<byte> writeData, Span<byte> replyData)
        {
            // See documentation at https://github.com/firmata/protocol/blob/master/i2c.md
            FirmataCommandSequence i2cSequence = new FirmataCommandSequence();
            bool doWait = false;
            if (writeData.Length > 0)
            {
                i2cSequence.WriteByte((byte)FirmataSysexCommand.I2C_REQUEST);
                i2cSequence.WriteByte((byte)slaveAddress);
                // Write flag is 0, all other bits normally, too.
                i2cSequence.WriteByte(0);
                i2cSequence.WriteBytesAsTwo7bitBytes(writeData);
                i2cSequence.WriteByte((byte)FirmataCommand.END_SYSEX);
            }

            int sequenceNo = (_i2cSequence++) & 0b111;

            if (replyData.Length > 0)
            {
                doWait = true;
                if (i2cSequence.Length > 1)
                {
                    // If the above block was executed, we have to insert another START_SYSEX, otherwise it's already there
                    i2cSequence.WriteByte((byte)FirmataCommand.START_SYSEX);
                }

                i2cSequence.WriteByte((byte)FirmataSysexCommand.I2C_REQUEST);
                i2cSequence.WriteByte((byte)slaveAddress);

                // Read flag is 1, all other bits are 0. We use bits 0-2  (slave address MSB, unused in 7 bit mode) as sequence id.
                i2cSequence.WriteByte((byte)(0b1000 | sequenceNo));
                byte length = (byte)replyData.Length;
                // Only write the length of the expected data.
                // We could insert the register to read here, but we assume that has been written already (the client is responsible for that)
                i2cSequence.WriteByte((byte)(length & (uint)sbyte.MaxValue));
                i2cSequence.WriteByte((byte)(length >> 7 & sbyte.MaxValue));
                i2cSequence.WriteByte((byte)FirmataCommand.END_SYSEX);
            }

            if (doWait)
            {
                byte[] response = SendCommandAndWait(i2cSequence, TimeSpan.FromSeconds(10), (sequence, bytes) =>
                {
                    if (bytes.Length < 5)
                    {
                        return false;
                    }

                    if (bytes[0] != (byte)FirmataSysexCommand.I2C_REPLY)
                    {
                        return false;
                    }

                    if ((bytes[2] & 0b111) != sequenceNo)
                    {
                        return false;
                    }

                    return true;
                }, out _);

                if (response[0] != (byte)FirmataSysexCommand.I2C_REPLY)
                {
                    throw new IOException("Firmata protocol error: received incorrect query response");
                }

                if (response[1] != (byte)slaveAddress && slaveAddress != 0)
                {
                    throw new IOException($"Firmata protocol error: The wrong device did answer. Expected {slaveAddress} but got {response[1]}.");
                }

                // Byte 0: I2C_REPLY
                // Bytes 1 & 2: Slave address (the MSB is always 0, since we're only supporting 7-bit addresses)
                // Bytes 3 & 4: Register. Often 0, and probably not needed
                // Anything after that: reply data, with 2 bytes for each byte in the data stream
                int bytesReceived = ReassembleByteString(response, 5, response.Length - 5, replyData);

                if (replyData.Length != bytesReceived)
                {
                    throw new IOException($"Expected {replyData.Length} bytes, got only {bytesReceived}");
                }

                _numberOfConsecutiveI2cWrites = 0; // after we get a reply, we can free-fire a few times again
            }
            else
            {
                SendCommand(i2cSequence);
                // If we use a series of write-only I2C commands we have to occasionally introduce a command that requires an answer, or we flood the device's input buffer,
                // causing network retries, which under the line causes more delays than this. This problem is particularly obvious with an ESP32 in Wifi mode
                _numberOfConsecutiveI2cWrites++;
                if (_numberOfConsecutiveI2cWrites % 4 == 0)
                {
                    var pin = _supportedPinConfigurations.FirstOrDefault(x => x.PinModes.Contains(SupportedMode.I2c));
                    if (pin != null)
                    {
                        GetPinMode(pin.Pin);
                    }
                    else
                    {
                        Thread.Sleep(10); // Hopefully, this doesn't happen
                    }
                }
            }
        }

        public void SetPwmChannel(int pin, double dutyCycle)
        {
            // The arduino expects values between 0 and 255 for PWM channels.
            // The frequency cannot currently be set using the protocol
            int pwmMaxValue = _supportedPinConfigurations[pin].PwmResolutionBits; // This is 8 for most arduino boards
            pwmMaxValue = (1 << pwmMaxValue) - 1;
            int value = (int)Math.Max(0, Math.Min(dutyCycle * pwmMaxValue, pwmMaxValue));

            // At most 14 bits used?
            if (((pwmMaxValue & 0x3FFF) == pwmMaxValue) && (pin <= 15))
            {
                // Use shorthand
                FirmataCommandSequence analogMessage = new FirmataCommandSequence(FirmataCommand.ANALOG_MESSAGE, pin);
                analogMessage.WriteByte((byte)(value & (uint)sbyte.MaxValue)); // lower 7 bits
                analogMessage.WriteByte((byte)(value >> 7 & sbyte.MaxValue)); // top bit (rest unused)
                // No(!) END_SYSEX here
                SendCommand(analogMessage);
                return;
            }

            FirmataCommandSequence pwmCommandSequence = new();
            pwmCommandSequence.WriteByte((byte)FirmataSysexCommand.EXTENDED_ANALOG);
            pwmCommandSequence.WriteByte((byte)pin);
            pwmCommandSequence.WriteByte((byte)(value & (uint)sbyte.MaxValue)); // lower 7 bits
            pwmCommandSequence.WriteByte((byte)(value >> 7 & sbyte.MaxValue)); // top bit (rest unused)
            pwmCommandSequence.WriteByte((byte)FirmataCommand.END_SYSEX);
            SendCommand(pwmCommandSequence);
        }

        /// <summary>
        /// Enable analog reporting for the given physical pin
        /// </summary>
        /// <param name="pinNumber">Physical pin number</param>
        /// <param name="analogChannel">Analog channel corresponding to the given pin (Axx in arduino terminology)</param>
        internal void EnableAnalogReporting(int pinNumber, int analogChannel)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _lastAnalogValues[pinNumber] = 0; // to make sure this entry exists
                _logger.LogInformation($"Enabling analog reporting on pin {pinNumber}, Channel A{analogChannel}");
                if (analogChannel <= 15)
                {
                    _firmataStream.WriteByte((byte)((int)FirmataCommand.REPORT_ANALOG_PIN + analogChannel));
                    _firmataStream.WriteByte((byte)1);
                }
                else if (_actualFirmataProtocolVersion >= new Version(2, 7))
                {
                    // Note: Requires Protocol Version 2.7 or later
                    FirmataCommandSequence commandSequence = new();
                    commandSequence.WriteByte((byte)FirmataSysexCommand.EXTENDED_REPORT_ANALOG);
                    commandSequence.WriteByte((byte)analogChannel);
                    commandSequence.WriteByte((byte)1);
                    commandSequence.WriteByte((byte)FirmataCommand.END_SYSEX);
                    SendCommand(commandSequence);
                }
                else
                {
                    throw new NotSupportedException($"Using analog channel A{analogChannel} requires firmata protocol version 2.7 or later");
                }
            }
        }

        internal void DisableAnalogReporting(int pinNumber, int analogChannel)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _logger.LogInformation($"Disabling analog reporting on pin {pinNumber}, Channel A{analogChannel}");
                if (analogChannel <= 15)
                {
                    _firmataStream.WriteByte((byte)((int)FirmataCommand.REPORT_ANALOG_PIN + analogChannel));
                    _firmataStream.WriteByte((byte)0);
                }
                else
                {
                    FirmataCommandSequence pwmCommandSequence = new();
                    pwmCommandSequence.WriteByte((byte)FirmataSysexCommand.EXTENDED_REPORT_ANALOG);
                    pwmCommandSequence.WriteByte((byte)analogChannel);
                    pwmCommandSequence.WriteByte((byte)0);
                    pwmCommandSequence.WriteByte((byte)FirmataCommand.END_SYSEX);
                    SendCommand(pwmCommandSequence);
        }
            }
        }

        /// <summary>
        /// Check support for System variables. Should be done on init/reinit.
        /// </summary>
        /// <returns>Null if everything is ok, an error message otherwise.</returns>
        internal string? CheckSystemVariablesSupported()
        {
            if (_actualFirmataProtocolVersion.Major == 0)
            {
                // This should not happen
                _systemVariablesSupported = false;
                return "Cannot query System Variables before the firmata version is known";
            }

            if (_actualFirmataProtocolVersion < new Version(2, 7))
            {
                _systemVariablesSupported = false;
                return "Firmata protocol version 2.7 or above required";
            }

            int value = 0;
            try
            {
                // now assume true (otherwise, the method would also fail immediately)
                _systemVariablesSupported = true;
                if (!GetOrSetSystemVariable(SystemVariable.FunctionSupportCheck, -1, true, ref value))
                {
                    _systemVariablesSupported = false;
                    return "System Variable support check failed";
                }
            }
            catch (Exception x) when (x is TimeoutException || x is IOException || x is NotSupportedException)
            {
                _systemVariablesSupported = false;
                return $"GetSystemVariable(FunctionSupportCheck) returned an exception: {x.Message}";
            }

            if (value != 1)
            {
                _systemVariablesSupported = false;
                return "System variables not supported";
            }

            _systemVariablesSupported = true;

            return null;
        }

        public bool GetOrSetSystemVariable(SystemVariable variableId, int pinNumber, bool readValue, ref int value)
        {
            if (!_systemVariablesSupported)
            {
                value = 0;
                return false;
            }

            FirmataCommandSequence cmd = new FirmataCommandSequence();
            cmd.WriteByte((byte)FirmataSysexCommand.SYSTEM_VARIABLE);
            cmd.WriteByte((byte)(readValue ? 0 : 1)); // Query or set
            cmd.WriteByte(1); // Data type: Integer
            cmd.WriteByte(0); // Status (irrelevant)
            cmd.SendInt14((int)variableId);
            if (pinNumber < 127 && pinNumber >= 0)
            {
                cmd.WriteByte((byte)pinNumber);
            }
            else
            {
                cmd.WriteByte(127);
            }

            if (readValue)
            {
                // Don't send garbage in case of a read request.
                value = 0;
            }

            cmd.SendInt32(value);
            cmd.WriteByte((byte)FirmataCommand.END_SYSEX);

            byte[] reply = SendCommandAndWait(cmd, DefaultReplyTimeout, (sequence, bytes) =>
            {
                if (bytes.Length < 12)
                {
                    return false;
                }

                if (bytes[0] != (byte)FirmataSysexCommand.SYSTEM_VARIABLE)
                {
                    return false;
                }

                // Reply must be for the same pin and the same variable Id.
                int replyPin = bytes[6];
                if (replyPin == 127)
                {
                    replyPin = -1;
                }

                if (replyPin != pinNumber)
                {
                    return false;
                }

                int id = FirmataCommandSequence.DecodeInt14(bytes, 4);
                if (id != (int)variableId)
                {
                    return false;
                }

                return true;
            }, out var error);

            _lastCommandError = error;

            if (error != CommandError.None)
            {
                throw new IOException($"Unable to query {variableId}. Command returned status {error}");
            }

            SystemVariableError status = (SystemVariableError)reply[3];
            if (!CheckVariableReplyStatus(variableId, status))
            {
                value = 0;
                return false;
            }

            int replyType = reply[2];
            if (replyType != 1)
            {
                // The firmware indicates that the type of the value is something else than int. This is a problem for now.
                throw new NotSupportedException($"Firmware reports an unknown data type: {replyType}");
            }

            value = FirmataCommandSequence.DecodeInt32(reply, 7);
            return true;
        }

        private bool CheckVariableReplyStatus(SystemVariable variableId, SystemVariableError status)
        {
            switch (status)
            {
                case SystemVariableError.FieldReadOnly:
                    _logger.LogError($"Field {variableId} is read-only");
                    return false;
                case SystemVariableError.FieldWriteOnly:
                    _logger.LogError($"Field {variableId} is write-only");
                    return false;
                case SystemVariableError.GenericError:
                    _logger.LogError($"There was an error processing the request");
                    return false;
                case SystemVariableError.UnknownDataType:
                    _logger.LogError($"The data type is not supported");
                    return false;
                case SystemVariableError.UnknownVariableId:
                    _logger.LogError($"The variable id {variableId} is not supported");
                    return false;
            }

            return true;
        }

        public void EnableSpi()
        {
            FirmataCommandSequence enableSpi = new();
            enableSpi.WriteByte((byte)FirmataSysexCommand.SPI_DATA);
            enableSpi.WriteByte((byte)FirmataSpiCommand.SPI_BEGIN);
            enableSpi.WriteByte((byte)0);
            enableSpi.WriteByte((byte)FirmataCommand.END_SYSEX);
            SendCommand(enableSpi);
        }

        public void DisableSpi()
        {
            FirmataCommandSequence disableSpi = new();
            disableSpi.WriteByte((byte)FirmataSysexCommand.SPI_DATA);
            disableSpi.WriteByte((byte)FirmataSpiCommand.SPI_END);
            disableSpi.WriteByte((byte)0);
            disableSpi.WriteByte((byte)FirmataCommand.END_SYSEX);
            SendCommand(disableSpi);
        }

        public void SpiWrite(int csPin, ReadOnlySpan<byte> writeBytes, bool waitForReply)
        {
            // When the command is SPI_WRITE, the device answer is already discarded in the firmware.
            if (waitForReply)
            {
                FirmataCommandSequence command = SpiWrite(csPin, FirmataSpiCommand.SPI_WRITE_ACK, writeBytes, out byte requestId);
                byte[] response = SendCommandAndWait(command, DefaultReplyTimeout, (sequence, bytes) =>
                {
                    if (bytes.Length < 5)
                    {
                        return false;
                    }

                    if (bytes[0] != (byte)FirmataSysexCommand.SPI_DATA || bytes[1] != (byte)FirmataSpiCommand.SPI_REPLY)
                    {
                        return false;
                    }

                    if (bytes[3] != (byte)requestId)
                    {
                        return false;
                    }

                    return true;
                }, out _lastCommandError);

                if (response[0] != (byte)FirmataSysexCommand.SPI_DATA || response[1] != (byte)FirmataSpiCommand.SPI_REPLY)
                {
                    throw new IOException("Firmata protocol error: received incorrect query response");
                }

                if (response[3] != (byte)requestId)
                {
                    throw new IOException($"Firmata protocol sequence error.");
                }
            }
            else
            {
                FirmataCommandSequence command = SpiWrite(csPin, FirmataSpiCommand.SPI_WRITE, writeBytes, out _);
                SendCommand(command);
            }
        }

        public void SpiTransfer(int csPin, ReadOnlySpan<byte> writeBytes, Span<byte> readBytes)
        {
            FirmataCommandSequence command = SpiWrite(csPin, FirmataSpiCommand.SPI_TRANSFER, writeBytes, out byte requestId);
            byte[] response = SendCommandAndWait(command, DefaultReplyTimeout, (sequence, bytes) =>
            {
                if (bytes.Length < 5)
                {
                    return false;
                }

                if (bytes[0] != (byte)FirmataSysexCommand.SPI_DATA || bytes[1] != (byte)FirmataSpiCommand.SPI_REPLY)
                {
                    return false;
                }

                if (bytes[3] != (byte)requestId)
                {
                    return false;
                }

                return true;
            }, out _lastCommandError);

            if (response[0] != (byte)FirmataSysexCommand.SPI_DATA || response[1] != (byte)FirmataSpiCommand.SPI_REPLY)
            {
                throw new IOException("Firmata protocol error: received incorrect query response");
            }

            if (response[3] != (byte)requestId)
            {
                throw new IOException($"Firmata protocol sequence error.");
            }

            ReassembleByteString(response, 5, response[4] * 2, readBytes);
        }

        private FirmataCommandSequence SpiWrite(int csPin, FirmataSpiCommand command, ReadOnlySpan<byte> writeBytes, out byte requestId)
        {
            requestId = (byte)(_lastRequestId++ & 0x7F);

            FirmataCommandSequence spiCommand = new();
            spiCommand.WriteByte((byte)FirmataSysexCommand.SPI_DATA);
            spiCommand.WriteByte((byte)command);
            spiCommand.WriteByte((byte)(csPin << 3)); // Device ID / channel
            spiCommand.WriteByte(requestId);
            spiCommand.WriteByte(1); // Deselect CS after transfer (yes)
            spiCommand.WriteByte((byte)writeBytes.Length);
            spiCommand.Write(Encoder7Bit.Encode(writeBytes));
            spiCommand.WriteByte((byte)FirmataCommand.END_SYSEX);
            return spiCommand;
        }

        public void SetAnalogInputSamplingInterval(TimeSpan interval)
        {
            int millis = (int)interval.TotalMilliseconds;
            FirmataCommandSequence seq = new();
            seq.WriteByte((byte)FirmataSysexCommand.SAMPLING_INTERVAL);
            int value = millis;
            seq.WriteByte((byte)(value & (uint)sbyte.MaxValue)); // lower 7 bits
            seq.WriteByte((byte)(value >> 7 & sbyte.MaxValue)); // top bits
            seq.WriteByte((byte)FirmataCommand.END_SYSEX);
            SendCommand(seq);
        }

        public void ConfigureSpiDevice(SpiConnectionSettings connectionSettings)
        {
            if (connectionSettings.ChipSelectLine >= 15)
            {
                // this limit is currently required because we derive the device id from the CS line, and that one has only 4 bits
                throw new NotSupportedException("Only pins <=15 are allowed as CS line");
            }

            if (_firmwareVersion <= new Version(2, 11))
            {
                // we could leverage this, if needed, by using the older data encoding
                throw new NotSupportedException("This library requires firmware version 2.12 or later for SPI transfers");
            }

            int deviceId = connectionSettings.ChipSelectLine;
            FirmataCommandSequence spiConfigSequence = new();
            spiConfigSequence.WriteByte((byte)FirmataSysexCommand.SPI_DATA);
            spiConfigSequence.WriteByte((byte)FirmataSpiCommand.SPI_DEVICE_CONFIG);
            byte deviceIdChannel = (byte)(deviceId << 3 | (connectionSettings.BusId & 0x7));
            spiConfigSequence.WriteByte((byte)(deviceIdChannel));
            int dataMode = 0;
            if (connectionSettings.DataFlow == DataFlow.MsbFirst)
            {
                dataMode = 1;
            }

            int mode = ((int)connectionSettings.Mode) << 1;
            dataMode |= mode;
            dataMode |= 0x8; // Use fast transfer mode

            spiConfigSequence.WriteByte((byte)dataMode);
            int clockSpeed = connectionSettings.ClockFrequency;
            if (clockSpeed <= 0)
            {
                clockSpeed = 1_000_000;
            }

            spiConfigSequence.SendInt32(clockSpeed);
            spiConfigSequence.WriteByte(0); // Word size (default = 8)
            spiConfigSequence.WriteByte(1); // Default CS pin control (enable)
            spiConfigSequence.WriteByte((byte)(connectionSettings.ChipSelectLine));
            spiConfigSequence.WriteByte((byte)FirmataCommand.END_SYSEX);
            SendCommand(spiConfigSequence);
        }

        internal uint GetAnalogRawValue(int pinNumber)
        {
            lock (_lastAnalogValueLock)
            {
                return _lastAnalogValues[pinNumber];
            }
        }

        /// <summary>
        /// Firmata uses 2 bytes to encode 8-bit data, because byte values with the top bit set
        /// are reserved for commands. This decodes such data chunks.
        /// </summary>
        private int ReassembleByteString(IList<byte> byteStream, int startIndex, int length, Span<byte> reply)
        {
            int num;
            if (reply.Length < length / 2)
            {
                length = reply.Length * 2;
            }

            for (num = 0; num < length / 2; ++num)
            {
                reply[num] = (byte)(byteStream[startIndex + (num * 2)] |
                                    byteStream[startIndex + (num * 2) + 1] << 7);
            }

            return length / 2;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                InputThreadShouldExit = true;

                lock (_synchronisationLock)
                {
                    if (_firmataStream != null)
                    {
                        _firmataStream.Close();
                    }

                    _firmataStream = null;
                }

                StopThread();

                if (_dataReceived != null)
                {
                    _dataReceived.Dispose();
                    _dataReceived = null!;
                }

                OnError = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SendSoftwareReset()
        {
            lock (_synchronisationLock)
            {
                _firmataStream?.WriteByte(0xFF);
            }
        }
    }
}

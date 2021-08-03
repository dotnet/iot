// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
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
        internal static readonly TimeSpan DefaultReplyTimeout = TimeSpan.FromMilliseconds(500);

        private byte _firmwareVersionMajor;
        private byte _firmwareVersionMinor;
        private byte _actualFirmataProtocolMajorVersion;
        private byte _actualFirmataProtocolMinorVersion;

        private int _lastRequestId;

        private string _firmwareName;
        private Stream? _firmataStream;
        private Thread? _inputThread;
        private bool _inputThreadShouldExit;
        private List<SupportedPinConfiguration> _supportedPinConfigurations;
        private IList<byte> _lastResponse;
        private List<PinValue> _lastPinValues;
        private Dictionary<int, uint> _lastAnalogValues;
        private object _lastPinValueLock;
        private object _lastAnalogValueLock;
        private object _synchronisationLock;
        private Queue<byte> _dataQueue;
        private StringBuilder _lastRawLine;

        // Event used when waiting for answers (i.e. after requesting firmware version)
        private AutoResetEvent _dataReceived;

        public event PinChangeEventHandler? DigitalPortValueUpdated;

        public event AnalogPinValueUpdated? AnalogPinValueUpdated;

        public event Action<string, Exception?>? OnError;

        public event Action<ReplyType, byte[]>? OnSysexReply;

        public FirmataDevice()
        {
            _firmwareVersionMajor = 0;
            _firmwareVersionMinor = 0;
            _firmataStream = null;
            _inputThreadShouldExit = false;
            _dataReceived = new AutoResetEvent(false);
            _supportedPinConfigurations = new List<SupportedPinConfiguration>();
            _synchronisationLock = new object();
            _lastPinValues = new List<PinValue>();
            _lastPinValueLock = new object();
            _lastAnalogValues = new Dictionary<int, uint>();
            _lastAnalogValueLock = new object();
            _dataQueue = new Queue<byte>();
            _lastResponse = new List<byte>();
            _lastRequestId = 1;
            _firmwareName = string.Empty;
            _lastRawLine = new StringBuilder();
        }

        internal List<SupportedPinConfiguration> PinConfigurations
        {
            get
            {
                return _supportedPinConfigurations;
            }
        }

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

            _inputThreadShouldExit = false;

            _inputThread = new Thread(InputThread);
            _inputThread.Start();

            // Reset device, in case it is still sending data from an aborted process
            _firmataStream.WriteByte((byte)FirmataCommand.SYSTEM_RESET);
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
                        return;
                    }
                }

                data = _dataQueue.Dequeue();
                // OnError?.Invoke($"0x{data:X}", null);
                // if no data was available, check for timeout
                if (data == 0xFFFF)
                {
                    // get elapsed seconds, given as a double with resolution in nanoseconds
                    var elapsed = timeout_start.Elapsed;

                    if (elapsed > DefaultReplyTimeout)
                    {
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
                    if (_actualFirmataProtocolMajorVersion != 0)
                    {
                        // Firmata sends this message automatically after a device reset (if you press the reset button on the arduino)
                        // If we know the version already, this is unexpected.
                        OnError?.Invoke("The device was unexpectedly reset. Please restart the communication.", null);
                    }

                    _actualFirmataProtocolMajorVersion = message[0];
                    _actualFirmataProtocolMinorVersion = message[1];
                    _dataReceived.Set();

                    return;

                case FirmataCommand.ANALOG_MESSAGE:
                    // report analog commands store the pin number in the lower nibble of the command byte, the value is split over two 7-bit bytes
                    // AnalogValueUpdated(this,
                    //    new CallbackEventArgs(lower_nibble, (ushort)(message[0] | (message[1] << 7))));
                    {
                        int pin = lower_nibble;
                        uint value = (uint)(message[0] | (message[1] << 7));
                        lock (_lastAnalogValueLock)
                        {
                            _lastAnalogValues[pin] = value;
                        }

                        AnalogPinValueUpdated?.Invoke(pin, value);
                    }

                    break;

                case FirmataCommand.DIGITAL_MESSAGE:
                    // digital messages store the port number in the lower nibble of the command byte, the port value is split over two 7-bit bytes
                    // Each port corresponds to 8 pins
                    {
                        int offset = lower_nibble * 8;
                        ushort pinValues = (ushort)(message[0] | (message[1] << 7));
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
                        return;
                    }

                    // retrieve the raw data array & extract the extended-command byte
                    var raw_data = message.ToArray();
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
                                int stringLength = (raw_data.Length - 3) / 2;
                                Span<byte> bytesReceived = stackalloc byte[stringLength];
                                ReassembleByteString(raw_data, 3, stringLength * 2, bytesReceived);

                                _firmwareName = Encoding.ASCII.GetString(bytesReceived);
                                _dataReceived.Set();
                            }

                            return;

                        case FirmataSysexCommand.STRING_DATA:
                            {
                                // condense back into 1-byte data
                                int stringLength = (raw_data.Length - 1) / 2;
                                Span<byte> bytesReceived = stackalloc byte[stringLength];
                                ReassembleByteString(raw_data, 1, stringLength * 2, bytesReceived);

                                string message1 = Encoding.ASCII.GetString(bytesReceived);
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
                                var currentPin = new SupportedPinConfiguration(0);
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
                                    SupportedMode? sm = ArduinoBoard.KnownModes.FirstOrDefault(x => x.Value == mode);
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
                        case FirmataSysexCommand.I2C_REPLY:

                            _lastResponse = raw_data;
                            _dataReceived.Set();
                            break;

                        case FirmataSysexCommand.SPI_DATA:
                            _lastResponse = raw_data;
                            _dataReceived.Set();
                            break;

                        default:
                            // we pass the data forward as-is for any other type of sysex command
                            _lastResponse = raw_data; // the instance is constant, so we can just remember the pointer
                            OnSysexReply?.Invoke(ReplyType.SysexCommand, raw_data);
                            _dataReceived.Set();
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

                // Use an explicit iteration, avoids a memory allocation here
                for (int i = 0; i < sequence.Sequence.Count; i++)
                {
                    _firmataStream.WriteByte(sequence.Sequence[i]);
                }

                _firmataStream.Flush();
            }
        }

        /// <summary>
        /// Send a command and wait for a reply
        /// </summary>
        /// <param name="sequence">The command sequence, typically starting with <see cref="FirmataCommand.START_SYSEX"/> and ending with <see cref="FirmataCommand.END_SYSEX"/></param>
        /// <returns>The raw sequence of sysex reply bytes. The reply does not include the START_SYSEX byte, but it does include the terminating END_SYSEX byte. The first byte is the
        /// <see cref="FirmataSysexCommand"/> command number of the corresponding request</returns>
        public byte[] SendCommandAndWait(FirmataCommandSequence sequence)
        {
            return SendCommandAndWait(sequence, DefaultReplyTimeout);
        }

        /// <summary>
        /// Send a command and wait for a reply
        /// </summary>
        /// <param name="sequence">The command sequence, typically starting with <see cref="FirmataCommand.START_SYSEX"/> and ending with <see cref="FirmataCommand.END_SYSEX"/></param>
        /// <param name="timeout">A non-default timeout</param>
        /// <returns>The raw sequence of sysex reply bytes. The reply does not include the START_SYSEX byte, but it does include the terminating END_SYSEX byte. The first byte is the
        /// <see cref="FirmataSysexCommand"/> command number of the corresponding request</returns>
        public byte[] SendCommandAndWait(FirmataCommandSequence sequence, TimeSpan timeout)
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

                _dataReceived.Reset();
                // Use an explicit iteration, avoids a memory allocation here
                for (int i = 0; i < sequence.Sequence.Count; i++)
                {
                    _firmataStream.WriteByte(sequence.Sequence[i]);
                }

                _firmataStream.Flush();
                bool result = _dataReceived.WaitOne(timeout);
                if (result == false)
                {
                    throw new TimeoutException("Timeout waiting for command answer");
                }

                return _lastResponse.ToArray();
            }
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

            Span<byte> rawData = stackalloc byte[100];

            int bytesRead = _firmataStream.Read(rawData);
            for (int i = 0; i < bytesRead; i++)
            {
                _dataQueue.Enqueue(rawData[i]);
            }

            return _dataQueue.Count > 0;
        }

        private void InputThread()
        {
            while (!_inputThreadShouldExit)
            {
                try
                {
                    ProcessInput();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Firmata protocol error: Parser exception {ex.Message}", ex);
                }
            }
        }

        public Version QueryFirmataVersion()
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

                    if (_actualFirmataProtocolMajorVersion == 0)
                    {
                        // Something went wrong
                        Thread.Sleep(20);
                        continue;
                    }

                    return new Version(_actualFirmataProtocolMajorVersion, _actualFirmataProtocolMinorVersion);
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
                    if (result == false)
                    {
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
            _inputThreadShouldExit = true;
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
                }
            }

            throw new TimeoutException("Timeout waiting for answer. Aborting. ", lastException);
        }

        internal void SetPinMode(int pin, byte firmataMode)
        {
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
                var response = SendCommandAndWait(getPinModeSequence);

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
            if (writeData != null && writeData.Length > 0)
            {
                i2cSequence.WriteByte((byte)FirmataSysexCommand.I2C_REQUEST);
                i2cSequence.WriteByte((byte)slaveAddress);
                i2cSequence.WriteByte(0); // Write flag is 0, all other bits as well
                i2cSequence.AddValuesAsTwo7bitBytes(writeData);
                i2cSequence.WriteByte((byte)FirmataCommand.END_SYSEX);
            }

            if (replyData != null && replyData.Length > 0)
            {
                doWait = true;
                if (i2cSequence.Length > 1)
                {
                    // If the above block was executed, we have to insert another START_SYSEX, otherwise it's already there
                    i2cSequence.WriteByte((byte)FirmataCommand.START_SYSEX);
                }

                i2cSequence.WriteByte((byte)FirmataSysexCommand.I2C_REQUEST);
                i2cSequence.WriteByte((byte)slaveAddress);
                i2cSequence.WriteByte(0b1000); // Read flag is 1, all other bits are 0
                byte length = (byte)replyData.Length;
                // Only write the length of the expected data.
                // We could insert the register to read here, but we assume that has been written already (the client is responsible for that)
                i2cSequence.WriteByte((byte)(length & (uint)sbyte.MaxValue));
                i2cSequence.WriteByte((byte)(length >> 7 & sbyte.MaxValue));
                i2cSequence.WriteByte((byte)FirmataCommand.END_SYSEX);
            }

            if (doWait)
            {
                var response = SendCommandAndWait(i2cSequence);

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
            }
            else
            {
                SendCommand(i2cSequence);
            }
        }

        public void SetPwmChannel(int pin, double dutyCycle)
        {
            FirmataCommandSequence pwmCommandSequence = new();
            pwmCommandSequence.WriteByte((byte)FirmataSysexCommand.EXTENDED_ANALOG);
            pwmCommandSequence.WriteByte((byte)pin);
            // The arduino expects values between 0 and 255 for PWM channels.
            // The frequency cannot be set.
            int pwmMaxValue = _supportedPinConfigurations[pin].PwmResolutionBits; // This is 8 for most arduino boards
            pwmMaxValue = (1 << pwmMaxValue) - 1;
            int value = (int)Math.Max(0, Math.Min(dutyCycle * pwmMaxValue, pwmMaxValue));
            pwmCommandSequence.WriteByte((byte)(value & (uint)sbyte.MaxValue)); // lower 7 bits
            pwmCommandSequence.WriteByte((byte)(value >> 7 & sbyte.MaxValue)); // top bit (rest unused)
            pwmCommandSequence.WriteByte((byte)FirmataCommand.END_SYSEX);
            SendCommand(pwmCommandSequence);
        }

        /// <summary>
        /// This takes the pin number in Arduino's own Analog numbering scheme. So A0 shall be specifed as 0
        /// </summary>
        internal void EnableAnalogReporting(int pinNumber)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _lastAnalogValues[pinNumber] = 0; // to make sure this entry exists
                _firmataStream.WriteByte((byte)((int)FirmataCommand.REPORT_ANALOG_PIN + pinNumber));
                _firmataStream.WriteByte((byte)1);
            }
        }

        internal void DisableAnalogReporting(int pinNumber)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _firmataStream.WriteByte((byte)((int)FirmataCommand.REPORT_ANALOG_PIN + pinNumber));
                _firmataStream.WriteByte((byte)0);
            }
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

        public void SpiWrite(int csPin, ReadOnlySpan<byte> writeBytes)
        {
            // When the command is SPI_WRITE, the device answer is already discarded in the firmware.
            var command = SpiWrite(csPin, FirmataSpiCommand.SPI_WRITE, writeBytes, out _);
            SendCommand(command);
        }

        public void SpiTransfer(int csPin, ReadOnlySpan<byte> writeBytes, Span<byte> readBytes)
        {
            var command = SpiWrite(csPin, FirmataSpiCommand.SPI_TRANSFER, writeBytes, out byte requestId);
            var response = SendCommandAndWait(command);

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
            spiCommand.AddValuesAsTwo7bitBytes(writeBytes);
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

            FirmataCommandSequence spiConfigSequence = new();
            spiConfigSequence.WriteByte((byte)FirmataSysexCommand.SPI_DATA);
            spiConfigSequence.WriteByte((byte)FirmataSpiCommand.SPI_DEVICE_CONFIG);
            byte deviceIdChannel = (byte)(connectionSettings.ChipSelectLine << 3);
            spiConfigSequence.WriteByte((byte)(deviceIdChannel));
            spiConfigSequence.WriteByte((byte)1);
            int clockSpeed = 1_000_000; // Hz
            spiConfigSequence.WriteByte((byte)(clockSpeed & 0x7F));
            spiConfigSequence.WriteByte((byte)((clockSpeed >> 7) & 0x7F));
            spiConfigSequence.WriteByte((byte)((clockSpeed >> 15) & 0x7F));
            spiConfigSequence.WriteByte((byte)((clockSpeed >> 22) & 0x7F));
            spiConfigSequence.WriteByte((byte)((clockSpeed >> 29) & 0x7F));
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
                _inputThreadShouldExit = true;

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
    }
}

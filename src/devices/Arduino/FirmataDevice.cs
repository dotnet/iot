// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
    /// <summary>
    /// This delegate is used to fire digital value changed events.
    /// </summary>
    public delegate void DigitalPinValueChanged(int pin, PinValue newValue);

    internal delegate void AnalogPinValueUpdated(int pin, uint rawValue);

    internal sealed class FirmataDevice : IDisposable
    {
        private const byte FIRMATA_PROTOCOL_MAJOR_VERSION = 2;
        private const byte FIRMATA_PROTOCOL_MINOR_VERSION = 5; // 2.5 works, but 2.6 is recommended
        private const int FIRMATA_INIT_TIMEOUT_SECONDS = 2;
        private static readonly TimeSpan DefaultReplyTimeout = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan ProgrammingTimeout = TimeSpan.FromMilliseconds(900);

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

        private ExecutionError _lastIlExecutionError;

        // Event used when waiting for answers (i.e. after requesting firmware version)
        private AutoResetEvent _dataReceived;
        public event Action<byte, MethodState, int, IList<byte>>? OnSchedulerReply;

        public event DigitalPinValueChanged? DigitalPortValueUpdated;

        public event AnalogPinValueUpdated? AnalogPinValueUpdated;

        public event Action<string, Exception?>? OnError;

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
            _lastIlExecutionError = 0;
            _firmwareName = string.Empty;
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

        public void Close()
        {
            StopThread();
            lock (_synchronisationLock)
            {
                if (_firmataStream != null)
                {
                    _firmataStream.Close();
                }

                _firmataStream = null;
            }

            if (_dataReceived != null)
            {
                _dataReceived.Dispose();
                _dataReceived = null!;
            }
        }

        /// <summary>
        /// Used where?
        /// </summary>
        private void SendString(byte command, string message)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            byte[] bytes = Encoding.Unicode.GetBytes(message);
            lock (_synchronisationLock)
            {
                _firmataStream.WriteByte(240);
                _firmataStream.WriteByte((byte)(command & (uint)sbyte.MaxValue));
                SendValuesAsTwo7bitBytes(bytes);
                _firmataStream.WriteByte(247);
                _firmataStream.Flush();
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
                                    _lastPinValues[i + offset] = newValue;
                                    // TODO: The callback should not be within the lock
                                    DigitalPortValueUpdated?.Invoke(i + offset, newValue);
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
                                    switch ((SupportedMode)mode)
                                    {
                                        default:
                                            currentPin.PinModes.Add((SupportedMode)mode);
                                            break;
                                        case SupportedMode.ANALOG_INPUT:
                                            currentPin.PinModes.Add(SupportedMode.ANALOG_INPUT);
                                            currentPin.AnalogInputResolutionBits = resolution;
                                            break;
                                        case SupportedMode.PWM:
                                            currentPin.PinModes.Add(SupportedMode.PWM);
                                            currentPin.PwmResolutionBits = resolution;
                                            break;
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

                        case FirmataSysexCommand.DHT_SENSOR_DATA_REQUEST:
                        case FirmataSysexCommand.PIN_STATE_RESPONSE:
                            _lastResponse = raw_data; // the instance is constant, so we can just remember the pointer
                            _dataReceived.Set();
                            break;

                        case FirmataSysexCommand.SCHEDULER_DATA:
                            {
                                if (raw_data.Length == 4 && raw_data[1] == (byte)ExecutorCommand.Ack)
                                {
                                    // Just an ack for a programming command.
                                    _lastIlExecutionError = 0;
                                    _dataReceived.Set();
                                    return;
                                }

                                if (raw_data.Length == 4 && raw_data[1] == (byte)ExecutorCommand.Nack)
                                {
                                    // This is a Nack
                                    _lastIlExecutionError = (ExecutionError)raw_data[3];
                                    _dataReceived.Set();
                                    return;
                                }

                                // Data from real-time methods
                                if (raw_data.Length < 7)
                                {
                                    OnError?.Invoke("Code execution returned invalid result or state", null);
                                    break;
                                }

                                int numArgs = raw_data[3];
                                Span<byte> bytesReceived = stackalloc byte[numArgs * 4];
                                ReassembleByteString(raw_data, 4, numArgs * 8, bytesReceived);

                                OnSchedulerReply?.Invoke(raw_data[1], (MethodState)raw_data[2], numArgs, bytesReceived.ToArray());
                                break;
                            }

                        default:

                            // we pass the data forward as-is for any other type of sysex command
                            break;
                    }

                    break;
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

            Span<byte> rawData = stackalloc byte[32];
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
                        continue;
                    }

                    return new Version(_actualFirmataProtocolMajorVersion, _actualFirmataProtocolMinorVersion);
                }
            }

            throw new TimeoutException("Timeout waiting for firmata version");
        }

        public Version QuerySupportedFirmataVersion()
        {
            return new Version(FIRMATA_PROTOCOL_MAJOR_VERSION, FIRMATA_PROTOCOL_MINOR_VERSION);
        }

        public Version QueryFirmwareVersion(out string firmwareName)
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

        public void QueryCapabilities()
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

        public void SetPinMode(int pin, SupportedMode firmataMode)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            for (int i = 0; i < 3; i++)
            {
                lock (_synchronisationLock)
                {
                    _firmataStream.WriteByte((byte)FirmataCommand.SET_PIN_MODE);
                    _firmataStream.WriteByte((byte)pin);
                    _firmataStream.WriteByte((byte)firmataMode);
                    _firmataStream.Flush();
                }

                if (GetPinMode(pin) == firmataMode)
                {
                    return;
                }
            }

            throw new TimeoutException($"Unable to set Pin mode to {firmataMode}. Looks like a communication problem.");
        }

        public SupportedMode GetPinMode(int pinNumber)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            return PerformRetries(3, () =>
            {
                lock (_synchronisationLock)
                {
                    _dataReceived.Reset();
                    _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                    _firmataStream.WriteByte((byte)FirmataSysexCommand.PIN_STATE_QUERY);
                    _firmataStream.WriteByte((byte)pinNumber);
                    _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                    _firmataStream.Flush();
                    bool result = _dataReceived.WaitOne(DefaultReplyTimeout);
                    if (result == false)
                    {
                        throw new TimeoutException("Timeout waiting for pin mode.");
                    }

                    // The mode is byte 4
                    if (_lastResponse.Count < 4)
                    {
                        throw new InvalidOperationException("Not enough data in reply");
                    }

                    if (_lastResponse[1] != pinNumber)
                    {
                        throw new InvalidOperationException(
                            "The reply didn't match the query (another port was indicated)");
                    }

                    SupportedMode mode = (SupportedMode)(_lastResponse[2]);
                    return mode;
                }
            });
        }

        /// <summary>
        /// Enables digital pin reporting for all ports (one port has 8 pins)
        /// </summary>
        public void EnableDigitalReporting()
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

        public PinValue GetDigitalPinState(int pinNumber)
        {
            lock (_lastPinValueLock)
            {
                return _lastPinValues[pinNumber];
            }
        }

        public void WriteDigitalPin(int pin, PinValue value)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _firmataStream.WriteByte((byte)FirmataCommand.SET_DIGITAL_VALUE);
                _firmataStream.WriteByte((byte)pin);
                _firmataStream.WriteByte((byte)(value == PinValue.High ? 1 : 0));
                _firmataStream.Flush();
            }
        }

        public void SendI2cConfigCommand()
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                // The command is mandatory, even if the argument is typically ignored
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.I2C_CONFIG);
                _firmataStream.WriteByte(0);
                _firmataStream.WriteByte(0);
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                _firmataStream.Flush();
            }
        }

        public void WriteReadI2cData(int slaveAddress,  ReadOnlySpan<byte> writeData, Span<byte> replyData)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            // See documentation at https://github.com/firmata/protocol/blob/master/i2c.md
            lock (_synchronisationLock)
            {
                if (writeData != null && writeData.Length > 0)
                {
                    _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                    _firmataStream.WriteByte((byte)FirmataSysexCommand.I2C_REQUEST);
                    _firmataStream.WriteByte((byte)slaveAddress);
                    _firmataStream.WriteByte(0); // Write flag is 0, all other bits as well
                    SendValuesAsTwo7bitBytes(writeData);
                    _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                    _firmataStream.Flush();
                }

                if (replyData != null && replyData.Length > 0)
                {
                    _dataReceived.Reset();
                    _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                    _firmataStream.WriteByte((byte)FirmataSysexCommand.I2C_REQUEST);
                    _firmataStream.WriteByte((byte)slaveAddress);
                    _firmataStream.WriteByte(0b1000); // Read flag is 1, all other bits are 0
                    byte length = (byte)replyData.Length;
                    // Only write the length of the expected data.
                    // We could insert the register to read here, but we assume that has been written already (the client is responsible for that)
                    _firmataStream.WriteByte((byte)(length & (uint)sbyte.MaxValue));
                    _firmataStream.WriteByte((byte)(length >> 7 & sbyte.MaxValue));
                    _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                    _firmataStream.Flush();
                    bool result = _dataReceived.WaitOne(DefaultReplyTimeout);
                    if (result == false)
                    {
                        throw new TimeoutException("Timeout waiting for device reply");
                    }

                    if (_lastResponse[0] != (byte)FirmataSysexCommand.I2C_REPLY)
                    {
                        throw new IOException("Firmata protocol error: received incorrect query response");
                    }

                    if (_lastResponse[1] != (byte)slaveAddress && slaveAddress != 0)
                    {
                        throw new IOException($"Firmata protocol error: The wrong device did answer. Expected {slaveAddress} but got {_lastResponse[1]}.");
                    }

                    // Byte 0: I2C_REPLY
                    // Bytes 1 & 2: Slave address (the MSB is always 0, since we're only supporting 7-bit addresses)
                    // Bytes 3 & 4: Register. Often 0, and probably not needed
                    // Anything after that: reply data, with 2 bytes for each byte in the data stream
                    int bytesReceived = ReassembleByteString(_lastResponse, 5, _lastResponse.Count - 5, replyData);

                    if (replyData.Length != bytesReceived)
                    {
                        throw new IOException($"Expected {replyData.Length} bytes, got only {bytesReceived}");
                    }
                }
            }
        }

        public void SetPwmChannel(int pin, double dutyCycle)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.EXTENDED_ANALOG);
                _firmataStream.WriteByte((byte)pin);
                // The arduino expects values between 0 and 255 for PWM channels.
                // The frequency cannot be set.
                int pwmMaxValue = _supportedPinConfigurations[pin].PwmResolutionBits; // This is 8 for most arduino boards
                pwmMaxValue = (1 << pwmMaxValue) - 1;
                int value = (int)Math.Max(0, Math.Min(dutyCycle * pwmMaxValue, pwmMaxValue));
                _firmataStream.WriteByte((byte)(value & (uint)sbyte.MaxValue)); // lower 7 bits
                _firmataStream.WriteByte((byte)(value >> 7 & sbyte.MaxValue)); // top bit (rest unused)
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                _firmataStream.Flush();
            }
        }

        /// <summary>
        /// This takes the pin number in Arduino's own Analog numbering scheme. So A0 shall be specifed as 0
        /// </summary>
        public void EnableAnalogReporting(int pinNumber)
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

        public void DisableAnalogReporting(int pinNumber)
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
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.SPI_DATA);
                _firmataStream.WriteByte((byte)FirmataSpiCommand.SPI_BEGIN);
                _firmataStream.WriteByte((byte)0);
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
            }
        }

        public void DisableSpi()
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.SPI_DATA);
                _firmataStream.WriteByte((byte)FirmataSpiCommand.SPI_END);
                _firmataStream.WriteByte((byte)0);
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
            }
        }

        public void SpiWrite(int csPin, ReadOnlySpan<byte> writeBytes)
        {
            lock (_synchronisationLock)
            {
                SpiWrite(csPin, FirmataSpiCommand.SPI_WRITE, writeBytes);
            }
        }

        public void SpiTransfer(int csPin, ReadOnlySpan<byte> writeBytes, Span<byte> readBytes)
        {
            lock (_synchronisationLock)
            {
                _dataReceived.Reset();
                byte requestId = SpiWrite(csPin, FirmataSpiCommand.SPI_TRANSFER, writeBytes);
                bool result = _dataReceived.WaitOne(DefaultReplyTimeout);
                if (result == false)
                {
                    throw new TimeoutException("Timeout waiting for device reply");
                }

                if (_lastResponse[0] != (byte)FirmataSysexCommand.SPI_DATA || _lastResponse[1] != (byte)FirmataSpiCommand.SPI_REPLY)
                {
                    throw new IOException("Firmata protocol error: received incorrect query response");
                }

                if (_lastResponse[3] != (byte)requestId)
                {
                    throw new IOException($"Firmata protocol sequence error.");
                }

                ReassembleByteString(_lastResponse, 5, _lastResponse[4] * 2, readBytes);
            }
        }

        private byte SpiWrite(int csPin, FirmataSpiCommand command, ReadOnlySpan<byte> writeBytes)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            byte requestId = (byte)(_lastRequestId++ & 0x7F);
            _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
            _firmataStream.WriteByte((byte)FirmataSysexCommand.SPI_DATA);
            _firmataStream.WriteByte((byte)command);
            _firmataStream.WriteByte((byte)(csPin << 3)); // Device ID / channel
            _firmataStream.WriteByte(requestId);
            _firmataStream.WriteByte(1); // Deselect CS after transfer (yes)
            _firmataStream.WriteByte((byte)writeBytes.Length);
            SendValuesAsTwo7bitBytes(writeBytes);
            _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
            return requestId;
        }

        public void SetSamplingInterval(TimeSpan interval)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            int millis = (int)interval.TotalMilliseconds;
            lock (_synchronisationLock)
            {
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.SAMPLING_INTERVAL);
                int value = millis;
                _firmataStream.WriteByte((byte)(value & (uint)sbyte.MaxValue)); // lower 7 bits
                _firmataStream.WriteByte((byte)(value >> 7 & sbyte.MaxValue)); // top bits
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                _firmataStream.Flush();
            }
        }

        public void ConfigureSpiDevice(SpiConnectionSettings connectionSettings)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            if (connectionSettings.ChipSelectLine >= 15)
            {
                // this is currently because we derive the device id from the CS line, and that one has only 4 bits
                throw new NotSupportedException("Only pins <=15 are allowed as CS line");
            }

            lock (_synchronisationLock)
            {
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.SPI_DATA);
                _firmataStream.WriteByte((byte)FirmataSpiCommand.SPI_DEVICE_CONFIG);
                byte deviceIdChannel = (byte)(connectionSettings.ChipSelectLine << 3);
                _firmataStream.WriteByte((byte)(deviceIdChannel));
                _firmataStream.WriteByte((byte)1);
                int clockSpeed = 1_000_000; // Hz
                _firmataStream.WriteByte((byte)(clockSpeed & 0x7F));
                _firmataStream.WriteByte((byte)((clockSpeed >> 7) & 0x7F));
                _firmataStream.WriteByte((byte)((clockSpeed >> 15) & 0x7F));
                _firmataStream.WriteByte((byte)((clockSpeed >> 22) & 0x7F));
                _firmataStream.WriteByte((byte)((clockSpeed >> 29) & 0x7F));
                _firmataStream.WriteByte(0); // Word size (default = 8)
                _firmataStream.WriteByte(1); // Default CS pin control (enable)
                _firmataStream.WriteByte((byte)(connectionSettings.ChipSelectLine));
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                _firmataStream.Flush();
            }
        }

        public bool TryReadDht(int pinNumber, int dhtType, out Temperature temperature, out Ratio humidity)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            temperature = default;
            humidity = default;
            lock (_synchronisationLock)
            {
                _dataReceived.Reset();
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.DHT_SENSOR_DATA_REQUEST);
                _firmataStream.WriteByte((byte)dhtType);
                _firmataStream.WriteByte((byte)pinNumber);
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                _firmataStream.Flush();

                bool result = _dataReceived.WaitOne(DefaultReplyTimeout);
                if (result == false)
                {
                    throw new TimeoutException("Timeout waiting for device reply");
                }

                // Command, pin number and 2x2 bytes data (+ END_SYSEX byte)
                if (_lastResponse.Count < 7)
                {
                    return false;
                }

                if (_lastResponse[0] != (byte)FirmataSysexCommand.DHT_SENSOR_DATA_REQUEST && _lastResponse[1] != 0)
                {
                    return false;
                }

                int t = _lastResponse[3] | _lastResponse[4] << 7;
                int h = _lastResponse[5] | _lastResponse[6] << 7;

                temperature = Temperature.FromDegreesCelsius(t / 10.0);
                humidity = Ratio.FromPercent(h / 10.0);
            }

            return true;
        }

        public uint GetAnalogRawValue(int pinNumber)
        {
            lock (_lastAnalogValueLock)
            {
                return _lastAnalogValues[pinNumber];
            }
        }

        private void WaitAndHandleIlCommandReply(ExecutorCommand command)
        {
            bool result = _dataReceived.WaitOne(ProgrammingTimeout);
            if (result == false)
            {
                throw new TimeoutException($"Arduino failed to accept IL command {command}.");
            }

            if (_lastIlExecutionError != 0)
            {
                throw new TaskSchedulerException($"Task scheduler method returned state {_lastIlExecutionError}.");
            }
        }

        public void SendMethodIlCode(byte methodIndex, byte[] byteCode)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                const int BYTES_PER_PACKET = 20;
                int codeIndex = 0;
                while (codeIndex < byteCode.Length)
                {
                    _dataReceived.Reset();
                    _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                    _firmataStream.WriteByte((byte)FirmataSysexCommand.SCHEDULER_DATA);
                    _firmataStream.WriteByte((byte)0xFF); // IL data
                    _firmataStream.WriteByte((byte)ExecutorCommand.LoadIl);
                    _firmataStream.WriteByte(methodIndex);
                    ushort len = (ushort)byteCode.Length;
                    // Transmit 14 bit values
                    _firmataStream.WriteByte((byte)(len & 0x7f));
                    _firmataStream.WriteByte((byte)(len >> 7));
                    _firmataStream.WriteByte((byte)(codeIndex & 0x7f));
                    _firmataStream.WriteByte((byte)(codeIndex >> 7));
                    int bytesThisPacket = Math.Min(BYTES_PER_PACKET, byteCode.Length - codeIndex);
                    SendValuesAsTwo7bitBytes(byteCode.AsSpan(codeIndex, bytesThisPacket));
                    codeIndex += bytesThisPacket;

                    _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                    _firmataStream.Flush();

                    WaitAndHandleIlCommandReply(ExecutorCommand.LoadIl);
                }
            }
        }

        public void ExecuteIlCode(byte codeReference, object[] parameters)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _dataReceived.Reset();
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.SCHEDULER_DATA);
                _firmataStream.WriteByte((byte)0xFF); // IL data
                _firmataStream.WriteByte((byte)ExecutorCommand.StartTask);
                _firmataStream.WriteByte(codeReference);
                for (int i = 0; i < parameters.Length; i++)
                {
                    Type t = parameters[i].GetType();
                    if (t == typeof(Int32) || t == typeof(Int16) || t == typeof(sbyte) || t == typeof(bool))
                    {
                        byte[] param = BitConverter.GetBytes(Convert.ToInt32(parameters[i]));
                        SendValuesAsTwo7bitBytes(param);
                    }
                    else if (t == typeof(UInt32) || t == typeof(UInt16) || t == typeof(byte))
                    {
                        byte[] param = BitConverter.GetBytes(Convert.ToUInt32(parameters[i]));
                        SendValuesAsTwo7bitBytes(param);
                    }
                    else // Object case for now
                    {
                        byte[] param = BitConverter.GetBytes(Convert.ToUInt32(0));
                        SendValuesAsTwo7bitBytes(param);
                    }
                }

                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                _firmataStream.Flush();
                WaitAndHandleIlCommandReply(ExecutorCommand.StartTask);
            }
        }

        public void SendMethodDeclaration(byte codeReference, int declarationToken, MethodFlags methodFlags, byte maxLocals, byte argCount, byte[] localTypes, byte[] argTypes)
        {
            if (maxLocals != localTypes.Length || argCount != argTypes.Length)
            {
                throw new InvalidOperationException("Argument/Variable counts do not match");
            }

            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _dataReceived.Reset();
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.SCHEDULER_DATA);
                _firmataStream.WriteByte((byte)0xFF); // IL data
                _firmataStream.WriteByte((byte)ExecutorCommand.DeclareMethod);
                _firmataStream.WriteByte(codeReference);
                _firmataStream.WriteByte((byte)methodFlags);
                _firmataStream.WriteByte(maxLocals);
                _firmataStream.WriteByte(argCount);
                byte[] param = BitConverter.GetBytes(declarationToken);
                SendValuesAsTwo7bitBytes(param);

                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                _firmataStream.Flush();

                WaitAndHandleIlCommandReply(ExecutorCommand.DeclareMethod);

                // Types of locals first
                int startIndex = 0;
                int localsToSend = Math.Min(localTypes.Length, 16);
                while (localsToSend > 0)
                {
                    _dataReceived.Reset();
                    _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                    _firmataStream.WriteByte((byte)FirmataSysexCommand.SCHEDULER_DATA);
                    _firmataStream.WriteByte((byte)0xFF); // IL data
                    _firmataStream.WriteByte((byte)ExecutorCommand.MethodSignature);
                    _firmataStream.WriteByte(codeReference);
                    _firmataStream.WriteByte(1); // Locals
                    _firmataStream.WriteByte((byte)localsToSend);
                    for (int i = startIndex; i < startIndex + localsToSend; i++)
                    {
                        _firmataStream.WriteByte(localTypes[i]);
                    }

                    _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                    _firmataStream.Flush();

                    WaitAndHandleIlCommandReply(ExecutorCommand.DeclareMethod);
                    localsToSend -= 16;
                }

                // Types of arguments
                startIndex = 0;
                localsToSend = Math.Min(argTypes.Length, 16);
                while (localsToSend > 0)
                {
                    _dataReceived.Reset();
                    _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                    _firmataStream.WriteByte((byte)FirmataSysexCommand.SCHEDULER_DATA);
                    _firmataStream.WriteByte((byte)0xFF); // IL data
                    _firmataStream.WriteByte((byte)ExecutorCommand.MethodSignature);
                    _firmataStream.WriteByte(codeReference);
                    _firmataStream.WriteByte(0); // arguments
                    _firmataStream.WriteByte((byte)localsToSend);
                    for (int i = startIndex; i < startIndex + localsToSend; i++)
                    {
                        _firmataStream.WriteByte(argTypes[i]);
                    }

                    _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                    _firmataStream.Flush();

                    WaitAndHandleIlCommandReply(ExecutorCommand.DeclareMethod);
                    localsToSend -= 16;
                }
            }
        }

        public void SendTokenMap(byte codeReference, int[] data)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                // Send four (two pairs) at a time, otherwise the maximum length of the message may be exceeded
                for (int token = 0; token < data.Length;)
                {
                    _dataReceived.Reset();
                    _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                    _firmataStream.WriteByte((byte)FirmataSysexCommand.SCHEDULER_DATA);
                    _firmataStream.WriteByte((byte)0xFF); // IL data
                    _firmataStream.WriteByte((byte)ExecutorCommand.SetMethodTokens);
                    _firmataStream.WriteByte(codeReference);
                    int remaining = data.Length - token;
                    if (remaining > 4)
                    {
                        remaining = 4;
                    }

                    ushort len = (ushort)data.Length;
                    // Transmit 14 bit values
                    _firmataStream.WriteByte((byte)(len & 0x7f));
                    _firmataStream.WriteByte((byte)(len >> 7));
                    _firmataStream.WriteByte((byte)(token & 0x7f));
                    _firmataStream.WriteByte((byte)(token >> 7));

                    for (int i = token; i < token + remaining; i++)
                    {
                        byte[] param = BitConverter.GetBytes(data[i]);
                        SendValuesAsTwo7bitBytes(param);
                    }

                    _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                    _firmataStream.Flush();
                    WaitAndHandleIlCommandReply(ExecutorCommand.SetMethodTokens);
                    token = token + remaining;
                }
            }
        }

        public void SendClassDeclaration(Int32 classToken, Int32 parentToken, Int16 sizeOfClass, List<(VariableKind Kind, Int32 FieldToken)> members)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                short len = (short)members.Count;
                for (short member = 0; member < members.Count; member++)
                {
                    _dataReceived.Reset();
                    _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                    _firmataStream.WriteByte((byte)FirmataSysexCommand.SCHEDULER_DATA);
                    _firmataStream.WriteByte((byte)0xFF); // IL data
                    _firmataStream.WriteByte((byte)ExecutorCommand.ClassDeclaration);
                    Send(classToken);
                    Send(parentToken);
                    Send(sizeOfClass);
                    Send(len);
                    Send(member);

                    // TODO: Sending one at a time has a bit much overhead, but lets make this work first
                    _firmataStream.WriteByte((byte)members[member].Kind);
                    Send(members[member].FieldToken);
                    _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                    _firmataStream.Flush();
                    WaitAndHandleIlCommandReply(ExecutorCommand.SetMethodTokens);
                }
            }
        }

        public void SendIlResetCommand(bool force)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.SCHEDULER_DATA);
                _firmataStream.WriteByte((byte)0xFF); // IL data
                _firmataStream.WriteByte((byte)ExecutorCommand.ResetExecutor);
                _firmataStream.WriteByte((byte)(force ? 1 : 0));
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                _firmataStream.Flush();
                Thread.Sleep(100);
            }
        }

        public void SendKillTask(byte codeReference)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            lock (_synchronisationLock)
            {
                _firmataStream.WriteByte((byte)FirmataCommand.START_SYSEX);
                _firmataStream.WriteByte((byte)FirmataSysexCommand.SCHEDULER_DATA);
                _firmataStream.WriteByte((byte)0xFF); // IL data
                _firmataStream.WriteByte((byte)ExecutorCommand.KillTask);
                _firmataStream.WriteByte(codeReference);
                _firmataStream.WriteByte((byte)FirmataCommand.END_SYSEX);
                _firmataStream.Flush();
                Thread.Sleep(100);
            }
        }

        private void SendValuesAsTwo7bitBytes(ReadOnlySpan<byte> values)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            for (int i = 0; i < values.Length; i++)
            {
                _firmataStream.WriteByte((byte)(values[i] & (uint)sbyte.MaxValue));
                _firmataStream.WriteByte((byte)(values[i] >> 7 & sbyte.MaxValue));
            }
        }

        /// <summary>
        /// Send an integer as 5 bytes (rather than 8, as <see cref="SendValuesAsTwo7bitBytes"/> would do)
        /// </summary>
        /// <param name="value">An integer value to send</param>
        private void Send(Int32 value)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            byte[] data = new byte[5];
            data[0] = (byte)(value & 0x7F);
            data[1] = (byte)((value >> 7) & 0x7F);
            data[2] = (byte)((value >> 14) & 0x7F);
            data[3] = (byte)((value >> 21) & 0x7F);
            data[4] = (byte)((value >> 28) & 0x7F);
            _firmataStream.Write(data);
        }

        /// <summary>
        /// Send a short as 2 bytes.
        /// Note: Only sends 14 bit!
        /// </summary>
        /// <param name="value">An integer value to send</param>
        private void Send(Int16 value)
        {
            if (_firmataStream == null)
            {
                throw new ObjectDisposedException(nameof(FirmataDevice));
            }

            Span<byte> data = stackalloc byte[2];
            data[0] = (byte)(value & 0x7F);
            data[1] = (byte)((value >> 7) & 0x7F);
            _firmataStream.Write(data);
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
                Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

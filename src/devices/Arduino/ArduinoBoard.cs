// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device;
using System.Device.Analog;
using System.Text;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Iot.Device.Common;
using Iot.Device.Board;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Implements an interface to an arduino board which is running Firmata.
    /// See documentation on how to prepare your arduino board to work with this.
    /// Note that the program will run on the PC, so you cannot disconnect the
    /// Arduino while this driver is connected.
    /// </summary>
    public class ArduinoBoard : Board.Board, IDisposable
    {
        private readonly List<SupportedMode> _knownSupportedModes = new List<SupportedMode>();
        private readonly List<ExtendedCommandHandler> _extendedCommandHandlers = new List<ExtendedCommandHandler>();
        private readonly ReaderWriterLockSlim _commandHandlersLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private SerialPort? _serialPort;
        private Stream? _dataStream;
        private FirmataDevice? _firmata;
        private IReadOnlyList<SupportedPinConfiguration> _supportedPinConfigurations = new List<SupportedPinConfiguration>();
        private bool _initialized;
        private bool _isDisposed = false;

        // Counts how many spi devices are attached, to make sure we enable/disable the bus only when no devices are attached
        private int _spiEnabled = 0;
        private Version _firmwareVersion = new Version();
        private string _firmwareName = string.Empty;
        private Version _firmataVersion = new Version();

        private object _initializationLock = new object();

        private ILogger _logger;

        /// <summary>
        /// Creates an instance of an Ardino board connection using the given stream (typically from a serial port)
        /// </summary>
        /// <remarks>
        /// The device is initialized when the first command is sent. The constructor always succeeds.
        /// </remarks>
        /// <param name="serialPortStream">A stream to an Arduino/Firmata device</param>
        /// <param name="usesHardwareFlowControl">True to indicate that the stream supports hardware flow control (can be a serial port
        /// with RTS/CTS handshake or a network stream where the protocol already supports flow control)</param>
        public ArduinoBoard(Stream serialPortStream, bool usesHardwareFlowControl)
        {
            _dataStream = serialPortStream ?? throw new ArgumentNullException(nameof(serialPortStream));
            StreamUsesHardwareFlowControl = usesHardwareFlowControl;
            _logger = this.GetCurrentClassLogger();
        }

        /// <summary>
        /// Creates an instance of an Ardino board connection using the given stream (typically from a serial port)
        /// </summary>
        /// <remarks>
        /// The device is initialized when the first command is sent. The constructor always succeeds.
        /// </remarks>
        /// <remarks>
        /// The stream must have a blocking read operation, or the connection might fail. Some serial port drivers incorrectly
        /// return immediately when no data is available and the <code>ReadTimeout</code> is set to infinite (the default). In such a case, set the
        /// ReadTimeout to a large value (such as <code>Int.Max - 10</code>), which will simulate a blocking call.
        /// </remarks>
        /// <param name="serialPortStream">A stream to an Arduino/Firmata device</param>
        public ArduinoBoard(Stream serialPortStream)
        : this(serialPortStream, false)
        {
        }

        /// <summary>
        /// Creates an instance of the Arduino board connection connected to a serial port
        /// </summary>
        /// The device is initialized when the first command is sent. The constructor always succeeds.
        /// <param name="portName">Port name. On Windows, this is usually "COM3" or "COM4" for an Arduino attached via USB.
        /// On Linux, possible values include "/dev/ttyAMA0", "/dev/serial0", "/dev/ttyUSB1", etc.</param>
        /// <param name="baudRate">Baudrate to use. It is recommended to use at least 115200 Baud.</param>
        public ArduinoBoard(string portName, int baudRate)
        {
            _dataStream = null;
            _serialPort = new SerialPort(portName, baudRate);
            // Set the timeout to a long time, but not infinite. See the note for the constructor above.
            _serialPort.ReadTimeout = int.MaxValue - 10;
            StreamUsesHardwareFlowControl = false; // Would need to configure the serial port externally for this to work
            _logger = this.GetCurrentClassLogger();
        }

        /// <summary>
        /// The board logger.
        /// </summary>
        protected ILogger Logger => _logger;

        /// <summary>
        /// Set this to true if the underlying stream uses some kind of hardware or low-level flow control (RTS/CTS for
        /// a serial port, or a TCP socket). Setting this to true may improve performance on bulk transfers (such as
        /// large SPI blocks) but can result in buffer overflows if flow control is not working. Default: false
        /// </summary>
        public bool StreamUsesHardwareFlowControl
        {
            get;
        }

        /// <summary>
        /// The list of supported pin modes.
        /// This list can be extended by adding special modes using <see cref="AddCommandHandler{T}"/>.
        /// </summary>
        public IReadOnlyList<SupportedMode> KnownModes
        {
            get
            {
                _commandHandlersLock.EnterReadLock();
                try
                {
                    return _knownSupportedModes.AsReadOnly();
                }
                finally
                {
                    _commandHandlersLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Searches the given list of com ports for a firmata device.
        /// </summary>
        /// <remarks>
        /// Scanning ports and testing for devices may affect unrelated devices. It is advisable to exclude ports known to contain other hardware from this scan.
        /// A board won't be found if its port is already open (by the same or a different process).
        /// </remarks>
        /// <param name="comPorts">List of com ports. Can be used with <see cref="SerialPort.GetPortNames"/>.</param>
        /// <param name="baudRates">List of baud rates to test. <see cref="CommonBaudRates"/>.</param>
        /// <param name="board">[Out] Returns the board reference. It is already initialized.</param>
        /// <returns>True on success, false if no board was found</returns>
        public static bool TryFindBoard(IEnumerable<string> comPorts, IEnumerable<int> baudRates,
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out ArduinoBoard? board)
        {
            foreach (string port in comPorts)
            {
                foreach (int baud in baudRates)
                {
                    ArduinoBoard? b = null;
                    try
                    {
                        b = new ArduinoBoard(port, baud);
                        b.Initialize();
                        board = b;
                        return true;
                    }
                    catch (Exception x) when (x is NotSupportedException || x is TimeoutException || x is IOException || x is UnauthorizedAccessException)
                    {
                        b?.Dispose();
                    }
                }
            }

            board = null!;
            return false;
        }

        /// <summary>
        /// Tries to connect to an arduino over network.
        /// This requires an arduino with an ethernet shield or an ESP32 with enabled WIFI support.
        /// </summary>
        /// <param name="boardAddress">The IP address of the board</param>
        /// <param name="port">The network port to use. The default port is 27016</param>
        /// <param name="board">Returns the board if successful</param>
        /// <returns>True on success, false otherwise</returns>
        public static bool TryConnectToNetworkedBoard(IPAddress boardAddress, int port,
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out ArduinoBoard? board)
        {
            return TryConnectToNetworkedBoard(boardAddress, port, true, out board);
        }

        /// <summary>
        /// Tries to connect to an arduino over network.
        /// This requires an arduino with an ethernet shield or an ESP32 with enabled WIFI support.
        /// </summary>
        /// <param name="boardAddress">The IP address of the board</param>
        /// <param name="port">The network port to use. The default port is 27016</param>
        /// <param name="useAutoReconnect">True to use an auto-reconnecting stream. Helpful when using an unreliable connection.</param>
        /// <param name="board">Returns the board if successful</param>
        /// <returns>True on success, false otherwise</returns>
        public static bool TryConnectToNetworkedBoard(IPAddress boardAddress, int port, bool useAutoReconnect,
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out ArduinoBoard? board)
        {
            try
            {
                Stream networkStream;
                if (useAutoReconnect)
                {
                    networkStream = new ReconnectingNetworkStream(() =>
                    {
                        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(boardAddress, port);
                        socket.NoDelay = true;
                        Stream socketStream = new NetworkStream(socket, true);
                        return socketStream;
                    });
                }
                else
                {
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(boardAddress, port);
                    socket.NoDelay = true;
                    networkStream = new NetworkStream(socket, true);
                }

                board = new ArduinoBoard(networkStream, true);
                if (!(board.FirmataVersion > new Version(1, 0)))
                {
                    // Actually not expecting to get here (but the above will throw a SocketException if the remote end is not there)
                    throw new NotSupportedException("Very old firmware found on board");
                }

                return true;
            }
            catch (SocketException)
            {
                board = null;
                return false;
            }
        }

        /// <summary>
        /// Searches all available com ports for an Arduino device.
        /// </summary>
        /// <param name="board">A board, already open and initialized. Null if none was found.</param>
        /// <returns>True if a board was found, false otherwise</returns>
        /// <remarks>
        /// Scanning serial ports may affect unrelated devices. If there are problems, use the
        /// <see cref="TryFindBoard(System.Collections.Generic.IEnumerable{string},System.Collections.Generic.IEnumerable{int},out Iot.Device.Arduino.ArduinoBoard?)"/> overload excluding ports that shall not be tested.
        /// </remarks>
        public static bool TryFindBoard(
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out ArduinoBoard? board)
        {
            return TryFindBoard(SerialPort.GetPortNames(), CommonBaudRates(), out board);
        }

        /// <summary>
        /// Returns a list of commonly used baud rates.
        /// </summary>
        public static List<int> CommonBaudRates()
        {
            return new List<int>()
            {
                9600,
                19200,
                18400,
                57600,
                115200,
                230400,
                250000,
                500000,
                1000000,
                2000000,
            };
        }

        /// <summary>
        /// Adds a new command handler.
        /// A command handler can support extended commands.
        /// </summary>
        /// <typeparam name="T">An instance of <see cref="ExtendedCommandHandler"/>.</typeparam>
        /// <param name="newCommandHandler">The new handler</param>
        public void AddCommandHandler<T>(T newCommandHandler)
            where T : ExtendedCommandHandler
        {
            if (newCommandHandler == null)
            {
                throw new ArgumentNullException(nameof(newCommandHandler));
            }

            _commandHandlersLock.EnterWriteLock();
            try
            {
                if (newCommandHandler.HandlesMode != null)
                {
                    // If we already know the mode, replace its configuration with the new one (typically, this will only update the name)
                    SupportedMode? m = _knownSupportedModes.FirstOrDefault(x => x.Value == newCommandHandler.HandlesMode.Value);
                    if (m != null)
                    {
                        _knownSupportedModes.Remove(m);
                    }

                    _knownSupportedModes.Add(newCommandHandler.HandlesMode);

                    // Update internal list
                    Firmata.SupportedModes = _knownSupportedModes;
                }

                _extendedCommandHandlers.Add(newCommandHandler);
            }
            finally
            {
                _commandHandlersLock.ExitWriteLock();
            }

            if (_firmata != null)
            {
                // Only if already initialized
                newCommandHandler.Registered(_firmata, this);
            }
        }

        /// <summary>
        /// Unregisters the given command handler
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="ExtendedCommandHandler"/></typeparam>
        /// <param name="commandHandler">The instance</param>
        /// <remarks>This is intended mostly for unit test scenarios, where the command handlers are recreated. It does not
        /// remove the modes supported by the handler</remarks>
        public void RemoveCommandHandler<T>(T commandHandler)
            where T : ExtendedCommandHandler
        {
            _commandHandlersLock.EnterWriteLock();
            try
            {
                _extendedCommandHandlers.Remove(commandHandler);
            }
            finally
            {
                _commandHandlersLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the command handler with the provided type. An exact type match is performed.
        /// </summary>
        /// <typeparam name="T">The type to query</typeparam>
        /// <returns>The command handler, or null if none was found</returns>
        public T? GetCommandHandler<T>()
            where T : ExtendedCommandHandler
        {
            foreach (ExtendedCommandHandler cmd in _extendedCommandHandlers)
            {
                if (cmd.GetType() == typeof(T))
                {
                    return (T)cmd;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the current assignment of the given pin
        /// </summary>
        /// <param name="pinNumber">Pin number to query</param>
        /// <returns>A value of the <see cref="PinUsage"/> enumeration</returns>
        public override PinUsage DetermineCurrentPinUsage(int pinNumber)
        {
            byte mode = Firmata.GetPinMode(pinNumber);
            _commandHandlersLock.EnterReadLock();
            try
            {
                SupportedMode? m = _knownSupportedModes.FirstOrDefault(x => x.Value == mode);
                if (m == null)
                {
                    return PinUsage.Unknown;
                }

                return m.PinUsage;
            }
            finally
            {
                _commandHandlersLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Initialize the board connection. This must be called before any other methods of this class.
        /// </summary>
        /// <exception cref="NotSupportedException">The Firmata firmware on the connected board is too old.</exception>
        /// <exception cref="TimeoutException">There was no answer from the board</exception>
        protected override void Initialize()
        {
            base.Initialize();

            // Shortcut, so we do not need to take the lock
            if (_initialized)
            {
                return;
            }

            lock (_initializationLock)
            {
                if (_initialized)
                {
                    return;
                }

                if (_isDisposed)
                {
                    throw new ObjectDisposedException("Board is already disposed");
                }

                if (_firmata != null)
                {
                    throw new InvalidOperationException("Board already initialized");
                }

                RegisterKnownSupportedModes();
                // Add the extended command handlers that are defined in this library
                RegisterCommandHandlers();

                if (_serialPort != null)
                {
                    _serialPort.Open();
                    _dataStream = _serialPort.BaseStream;
                }
                else if (_dataStream == null)
                {
                    // Should never get here
                    throw new InvalidOperationException("Constructor argument error: Neither port nor stream specified");
                }

                _firmata = new FirmataDevice(_knownSupportedModes);
                _firmata.Open(_dataStream);
                _firmata.OnError += FirmataOnError;
                _firmataVersion = _firmata.QueryFirmataVersion();
                if (_firmataVersion < _firmata.QuerySupportedFirmataVersion())
                {
                    throw new NotSupportedException($"Firmata version on board is {_firmataVersion}. Expected {_firmata.QuerySupportedFirmataVersion()}. They must be equal.");
                }

                Logger.LogInformation($"Firmata version on board is {_firmataVersion}.");

                _firmwareVersion = _firmata.QueryFirmwareVersion(out string firmwareName);
                _firmwareName = firmwareName;

                Logger.LogInformation($"Firmware version on board is {_firmwareVersion}");

                _firmata.QueryCapabilities();

                _supportedPinConfigurations = _firmata.PinConfigurations.AsReadOnly();

                Logger.LogInformation("Device capabilities: ");
                foreach (SupportedPinConfiguration pin in _supportedPinConfigurations)
                {
                    Logger.LogInformation(pin.ToString());
                }

                _firmata.EnableDigitalReporting();

                string? result = _firmata.CheckSystemVariablesSupported();
                if (result != null)
                {
                    Logger.LogInformation($"System variable support not available in firmware. Error: {result}");
                }
                else
                {
                    Logger.LogInformation("System variable support detected");
                    GetSystemVariable(SystemVariable.MaxSysexSize, -1, out int bufferSize);
                    // Should be excluding the SYSEX byte itself and the terminator, but see https://github.com/firmata/ConfigurableFirmata/issues/136
                    Logger.LogInformation($"Maximum SYSEX message size: {bufferSize}");
                }

                foreach (ExtendedCommandHandler e in _extendedCommandHandlers)
                {
                    e.Registered(_firmata, this);
                    e.OnConnected();
                }

                _initialized = true;
            }
        }

        /// <summary>
        /// Queries the given system variable.
        /// </summary>
        /// <param name="variableId">The variable to query</param>
        /// <param name="value">Receives the value</param>
        /// <returns>True on success, false otherwise (value not supported, etc. Check the log output)</returns>
        /// <exception cref="IOException">There was an error sending the command</exception>
        public bool GetSystemVariable(SystemVariable variableId, out int value)
        {
            value = 0;
            return Firmata.GetOrSetSystemVariable(variableId, -1, true, ref value);
        }

        /// <summary>
        /// Queries the given system variable.
        /// </summary>
        /// <param name="variableId">The variable to query</param>
        /// <param name="pinNumber">The pin number to use (-1 if not applicable for the given parameter)</param>
        /// <param name="value">Receives the value</param>
        /// <returns>True on success, false otherwise (value not supported, etc. Check the log output)</returns>
        /// <exception cref="IOException">There was an error sending the command</exception>
        public bool GetSystemVariable(SystemVariable variableId, int pinNumber, out int value)
        {
            value = 0;
            return Firmata.GetOrSetSystemVariable(variableId, pinNumber, true, ref value);
        }

        /// <summary>
        /// Update the given system variable.
        /// </summary>
        /// <param name="variableId">The variable to update</param>
        /// <param name="value">The new value</param>
        /// <returns>True on success, false otherwise (check the log output)</returns>
        /// <exception cref="IOException">There was a communication error</exception>
        public bool SetSystemVariable(SystemVariable variableId, int value)
        {
            return Firmata.GetOrSetSystemVariable(variableId, -1, false, ref value);
        }

        /// <summary>
        /// Update the given system variable.
        /// </summary>
        /// <param name="variableId">The variable to update</param>
        /// <param name="pinNumber">The pin number to use, or -1 if not relevant</param>
        /// <param name="value">The new value</param>
        /// <returns>True on success, false otherwise (check the log output)</returns>
        /// <exception cref="IOException">There was a communication error</exception>
        public bool SetSystemVariable(SystemVariable variableId, int pinNumber, int value)
        {
            return Firmata.GetOrSetSystemVariable(variableId, pinNumber, false, ref value);
        }

        private void RegisterCommandHandlers()
        {
            _commandHandlersLock.EnterWriteLock();
            _extendedCommandHandlers.Add(new DhtSensor());
            _extendedCommandHandlers.Add(new FrequencySensor());
            _commandHandlersLock.ExitWriteLock();
        }

        /// <summary>
        /// Registers the known supported modes. Should only be called once from Initialize.
        /// </summary>
        private void RegisterKnownSupportedModes()
        {
            _commandHandlersLock.EnterWriteLock();
            // We add all known modes to the list, even though we don't really support them all in the core
            _knownSupportedModes.Add(SupportedMode.DigitalInput);
            _knownSupportedModes.Add(SupportedMode.DigitalOutput);
            _knownSupportedModes.Add(SupportedMode.AnalogInput);
            _knownSupportedModes.Add(SupportedMode.Pwm);
            _knownSupportedModes.Add(SupportedMode.Servo);
            _knownSupportedModes.Add(SupportedMode.Shift);
            _knownSupportedModes.Add(SupportedMode.I2c);
            _knownSupportedModes.Add(SupportedMode.OneWire);
            _knownSupportedModes.Add(SupportedMode.Stepper);
            _knownSupportedModes.Add(SupportedMode.Encoder);
            _knownSupportedModes.Add(SupportedMode.Serial);
            _knownSupportedModes.Add(SupportedMode.InputPullup);
            _knownSupportedModes.Add(SupportedMode.Spi);
            _knownSupportedModes.Add(SupportedMode.Sonar);
            _knownSupportedModes.Add(SupportedMode.Tone);
            _knownSupportedModes.Add(SupportedMode.Dht);
            _knownSupportedModes.Add(SupportedMode.Frequency);
            _commandHandlersLock.ExitWriteLock();
        }

        /// <summary>
        /// Firmware version on the device
        /// </summary>
        public Version FirmwareVersion
        {
            get
            {
                Initialize();

                return _firmwareVersion;
            }
        }

        /// <summary>
        /// Name of the firmware.
        /// </summary>
        public string FirmwareName
        {
            get
            {
                Initialize();

                return _firmwareName;
            }
        }

        /// <summary>
        /// Firmata version found on the board.
        /// </summary>
        public Version FirmataVersion
        {
            get
            {
                Initialize();

                return _firmataVersion;
            }
        }

        internal FirmataDevice Firmata
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(ArduinoBoard));
                }

                // This exception should not normally happen
                return _firmata ?? throw new InvalidOperationException("Device not initialized");
            }
        }

        /// <summary>
        /// Returns the list of capabilities per pin
        /// </summary>
        public IReadOnlyList<SupportedPinConfiguration> SupportedPinConfigurations
        {
            get
            {
                Initialize();

                return _supportedPinConfigurations;
            }
        }

        private void FirmataOnError(string message, Exception? exception)
        {
            if (exception != null)
            {
                Logger.LogError(exception, message);
            }
            else
            {
                // If the message contains a line feed, strip that
                Logger.LogInformation(message.TrimEnd(new char[] { '\r', '\n' }));
            }

            _commandHandlersLock.EnterReadLock();
            try
            {
                foreach (ExtendedCommandHandler handler in _extendedCommandHandlers)
                {
                    handler.OnErrorMessage(message, exception);
                }
            }
            finally
            {
                _commandHandlersLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Sets the internal pin mode to the given value, if supported.
        /// </summary>
        /// <param name="pin">The pin to configure</param>
        /// <param name="arduinoMode">The mode to set</param>
        /// <exception cref="TimeoutException">The mode was not updated, either because the command was not understood or
        /// the mode is unknown by the firmware</exception>
        /// <remarks>This method is intended for use by <see cref="ExtendedCommandHandler"/> instances. Users should not
        /// call this method directly. It is the responsibility of the command handler to use the capabilities table to check
        /// that the mode is actually supported</remarks>
        public void SetPinMode(int pin, SupportedMode arduinoMode)
        {
            Firmata.SetPinMode(pin, arduinoMode);
        }

        /// <summary>
        /// Returns the current assignment of the given pin
        /// </summary>
        /// <param name="pinNumber">Pin number to query</param>
        /// <returns>An instance of <see cref="SupportedMode"/> from the list of known modes (or a new instance for an unknown mode)</returns>
        /// <remarks>Thi is the opposite of <see cref="SetPinMode"/>. See there for usage limitations.</remarks>
        public SupportedMode GetPinMode(int pinNumber)
        {
            byte mode = Firmata.GetPinMode(pinNumber);
            _commandHandlersLock.EnterReadLock();
            try
            {
                SupportedMode? m = _knownSupportedModes.FirstOrDefault(x => x.Value == mode);
                if (m == null)
                {
                    return new SupportedMode(mode, $"Unknown mode {mode}");
                }

                return m;
            }
            finally
            {
                _commandHandlersLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Creates a GPIO Controller instance for the board. This allows working with digital input/output pins.
        /// </summary>
        /// <returns>An instance of GpioController, using an Arduino-Enabled driver</returns>
        public override GpioController CreateGpioController()
        {
            Initialize();

            if (_firmata == null)
            {
                throw new ObjectDisposedException(nameof(_firmata));
            }

            return new GpioController(new ArduinoGpioControllerDriver(_firmata, _supportedPinConfigurations));
        }

        /// <inheritdoc />
        protected override I2cBusManager CreateI2cBusCore(int busNumber, int[]? pins)
        {
            Initialize();
            if (!SupportedPinConfigurations.Any(x => x.PinModes.Contains(SupportedMode.I2c)))
            {
                throw new NotSupportedException("No Pins with I2c support found. Is the I2c module loaded?");
            }

            return new I2cBusManager(this, busNumber, pins, new ArduinoI2cBus(this, busNumber));
        }

        /// <inheritdoc />
        public override int GetDefaultI2cBusNumber()
        {
            return 0;
        }

        /// <summary>
        /// Connect to a device connected to the primary SPI bus on the Arduino
        /// Firmata's default implementation has no SPI support, so this first checks whether it's available at all.
        /// </summary>
        /// <param name="settings">Spi Connection settings</param>
        /// <param name="pins">The pins to use.</param>
        /// <returns>An <see cref="SpiDevice"/> instance.</returns>
        /// <exception cref="NotSupportedException">The Bus number is not 0, or the SPI component has not been enabled in the firmware.</exception>
        protected override SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings settings, int[] pins)
        {
            Initialize();

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.BusId != 0)
            {
                throw new NotSupportedException("Only Bus Id 0 is supported");
            }

            if (!SupportedPinConfigurations.Any(x => x.PinModes.Contains(SupportedMode.Spi)))
            {
                throw new NotSupportedException("No Pins with SPI support found. Is the SPI module loaded?");
            }

            return new ArduinoSpiDevice(this, settings);
        }

        /// <summary>
        /// Creates a PWM channel.
        /// </summary>
        /// <param name="chip">Must be 0.</param>
        /// <param name="channel">Pin number to use</param>
        /// <param name="frequency">This value is ignored</param>
        /// <param name="dutyCyclePercentage">The duty cycle as a fraction.</param>
        /// <returns></returns>
        protected override PwmChannel CreateSimplePwmChannel(
            int chip,
            int channel,
            int frequency,
            double dutyCyclePercentage)
        {
            Initialize();

            return new ArduinoPwmChannel(this, chip, channel, frequency, dutyCyclePercentage);
        }

        /// <inheritdoc />
        public override int GetDefaultPinAssignmentForPwm(int chip, int channel)
        {
            if (chip == 0)
            {
                return channel;
            }

            throw new NotSupportedException($"Unknown chip numbe {chip}");
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForI2c(int busId)
        {
            if (busId != 0)
            {
                throw new NotSupportedException("Only bus number 0 is currently supported");
            }

            IEnumerable<int> pins = _supportedPinConfigurations.Where(x => x.PinModes.Contains(SupportedMode.I2c)).Select(y => y.Pin);

            return pins.ToArray();
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
        {
            if (connectionSettings.BusId != 0)
            {
                throw new NotSupportedException("Only bus number 0 is currently supported");
            }

            IEnumerable<int> pins = _supportedPinConfigurations.Where(x => x.PinModes.Contains(SupportedMode.Spi)).Select(y => y.Pin);

            return pins.ToArray();
        }

        /// <summary>
        /// Creates an anlog controller for this board.
        /// </summary>
        /// <param name="chip">Must be 0</param>
        /// <returns>An <see cref="AnalogController"/> instance</returns>
        public virtual AnalogController CreateAnalogController(int chip)
        {
            Initialize();

            return new ArduinoAnalogController(this, SupportedPinConfigurations);
        }

        /// <summary>
        /// Configures the sampling interval for analog input pins (when an event callback is enabled)
        /// </summary>
        /// <param name="timeSpan">Timespan between updates. Default ~20ms</param>
        public void SetAnalogPinSamplingInterval(TimeSpan timeSpan)
        {
            Initialize();

            Firmata.SetAnalogInputSamplingInterval(timeSpan);
        }

        /// <summary>
        /// Performs a software reset of the Arduino firmware
        /// </summary>
        public void SoftwareReset()
        {
            Initialize();
            Firmata.SendSoftwareReset();
            Firmata.QueryCapabilities();
        }

        /// <summary>
        /// Standard dispose pattern
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            foreach (ExtendedCommandHandler e in _extendedCommandHandlers)
            {
                try
                {
                    e.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore
                }
            }

            if (_firmata != null)
            {
                // Can end the next possible moment (otherwise might just throw a bunch of warnings before actually terminating anyway)
                _firmata.InputThreadShouldExit = true;
            }

            _isDisposed = true;
            // Do this first, to force any blocking read operations to end
            if (_dataStream != null)
            {
                _dataStream.Dispose();
                _dataStream = null!;
            }

            if (_serialPort != null)
            {
                _serialPort.Dispose();
                _serialPort = null;
            }

            if (_firmata != null)
            {
                _firmata.OnError -= FirmataOnError;
                _firmata.Dispose();
                _firmata = null;
            }

            _initialized = false;
        }

        internal void EnableSpi()
        {
            _spiEnabled++;
            if (_spiEnabled == 1)
            {
                Firmata.EnableSpi();
            }
        }

        internal void DisableSpi()
        {
            if (_spiEnabled <= 0)
            {
                throw new InvalidOperationException("Internal reference counting error: Spi ports already closed");
            }

            _spiEnabled--;
            if (_spiEnabled == 0)
            {
                Firmata.DisableSpi();
            }
        }

        /// <inheritdoc />
        public override ComponentInformation QueryComponentInformation()
        {
            var self = base.QueryComponentInformation();
            self.Properties["FirmwareVersion"] = FirmwareVersion.ToString();
            self.Properties["FirmwareName"] = FirmwareName;
            self.Properties["FirmataVersion"] = FirmataVersion.ToString();
            return self;
        }

        /// <summary>
        /// Pings the device, to get an estimate about the round-trip time.
        /// With some Wifi setups, the round trip time may be significantly higher than desired.
        /// </summary>
        /// <param name="number">The number of pings to send</param>
        /// <returns>The list of reply times. Contains a negative value for lost packets</returns>
        public List<TimeSpan> Ping(int number)
        {
            Initialize();
            if (_firmata == null)
            {
                throw new ObjectDisposedException("Not connected");
            }

            List<TimeSpan> ret = new();
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < number; i++)
            {
                sw.Restart();
                try
                {
                    _firmata.QueryFirmwareVersion(out _);
                    TimeSpan elapsed = sw.Elapsed;
                    ret.Add(elapsed);
                    _logger.LogInformation($"Round trip time: {elapsed.TotalMilliseconds}ms");
                }
                catch (TimeoutException x)
                {
                    _logger.LogError(x, $"Timeout: {x.Message}");
                    ret.Add(TimeSpan.FromMinutes(-1));
                }

                sw.Stop();
            }

            return ret;
        }
    }
}

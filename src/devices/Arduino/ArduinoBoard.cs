// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public ArduinoBoard(Stream serialPortStream)
        {
            _dataStream = serialPortStream ?? throw new ArgumentNullException(nameof(serialPortStream));
            _logger = this.GetCurrentClassLogger();
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
            _logger = this.GetCurrentClassLogger();
        }

        /// <summary>
        /// The board logger.
        /// </summary>
        protected ILogger Logger => _logger;

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
            foreach (var port in comPorts)
            {
                foreach (var baud in baudRates)
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
        /// <param name="port">The network port to use</param>
        /// <param name="board">Returns the board if successful</param>
        /// <returns>True on success, false otherwise</returns>
        public static bool TryConnectToNetworkedBoard(IPAddress boardAddress, int port,
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out ArduinoBoard? board)
        {
            board = null;
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(boardAddress, port);
                socket.NoDelay = true;
                var networkStream = new NetworkStream(socket, true);
                board = new ArduinoBoard(networkStream);
                if (!(board.FirmataVersion > new Version(1, 0)))
                {
                    // Actually not expecting to get here (but the above will throw a SocketException if the remote end is not there)
                    throw new NotSupportedException("Very old firmware found on board");
                }

                return true;
            }
            catch (SocketException)
            {
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
                    var m = _knownSupportedModes.FirstOrDefault(x => x.Value == newCommandHandler.HandlesMode.Value);
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
        /// Gets the command handler with the provided type. An exact type match is performed.
        /// </summary>
        /// <typeparam name="T">The type to query</typeparam>
        /// <returns>The command handler, or null if none was found</returns>
        public T? GetCommandHandler<T>()
            where T : ExtendedCommandHandler
        {
            foreach (var cmd in _extendedCommandHandlers)
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
                var m = _knownSupportedModes.FirstOrDefault(x => x.Value == mode);
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

                _firmwareVersion = _firmata.QueryFirmwareVersion(out var firmwareName);
                _firmwareName = firmwareName;

                Logger.LogInformation($"Firmware version on board is {_firmwareVersion}");

                _firmata.QueryCapabilities();

                _supportedPinConfigurations = _firmata.PinConfigurations.AsReadOnly();

                Logger.LogInformation("Device capabilities: ");
                foreach (var pin in _supportedPinConfigurations)
                {
                    Logger.LogInformation(pin.ToString());
                }

                _firmata.EnableDigitalReporting();

                foreach (var e in _extendedCommandHandlers)
                {
                    e.Registered(_firmata, this);
                    e.OnConnected();
                }

                _initialized = true;
            }
        }

        private void RegisterCommandHandlers()
        {
            lock (_commandHandlersLock)
            {
                _extendedCommandHandlers.Add(new DhtSensor());
                _extendedCommandHandlers.Add(new FrequencySensor());
            }
        }

        /// <summary>
        /// Registers the known supported modes. Should only be called once from Initialize.
        /// </summary>
        private void RegisterKnownSupportedModes()
        {
            lock (_commandHandlersLock)
            {
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
            }
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
                Logger.LogInformation(message);
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
                var m = _knownSupportedModes.FirstOrDefault(x => x.Value == mode);
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

            return new GpioController(PinNumberingScheme.Logical, new ArduinoGpioControllerDriver(this, _supportedPinConfigurations));
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

            var pins = _supportedPinConfigurations.Where(x => x.PinModes.Contains(SupportedMode.I2c)).Select(y => y.Pin);

            return pins.ToArray();
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
        {
            if (connectionSettings.BusId != 0)
            {
                throw new NotSupportedException("Only bus number 0 is currently supported");
            }

            var pins = _supportedPinConfigurations.Where(x => x.PinModes.Contains(SupportedMode.Spi)).Select(y => y.Pin);

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

            return new ArduinoAnalogController(this, SupportedPinConfigurations, PinNumberingScheme.Logical);
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
        /// Standard dispose pattern
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            foreach (var e in _extendedCommandHandlers)
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
    }
}

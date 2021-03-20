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
using Iot.Device.Common;
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
    public class ArduinoBoard : IDisposable
    {
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
#if !NETCOREAPP2_1
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
        /// Searches all available com ports for an Arduino device.
        /// </summary>
        /// <param name="board">A board, already open and initialized. Null if none was found.</param>
        /// <returns>True if a board was found, false otherwise</returns>
        /// <remarks>
        /// Scanning serial ports may affect unrelated devices. If there are problems, use the
        /// <see cref="TryFindBoard(System.Collections.Generic.IEnumerable{string},System.Collections.Generic.IEnumerable{int},out Iot.Device.Arduino.ArduinoBoard?)"/> overload excluding ports that shall not be tested.
        /// </remarks>
        public static bool TryFindBoard(
#if !NETCOREAPP2_1
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
        /// Initialize the board connection. This must be called before any other methods of this class.
        /// </summary>
        /// <exception cref="NotSupportedException">The Firmata firmware on the connected board is too old.</exception>
        /// <exception cref="TimeoutException">There was no answer from the board</exception>
        protected virtual void Initialize()
        {
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

                _firmata = new FirmataDevice();
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

                _initialized = true;
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
        /// Creates a GPIO Controller instance for the board. This allows working with digital input/output pins.
        /// </summary>
        /// <returns>An instance of GpioController, using an Arduino-Enabled driver</returns>
        public GpioController CreateGpioController()
        {
            Initialize();

            return new GpioController(PinNumberingScheme.Logical, new ArduinoGpioControllerDriver(this, _supportedPinConfigurations));
        }

        /// <summary>
        /// Creates an I2c device with the given connection settings.
        /// </summary>
        /// <param name="connectionSettings">I2c connection settings. Only Bus 0 is supported.</param>
        /// <returns>An <see cref="I2cDevice"/> instance</returns>
        /// <exception cref="NotSupportedException">The firmware reports that no pins are available for I2C. Check whether the I2C module is enabled in Firmata.
        /// Or: An invalid Bus Id or device Id was specified</exception>
        public virtual I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings)
        {
            Initialize();

            if (connectionSettings == null)
            {
                throw new ArgumentNullException(nameof(connectionSettings));
            }

            if (!SupportedPinConfigurations.Any(x => x.PinModes.Contains(SupportedMode.I2c)))
            {
                throw new NotSupportedException("No Pins with I2c support found. Is the I2c module loaded?");
            }

            return new ArduinoI2cDevice(this, connectionSettings);
        }

        /// <summary>
        /// Connect to a device connected to the primary SPI bus on the Arduino
        /// Firmata's default implementation has no SPI support, so this first checks whether it's available at all.
        /// </summary>
        /// <param name="settings">Spi Connection settings</param>
        /// <returns>An <see cref="SpiDevice"/> instance.</returns>
        /// <exception cref="NotSupportedException">The Bus number is not 0, or the SPI component has not been enabled in the firmware.</exception>
        public virtual SpiDevice CreateSpiDevice(SpiConnectionSettings settings)
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
        public virtual PwmChannel CreatePwmChannel(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCyclePercentage = 0.5)
        {
            Initialize();

            return new ArduinoPwmChannel(this, chip, channel, frequency, dutyCyclePercentage);
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
        /// Special function to read DHT sensor, if supported
        /// </summary>
        /// <param name="pinNumber">Pin Number</param>
        /// <param name="dhtType">Type of DHT Sensor: 11 = DHT11, 22 = DHT22, etc.</param>
        /// <param name="temperature">Temperature</param>
        /// <param name="humidity">Relative humidity</param>
        /// <returns>True on success, false otherwise</returns>
        public bool TryReadDht(int pinNumber, int dhtType, out Temperature temperature, out RelativeHumidity humidity)
        {
            Initialize();

            if (!_supportedPinConfigurations[pinNumber].PinModes.Contains(SupportedMode.Dht))
            {
                temperature = default;
                humidity = default;
                return false;
            }

            return Firmata.TryReadDht(pinNumber, dhtType, out temperature, out humidity);
        }

        /// <summary>
        /// Standard dispose pattern
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
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
                _firmata.Dispose();
                _firmata = null;
            }

            _initialized = false;
        }

        /// <summary>
        /// Dispose this instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

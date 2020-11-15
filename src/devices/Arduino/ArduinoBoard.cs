// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Analog;
using System.Text;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Linq;
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
        private Stream _dataStream;
        private FirmataDevice? _firmata;
        private Version? _firmwareVersion;
        private Version? _protocolVersion;
        private string _firmwareName;
        private List<SupportedPinConfiguration> _supportedPinConfigurations;

        // Only a delegate, not an event, because one board can only have one compiler attached at a time
        private Action<int, MethodState, object[]>? _compilerCallback;

        // Counts how many spi devices are attached, to make sure we enable/disable the bus only when no devices are attached
        private int _spiEnabled;

        /// <summary>
        /// Creates an instance of an Ardino board connection using the given stream (typically from a serial port)
        /// </summary>
        /// <param name="serialPortStream">A stream to an Arduino/Firmata device</param>
        public ArduinoBoard(Stream serialPortStream)
        {
            _dataStream = serialPortStream;
            _spiEnabled = 0;
            _supportedPinConfigurations = new List<SupportedPinConfiguration>();
            _firmwareName = string.Empty;
        }

        /// <summary>
        /// Creates an instance of the Arduino board connection connected to a serial port
        /// </summary>
        /// <param name="portName">Port name. On Windows, this is usually "COM3" or "COM4" for an Arduino attached via USB.
        /// On Linux, possible values include "/dev/ttyAMA0", "/dev/serial0", "/dev/ttyUSB1", etc.</param>
        /// <param name="baudRate">Baudrate to use. It is recommended to use at least 115200 Baud.</param>
        public ArduinoBoard(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.Open();
            _dataStream = _serialPort.BaseStream;
            _supportedPinConfigurations = new List<SupportedPinConfiguration>();
            _firmwareName = string.Empty;
        }

        /// <summary>
        /// Default pin numbering scheme of this board. Always <see cref="PinNumberingScheme.Logical"/>, because the Arduino has only one pin numbering scheme.
        /// </summary>
        public PinNumberingScheme DefaultPinNumberingScheme
        {
            get
            {
                // We have only one scheme
                return PinNumberingScheme.Logical;
            }
        }

        /// <summary>
        /// Attach to this event to retrieve log messages
        /// </summary>
        public event Action<string, Exception?>? LogMessages;

        /// <summary>
        /// Searches the given list of com ports for a firmata device.
        /// </summary>
        /// <param name="comPorts">List of com ports. See <see cref="GetSerialPortNames"/>.</param>
        /// <param name="baudRates">List of baud rates to test. <see cref="CommonBaudRates"/>.</param>
        /// <returns>A board, already open and initialized. Null if none was found.</returns>
        public static ArduinoBoard? FindBoard(List<string> comPorts, List<int> baudRates)
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
                        return b;
                    }
                    catch (Exception x) when (x is NotSupportedException || x is TimeoutException || x is IOException)
                    {
                        b?.Dispose();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Searches all available com ports for an Arduino device
        /// </summary>
        /// <returns>A board, already open and initialized. Null if none was found.</returns>
        public static ArduinoBoard? FindBoard()
        {
            return FindBoard(GetSerialPortNames(), CommonBaudRates());
        }

        /// <summary>
        /// Returns the list of available serial ports
        /// </summary>
        /// <returns>A list of available serial ports</returns>
        /// <exception cref="Win32Exception">There was an error retrieving the list</exception>
        public static List<string> GetSerialPortNames()
        {
            return SerialPort.GetPortNames().ToList();
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
        public virtual void Initialize()
        {
            if (_firmata != null)
            {
                throw new InvalidOperationException("Board already initialized");
            }

            _firmata = new FirmataDevice();
            _firmata.Open(_dataStream);
            _firmata.OnError += FirmataOnError;
            _protocolVersion = _firmata.QueryFirmataVersion();
            if (_protocolVersion < _firmata.QuerySupportedFirmataVersion())
            {
                throw new NotSupportedException($"Firmata version on board is {_protocolVersion}. Expected {_firmata.QuerySupportedFirmataVersion()}. They must be equal.");
            }

            Log($"Firmata version on board is {_protocolVersion}.");

            _firmwareVersion = _firmata.QueryFirmwareVersion(out _firmwareName);

            Log($"Firmware version on board is {_firmwareVersion}");

            _firmata.QueryCapabilities();

            _supportedPinConfigurations = _firmata.PinConfigurations; // Clone reference

            Log("Device capabilities: ");
            foreach (var pin in _supportedPinConfigurations)
            {
                Log(pin.ToString());
            }

            _firmata.EnableDigitalReporting();

            _firmata.OnSchedulerReply += FirmataOnSchedulerReply;
        }

        /// <summary>
        /// Firmware version on the device
        /// </summary>
        public Version FirmwareVersion
        {
            get
            {
                return _firmwareVersion ?? new Version();
            }
        }

        /// <summary>
        /// Name of the firmware.
        /// </summary>
        public string FirmwareName
        {
            get
            {
                return _firmwareName ?? string.Empty;
            }
        }

        internal FirmataDevice Firmata
        {
            get
            {
                return _firmata ?? throw new ObjectDisposedException(nameof(ArduinoBoard));
            }
        }

        /// <summary>
        /// Returns the list of capabilities per pin
        /// </summary>
        public List<SupportedPinConfiguration> SupportedPinConfigurations
        {
            get
            {
                return _supportedPinConfigurations;
            }
        }

        internal void Log(string message)
        {
            LogMessages?.Invoke(message, null);
        }

        private void FirmataOnError(string message, Exception? innerException)
        {
            LogMessages?.Invoke(message, innerException);
        }

        private void FirmataOnSchedulerReply(byte method, MethodState schedulerMethodState, int numArgs, IList<byte> bytesOfArgs)
        {
            object[] data = new object[numArgs];

            for (int i = 0; i < numArgs * 4; i += 4)
            {
                int retVal = bytesOfArgs[i] | bytesOfArgs[i + 1] << 8 | bytesOfArgs[i + 2] << 16 | bytesOfArgs[i + 3] << 24;
                data[i / 4] = retVal;
            }

            _compilerCallback?.Invoke(method, schedulerMethodState, data);
        }

        /// <summary>
        /// Creates a GPIO Controller instance for the board. This allows working with digital input/output pins.
        /// </summary>
        /// <returns>An instance of GpioController, using an Arduino-Enabled driver</returns>
        public GpioController CreateGpioController()
        {
            return CreateGpioController(DefaultPinNumberingScheme);
        }

        /// <summary>
        /// Creates a GPIO Controller instance for the board. This allows working with digital input/output pins.
        /// </summary>
        /// <param name="pinNumberingScheme">Pin numbering scheme to use for this controller</param>
        /// <returns>An instance of GpioController, using an Arduino-Enabled driver</returns>
        public GpioController CreateGpioController(PinNumberingScheme pinNumberingScheme)
        {
            return new GpioController(pinNumberingScheme, new ArduinoGpioControllerDriver(this, _supportedPinConfigurations));
        }

        /// <summary>
        /// Creates an I2c device with the given connection settings.
        /// </summary>
        /// <param name="connectionSettings">I2c connection settings. Only Bus 0 is supported.</param>
        /// <returns>An <see cref="I2cDevice"/> instance</returns>
        /// <exception cref="NotSupportedException">The firmware reports that no pins are available for I2C. Check whether the I2C module is enabled in Firmata.
        /// Or: An invalid Bus Id or device Id was specified</exception>
        public I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings)
        {
            if (!SupportedPinConfigurations.Any(x => x.PinModes.Contains(SupportedMode.I2C)))
            {
                throw new NotSupportedException("No Pins with I2c support found. Is the I2c module loaded?");
            }

            return new ArduinoI2cDevice(this, connectionSettings);
        }

        /// <summary>
        /// Firmata has no support for SPI, even though the Arduino basically has an SPI interface.
        /// This therefore returns a Software SPI device for the default Arduino SPI port on pins 11, 12 and 13.
        /// </summary>
        /// <param name="settings">Spi Connection settings</param>
        /// <returns>An <see cref="SpiDevice"/> instance.</returns>
        public SpiDevice CreateSpiDevice(SpiConnectionSettings settings)
        {
            if (settings.BusId != 0)
            {
                throw new NotSupportedException("Only Bus Id 0 is supported");
            }

            if (!SupportedPinConfigurations.Any(x => x.PinModes.Contains(SupportedMode.SPI)))
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
        public PwmChannel CreatePwmChannel(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCyclePercentage = 0.5)
        {
            return new ArduinoPwmChannel(this, chip, channel, frequency, dutyCyclePercentage);
        }

        /// <summary>
        /// Creates an anlog controller for this board.
        /// </summary>
        /// <param name="chip">Must be 0</param>
        /// <returns>An <see cref="AnalogController"/> instance</returns>
        public AnalogController CreateAnalogController(int chip)
        {
            return new ArduinoAnalogController(this, SupportedPinConfigurations, PinNumberingScheme.Logical);
        }

        /// <summary>
        /// Special function to read DHT sensor, if supported
        /// </summary>
        /// <param name="pinNumber">Pin Number</param>
        /// <param name="dhtType">Type of DHT Sensor: 11 = DHT11, 22 = DHT22, etc.</param>
        /// <param name="temperature">Temperature</param>
        /// <param name="humidity">Relative humidity</param>
        /// <returns>True on success, false otherwise</returns>
        public bool TryReadDht(int pinNumber, int dhtType, out Temperature temperature, out Ratio humidity)
        {
            if (!_supportedPinConfigurations[pinNumber].PinModes.Contains(SupportedMode.DHT))
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
            // Do this first, to force any blocking read operations to end
            if (_dataStream != null)
            {
                _dataStream.Close();
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
                _firmata.Close();
                _firmata.Dispose();
            }

            _firmata = null;
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
                _firmata?.EnableSpi();
            }
        }

        internal void DisableSpi()
        {
            _spiEnabled--;
            if (_spiEnabled == 0)
            {
                _firmata?.DisableSpi();
            }
        }

        internal void SetCompilerCallback(Action<int, MethodState, object[]> onCompilerCallback)
        {
            if (onCompilerCallback == null)
            {
                _compilerCallback = null;
                return;
            }

            if (_compilerCallback != null)
            {
                throw new InvalidOperationException("Only one compiler can be active for a single board");
            }

            _compilerCallback = onCompilerCallback;
        }
    }
}

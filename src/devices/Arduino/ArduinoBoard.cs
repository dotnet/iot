using System;
using System.Collections.Generic;
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

#pragma warning disable CS1591

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
        private SerialPort _serialPort;
        private Stream _serialPortStream;
        private FirmataDevice _firmata;
        private Version _firmwareVersion;
        private Version _protocolVersion;
        private string _firmwareName;
        private List<SupportedPinConfiguration> _supportedPinConfigurations;

        // Counts how many spi devices are attached, to make sure we enable/disable the bus only when no devices are attached
        private int _spiEnabled;

        /// <summary>
        /// Creates an instance of an Ardino board connection using the given stream (typically from a serial port)
        /// </summary>
        /// <param name="serialPortStream">A stream to an Arduino/Firmata device</param>
        public ArduinoBoard(Stream serialPortStream)
        {
            _serialPortStream = serialPortStream;
            _spiEnabled = 0;
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
            _serialPortStream = _serialPort.BaseStream;
        }

        public PinNumberingScheme DefaultPinNumberingScheme
        {
            get
            {
                // We have only one scheme
                return PinNumberingScheme.Logical;
            }
        }

        public event Action<string, Exception> LogMessages;

        public virtual void Initialize()
        {
            _firmata = new FirmataDevice();
            _firmata.Open(_serialPortStream);
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
        }

        public Version FirmwareVersion
        {
            get
            {
                return _firmwareVersion;
            }
        }

        public string FirmwareName
        {
            get
            {
                return _firmwareName;
            }
        }

        internal FirmataDevice Firmata
        {
            get
            {
                return _firmata;
            }
        }

        internal List<SupportedPinConfiguration> SupportedPinConfigurations
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

        private void FirmataOnError(string message, Exception innerException)
        {
            LogMessages?.Invoke(message, innerException);
        }

        public GpioController CreateGpioController()
        {
            return CreateGpioController(DefaultPinNumberingScheme);
        }

        public GpioController CreateGpioController(PinNumberingScheme pinNumberingScheme)
        {
            return new GpioController(pinNumberingScheme, new ArduinoGpioControllerDriver(this, _supportedPinConfigurations));
        }

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
        /// <returns></returns>
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

        public PwmChannel CreatePwmChannel(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCyclePercentage = 0.5)
        {
            return new ArduinoPwmChannel(this, chip, channel, frequency, dutyCyclePercentage);
        }

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

        protected virtual void Dispose(bool disposing)
        {
            // Do this first, to force any blocking read operations to end
            if (_serialPortStream != null)
            {
                _serialPortStream.Close();
                _serialPortStream.Dispose();
                _serialPortStream = null;
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
                _firmata.EnableSpi();
            }
        }

        internal void DisableSpi()
        {
            _spiEnabled--;
            if (_spiEnabled == 0)
            {
                _firmata.DisableSpi();
            }
        }
    }
}

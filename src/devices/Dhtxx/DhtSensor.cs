// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using Iot.Units;

namespace Iot.Device.DHTxx
{
    /// <summary>
    /// Temperature and Humidity Sensor DHTxx
    /// </summary>
    public class DhtSensor : IDisposable
    {
        /// <summary>
        /// DHT12 Default I2C Address
        /// </summary>
        public const byte Dht12DefaultI2cAddress = 0x5C;

        private byte[] _readBuff = new byte[5];

        private readonly CommunicationProtocol _protocol;
        private readonly int _pin;
        private readonly DhtType _dhtType;
        private I2cDevice _sensor;
        private GpioController _controller;

        private Stopwatch _stopwatch = new Stopwatch();

        private int _lastMeasurement = 0;

        /// <summary>
        /// How last read went, <c>true</c> for success, <c>false</c> for failure
        /// </summary>
        public bool IsLastReadSuccessful { get; internal set; }

        /// <summary>
        /// Get the last read temperature
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public Temperature Temperature
        {
            get
            {
                ReadData();

                switch (_dhtType)
                {
                    case DhtType.DHT11:
                        return Temperature.FromCelsius(GetHumidityDht11());
                    case DhtType.DHT12:
                    case DhtType.DHT21:
                    case DhtType.DHT22:
                        return Temperature.FromCelsius(GetHumidityDht22());
                    default:
                        return Temperature.FromCelsius(double.MaxValue);
                }
            }
        }

        /// <summary>
        /// Get the last read of relative humidity in percentage
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public double Humidity
        {
            get
            {
                ReadData();

                switch (_dhtType)
                {
                    case DhtType.DHT11:
                        return GetHumidityDht11();
                    case DhtType.DHT12:
                    case DhtType.DHT21:
                    case DhtType.DHT22:
                        return GetHumidityDht22();
                    default:
                        return double.MaxValue;
                }
            }
        }

        /// <summary>
        /// Create a DHT sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        /// <param name="dhtType">The DHT Type, either Dht11 or Dht22</param>
        /// <param name="pinNumberingScheme">The GPIO pin numbering scheme</param>
        public DhtSensor(int pin, DhtType dhtType, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical)
        {
            _protocol = CommunicationProtocol.OneWire;
            _controller = new GpioController(pinNumberingScheme);
            _pin = pin;
            _dhtType = dhtType;

            _controller.OpenPin(_pin);
            // keep data line HIGH
            _controller.SetPinMode(_pin, PinMode.Output);
            _controller.Write(_pin, PinValue.High);
            // delay 1s to make sure DHT stable
            DelayHelper.DelayMilliseconds(1000, true);
        }

        /// <summary>
        /// Create a DHT sensor through I2C (Only DHT12)
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        public DhtSensor(I2cDevice sensor)
        {
            _protocol = CommunicationProtocol.I2C;
            _sensor = sensor;
            _dhtType = DhtType.DHT12;
        }

        /// <summary>
        /// Start a reading
        /// </summary>
        /// <returns>
        /// <c>true</c> if read is successfull, otherwise <c>false</c>.
        /// </returns>
        private void ReadData()
        {
            // The time of two measurements should be more than 1s.
            if (Environment.TickCount - _lastMeasurement < 1000)
            {
                return;
            }

            if (_protocol == CommunicationProtocol.OneWire)
            {
                ReadThroughOneWire();
            }
            else
            {
                ReadThroughI2c();
            }
        }

        /// <summary>
        /// Read through One-Wire
        /// </summary>
        /// <returns>
        /// <c>true</c> if read is successfull, otherwise <c>false</c>.
        /// </returns>
        private bool ReadThroughOneWire()
        {
            _controller.Write(_pin, PinValue.Low);
            // wait 18 milliseconds
            DelayHelper.DelayMilliseconds(18, true);

            _controller.Write(_pin, PinValue.High);
            // wait 20 - 40 microseconds
            DelayHelper.DelayMicroseconds(30, true);

            _controller.SetPinMode(_pin, PinMode.Input);

            // DHT corresponding signal - LOW - 80 microseconds
            while (_controller.Read(_pin) == PinValue.Low)
                ;
            // HIGH - 80 microseconds
            while (_controller.Read(_pin) == PinValue.High)
                ;

            // the read data contains 40 bits
            byte readVal = 0;
            for (int i = 0; i < 40; i++)
            {
                // beginning signal per bit, 50 microseconds
                while (_controller.Read(_pin) == PinValue.Low)
                    ;

                // 26 - 28 microseconds represent 0
                // 70 microseconds represent 1
                _stopwatch.Restart();
                while (_controller.Read(_pin) == PinValue.High)
                    ;
                _stopwatch.Stop();

                // bit to byte
                readVal <<= 1;
                if (_stopwatch.Elapsed.TotalMilliseconds > 0.028)
                {
                    readVal |= 1;
                }

                if (((i + 1) % 8) == 0)
                {
                    _readBuff[i / 8] = readVal;
                }
            }

            _lastMeasurement = Environment.TickCount;

            // keep data line HIGH
            _controller.SetPinMode(_pin, PinMode.Output);
            _controller.Write(_pin, PinValue.High);

            if ((_readBuff[4] == ((_readBuff[0] + _readBuff[1] + _readBuff[2] + _readBuff[3]) & 0xFF)))
            {
                IsLastReadSuccessful = (_readBuff[0] != 0) || (_readBuff[2] != 0);
            }
            else
            {
                IsLastReadSuccessful = false;
            }

            return IsLastReadSuccessful;
        }

        /// <summary>
        /// Read through I2C
        /// </summary>
        /// <returns>
        /// <c>true</c> if read is successfull, otherwise <c>false</c>.
        /// </returns>
        private bool ReadThroughI2c()
        {
            // DHT12 Humidity Register
            _sensor.WriteByte(0x00);
            // humidity int, humidity decimal, temperature int, temperature decimal, checksum
            _sensor.Read(_readBuff);

            _lastMeasurement = Environment.TickCount;

            if ((_readBuff[4] == ((_readBuff[0] + _readBuff[1] + _readBuff[2] + _readBuff[3]) & 0xFF)))
            {
                IsLastReadSuccessful = (_readBuff[0] != 0) || (_readBuff[2] != 0);
            }
            else
            {
                IsLastReadSuccessful = false;
            }

            return IsLastReadSuccessful;
        }

        // Convertion for DHT11
        // The meaning of 0.1 is to convert byte to decimal
        private double GetTempDht11() => IsLastReadSuccessful ? _readBuff[2] + _readBuff[3] * 0.1 : double.MaxValue;

        private double GetHumidityDht11() => IsLastReadSuccessful ? _readBuff[0] + _readBuff[1] * 0.1 : double.MaxValue;

        // convertion for DHT12, DHT21, DHT22
        private double GetTempDht22()
        {
            if (IsLastReadSuccessful)
            {
                var temp = _readBuff[2] + (_readBuff[3] & 0x7F) * 0.1;
                // if MSB = 1 we have negative temperature
                return ((_readBuff[3] & 0x80) == 0 ? temp : -temp);
            }
            else
                return (double.MaxValue);
        }

        private double GetHumidityDht22() => IsLastReadSuccessful ? _readBuff[0] + _readBuff[1] * 0.1 : double.MaxValue;

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _controller?.Dispose();
            _sensor?.Dispose();
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
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

        // wait about 1 ms
        private readonly uint _loopCount = 10000;

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
        /// If last read was not successfull, it returns double.NaN
        /// </remarks>
        public Temperature Temperature
        {
            get
            {
                ReadData();

                switch (_dhtType)
                {
                    case DhtType.Dht11:
                        return Temperature.FromCelsius(GetTempDht11());
                    case DhtType.Dht12:
                    case DhtType.Dht21:
                    case DhtType.Dht22:
                        return Temperature.FromCelsius(GetTempDht22());
                    default:
                        return Temperature.FromCelsius(double.NaN);
                }
            }
        }

        /// <summary>
        /// Get the last read of relative humidity in percentage
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.NaN
        /// </remarks>
        public double Humidity
        {
            get
            {
                ReadData();

                switch (_dhtType)
                {
                    case DhtType.Dht11:
                        return GetHumidityDht11();
                    case DhtType.Dht12:
                    case DhtType.Dht21:
                    case DhtType.Dht22:
                        return GetHumidityDht22();
                    default:
                        return double.NaN;
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
            // delay 1s to make sure DHT stable
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Create a DHT sensor through I2C (Only DHT12)
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        public DhtSensor(I2cDevice sensor)
        {
            _protocol = CommunicationProtocol.I2C;
            _sensor = sensor;
            _dhtType = DhtType.Dht12;
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
            byte readVal = 0;
            uint count;

            // keep data line HIGH
            _controller.SetPinMode(_pin, PinMode.Output);
            _controller.Write(_pin, PinValue.High);
            DelayHelper.DelayMilliseconds(20, true);

            // send trigger signal
            _controller.Write(_pin, PinValue.Low);
            // wait at least 18 milliseconds
            // here wait for 18 milliseconds will cause sensor initialization to fail
            DelayHelper.DelayMilliseconds(20, true);

            // pull up data line
            _controller.Write(_pin, PinValue.High);
            // wait 20 - 40 microseconds
            DelayHelper.DelayMicroseconds(40, true);

            _controller.SetPinMode(_pin, PinMode.InputPullUp);

            // DHT corresponding signal - LOW - about 80 microseconds
            DelayHelper.DelayMicroseconds(80, true);

            // HIGH - about 80 microseconds
            DelayHelper.DelayMicroseconds(80, true);

            // the read data contains 40 bits
            for (int i = 0; i < 40; i++)
            {
                // beginning signal per bit, about 50 microseconds
                count = _loopCount;
                while (_controller.Read(_pin) == PinValue.Low)
                {
                    if (count-- == 0)
                    {
                        IsLastReadSuccessful = false;
                        return IsLastReadSuccessful;
                    }
                }

                // 26 - 28 microseconds represent 0
                // 70 microseconds represent 1
                _stopwatch.Restart();
                count = _loopCount;
                while (_controller.Read(_pin) == PinValue.High)
                {
                    if (count-- == 0)
                    {
                        IsLastReadSuccessful = false;
                        return IsLastReadSuccessful;
                    }
                }
                _stopwatch.Stop();

                // bit to byte
                // less than 40 microseconds can be considered as 0, not necessarily less than 28 microseconds
                // here take 30 microseconds
                readVal <<= 1;
                if (!(_stopwatch.ElapsedTicks * 1000000F / Stopwatch.Frequency <= 30))
                {
                    readVal |= 1;
                }

                if (((i + 1) % 8) == 0)
                {
                    _readBuff[i / 8] = readVal;
                }
            }

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

        // convertion for DHT11
        // the meaning of 0.1 is to convert byte to decimal
        private double GetTempDht11() => IsLastReadSuccessful ? _readBuff[2] + _readBuff[3] * 0.1 : double.NaN;

        private double GetHumidityDht11() => IsLastReadSuccessful ? _readBuff[0] + _readBuff[1] * 0.1 : double.NaN;

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
                return double.NaN;
        }

        private double GetHumidityDht22() => IsLastReadSuccessful ? _readBuff[0] + _readBuff[1] * 0.1 : double.NaN;

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

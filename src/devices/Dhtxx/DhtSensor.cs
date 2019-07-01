// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c.Devices;
using System.Diagnostics;
using System.Threading;
using Dhtxx.Devices;
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
        private readonly byte[] _readBuff = new byte[5];
        private readonly CommunicationProtocol _protocol;
        private readonly int _pin;
        private readonly I2cDevice _i2cDevice;
        private readonly GpioController _controller;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _lastMeasurement = 0;
        private IDhtDevice _device;

        /// <summary>
        /// How last read went, <c>true</c> for success, <c>false</c> for failure
        /// </summary>
        public bool IsLastReadSuccessful { get; private set; }

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
                return IsLastReadSuccessful ? _device.GetTemperature(_readBuff) : Temperature.FromCelsius(double.NaN);
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
                return IsLastReadSuccessful ? _device.GetHumidity(_readBuff) : double.NaN;
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
            SetDevice(dhtType);

            _controller.OpenPin(_pin);
            // delay 1s to make sure DHT stable
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Create a DHT sensor through I2C (Only DHT12)
        /// </summary>
        /// <param name="i2cDevice">>The I2C device used for communication.</param>
        public DhtSensor(I2cDevice i2cDevice)
        {
            _protocol = CommunicationProtocol.I2C;
            _i2cDevice = i2cDevice;
            SetDevice(DhtType.Dht12);
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
            DelayHelper.DelayMicroseconds(30, true);

            _controller.SetPinMode(_pin, PinMode.InputPullUp);

            // DHT corresponding signal - LOW - about 80 microseconds
            count = _loopCount;
            while (_controller.Read(_pin) == PinValue.Low)
            {
                if (count-- == 0)
                {
                    IsLastReadSuccessful = false;
                    return IsLastReadSuccessful;
                }
            }

            // HIGH - about 80 microseconds
            count = _loopCount;
            while (_controller.Read(_pin) == PinValue.High)
            {
                if (count-- == 0)
                {
                    IsLastReadSuccessful = false;
                    return IsLastReadSuccessful;
                }
            }

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
            _i2cDevice.WriteByte(0x00);
            // humidity int, humidity decimal, temperature int, temperature decimal, checksum
            _i2cDevice.Read(_readBuff);

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
        /// Initialise the sensor instance with the corresponding device based on DhtType.
        /// </summary>
        private void SetDevice(DhtType type)
        {
            switch (type)
            {
                case DhtType.Dht11:
                    _device = new Dht11Device();
                    break;
                case DhtType.Dht12:
                case DhtType.Dht21:
                case DhtType.Dht22:
                    _device = new Dht22Device();
                    break;
            }
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _controller?.Dispose();
            _i2cDevice?.Dispose();
        }
    }
}

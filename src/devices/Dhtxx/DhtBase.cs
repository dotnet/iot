// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Model;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.DHTxx
{
    /// <summary>
    /// Temperature and Humidity Sensor DHTxx
    /// </summary>
    [Interface("Temperature and Humidity Sensor DHTxx")]
    public abstract class DhtBase : IDisposable
    {
        private const int BufferSize = 5;
        private readonly CommunicationProtocol _protocol;

        /// <summary>
        /// Read buffer
        /// </summary>
        private byte[] _readBuff = new byte[BufferSize];

        /// <summary>
        /// GPIO pin
        /// </summary>
        protected readonly int _pin;

        /// <summary>
        /// True to dispose the Gpio Controller
        /// </summary>
        protected readonly bool _shouldDispose;

        /// <summary>
        /// I2C device used to communicate with the device
        /// </summary>
        protected I2cDevice? _i2cDevice;

        /// <summary>
        /// <see cref="GpioController"/> related with the <see cref="_pin"/>.
        /// </summary>
        protected GpioController? _controller;

        // wait about 1 ms
        private readonly uint _loopCount = 10000;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _lastMeasurement = 0;
        private TimeSpan _minTimeBetweenReads;

        /// <summary>
        /// True when the last read values are valid
        /// </summary>
        protected bool _isLastReadSuccessful;

        /// <summary>
        /// How last read went, <c>true</c> for success, <c>false</c> for failure
        /// </summary>
        [Obsolete("This property will be removed in a future release.")]
        public bool IsLastReadSuccessful
        {
            get => _isLastReadSuccessful;
        }

        /// <summary>
        /// Get the last read temperature
        /// </summary>
        /// <remarks>
        /// If last read was not successful, it returns <code>default(Temperature)</code>
        /// </remarks>
        [Obsolete("This property will be removed in the a future release. Use TryGetTemperature instead.")]
        public virtual Temperature Temperature
        {
            get
            {
                var buf = ReadData();
                return IsLastReadSuccessful ? GetTemperature(buf) : default(Temperature);
            }
        }

        /// <summary>
        /// Get the last read of relative humidity in percentage
        /// </summary>
        /// <remarks>
        /// If last read was not successful, it returns <code>default(RelativeHumidity)</code>
        /// </remarks>
        [Obsolete("This property will be removed in a future release. Use TryGetHumidity instead.")]
        public virtual RelativeHumidity Humidity
        {
            get
            {
                var buf = ReadData();
                return IsLastReadSuccessful ? GetHumidity(buf) : default(RelativeHumidity);
            }
        }

        /// <summary>
        /// Gets or sets the minimum time between sensor reads.
        /// When querying the sensor faster than this, the last values will be returned (whether valid or not)
        /// </summary>
        public TimeSpan MinTimeBetweenReads
        {
            get
            {
                return _minTimeBetweenReads;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Minimum time between reads must be larger than zero");
                }

                _minTimeBetweenReads = value;
            }
        }

        /// <summary>
        /// Create a DHT sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        /// <param name="gpioController"><see cref="GpioController"/> related with operations on pins</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public DhtBase(int pin, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _protocol = CommunicationProtocol.OneWire;
            _shouldDispose = shouldDispose || gpioController is null;
            _controller = gpioController ?? new GpioController();
            _pin = pin;

            // These sensors typically require 2.5 seconds between read attempts, or the result will be garbage
            MinTimeBetweenReads = TimeSpan.FromSeconds(2.5);
            _controller.OpenPin(_pin);
            // delay 1s to make sure DHT stable
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Create a DHT sensor through I2C (Only DHT12)
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public DhtBase(I2cDevice i2cDevice)
        {
            _protocol = CommunicationProtocol.I2C;
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Read data from the sensor. Returns the last read value if the last read operation was too close.
        /// </summary>
        /// <returns>The raw data last read</returns>
        internal virtual byte[] ReadData()
        {
            // The time of two measurements should be more than 1s.
            if (Environment.TickCount - _lastMeasurement < MinTimeBetweenReads.Milliseconds)
            {
                return _readBuff;
            }

            if (_protocol == CommunicationProtocol.OneWire)
            {
                _readBuff = ReadThroughOneWire();
            }
            else
            {
                _readBuff = ReadThroughI2c();
            }

            return _readBuff;
        }

        /// <summary>
        /// Read through One-Wire
        /// </summary>
        internal virtual byte[] ReadThroughOneWire()
        {
            if (_controller is null)
            {
                throw new Exception("GPIO controller is not configured.");
            }

            byte readVal = 0;
            uint count;
            var pinMode = _controller.IsPinModeSupported(_pin, PinMode.InputPullUp) ? PinMode.InputPullUp : PinMode.Input;

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

            _controller.SetPinMode(_pin, pinMode);

            // DHT corresponding signal - LOW - about 80 microseconds
            count = _loopCount;
            byte[] newBuf = new byte[BufferSize];
            while (_controller.Read(_pin) == PinValue.Low)
            {
                if (count-- == 0)
                {
                    _isLastReadSuccessful = false;
                    return newBuf;
                }
            }

            // HIGH - about 80 microseconds
            count = _loopCount;
            while (_controller.Read(_pin) == PinValue.High)
            {
                if (count-- == 0)
                {
                    _isLastReadSuccessful = false;
                    return newBuf;
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
                        _isLastReadSuccessful = false;
                        return newBuf;
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
                        _isLastReadSuccessful = false;
                        return newBuf;
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
                    newBuf[i / 8] = readVal;
                }
            }

            _lastMeasurement = Environment.TickCount;

            if ((newBuf[4] == ((newBuf[0] + newBuf[1] + newBuf[2] + newBuf[3]) & 0xFF)))
            {
                _isLastReadSuccessful = !((newBuf[0] == 0) && (newBuf[1] == 0) && (newBuf[2] == 0) && (newBuf[3] == 0));
            }
            else
            {
                _isLastReadSuccessful = false;
            }

            return newBuf;
        }

        /// <summary>
        /// Read through I2C
        /// </summary>
        internal virtual byte[] ReadThroughI2c()
        {
            if (_i2cDevice is null)
            {
                throw new Exception("I2C device is not configured");
            }

            // DHT12 Humidity Register
            _i2cDevice.WriteByte(0x00);
            // humidity int, humidity decimal, temperature int, temperature decimal, checksum
            byte[] newBuf = new byte[BufferSize];
            _i2cDevice.Read(newBuf.AsSpan());

            _lastMeasurement = Environment.TickCount;

            if ((newBuf[4] == ((newBuf[0] + newBuf[1] + newBuf[2] + newBuf[3]) & 0xFF)))
            {
                _isLastReadSuccessful = (newBuf[0] != 0) || (newBuf[2] != 0);
            }
            else
            {
                _isLastReadSuccessful = false;
            }

            return newBuf;
        }

        /// <summary>
        /// Returns the current temperature
        /// </summary>
        /// <param name="temperature">[Out] The current temperature on success</param>
        /// <returns>True on success, false if reading failed</returns>
        [Telemetry("Temperature")]
        public bool TryReadTemperature(
#if NET5_0_OR_GREATER
        [NotNullWhen(true)]
#endif
        out Temperature temperature)
        {
            temperature = default;
            var buf = ReadData();
            if (_isLastReadSuccessful)
            {
                temperature = GetTemperature(buf);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the current relative humidity
        /// </summary>
        /// <param name="humidity">[Out] The current relative humidity on success</param>
        /// <returns>True on success, false if reading failed</returns>
        [Telemetry("Humidity")]
        public bool TryReadHumidity(
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out RelativeHumidity humidity)
        {
            humidity = default;
            var buf = ReadData();
            if (_isLastReadSuccessful)
            {
                humidity = GetHumidity(buf);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converting data to humidity
        /// </summary>
        /// <param name="readBuff">Data</param>
        /// <returns>Humidity</returns>
        internal abstract RelativeHumidity GetHumidity(Span<byte> readBuff);

        /// <summary>
        /// Converting data to Temperature
        /// </summary>
        /// <param name="readBuff">Data</param>
        /// <returns>Temperature</returns>
        internal abstract Temperature GetTemperature(Span<byte> readBuff);

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }
            else if (_controller?.IsPinOpen(_pin) ?? false)
            {
                _controller.ClosePin(_pin);
            }

            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}

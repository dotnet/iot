// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Linq;
using UnitsNet;

namespace Iot.Device.Hx711
{
    /// <summary>
    /// Hx711 - Weight scale Module
    /// </summary>
    public sealed class Hx711 : IDisposable
    {
        private readonly GpioController _gpioController;
        private readonly bool _shouldDispose;

        private readonly int _pinDout;
        private readonly int _pinPdSck;

        private readonly Hx711Options _options;
        private readonly object _readLock;
        private readonly Hx711Reader _reader;

        private readonly List<double> _conversionRatioList = new();

        private bool _isInitialize;
        private bool _isCalibrated;
        private int _tareValue;

        /// <summary>
        /// Offset value from 0 at startup
        /// </summary>
        private int _offsetFormZero;

        /// <summary>
        /// Conversion ratio between Hx711 units and grams
        /// </summary>
        public double ConversionRatio { get; private set; }

        /// <summary>
        /// Weight set as tare
        /// </summary>
        public Mass TareValue
        {
            get
            {
                return ConversionRatio == 0 ? Mass.FromGrams(_tareValue) : Mass.FromGrams(_tareValue / ConversionRatio);
            }
        }

        /// <summary>
        /// Creates a new instance of the Hx711 module.
        /// </summary>
        /// <param name="pinDout">Trigger pulse output. (Digital OUTput)</param>
        /// <param name="pinPdSck">Trigger pulse input. (Power Down control and Serial Clock input)</param>
        /// <param name="options">How to use the Hx711 module.</param>
        /// <param name="gpioController">GPIO controller related with the pins.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller.</param>
        public Hx711(int pinDout, int pinPdSck, Hx711Options? options = null, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _pinDout = pinDout;
            _pinPdSck = pinPdSck;

            _shouldDispose = shouldDispose || gpioController is null;
            _gpioController = gpioController ?? new();

            // Mutex for reading from the Hx711, in case multiple threads in client
            // software try to access get values from the class at the same time.
            _readLock = new object();

            _gpioController.OpenPin(_pinPdSck, PinMode.Output);
            _gpioController.OpenPin(_pinDout, PinMode.Input);

            // Needed initialize
            _isInitialize = false;

            // Needed calibration
            _isCalibrated = false;

            _offsetFormZero = 0;
            _tareValue = 0;
            ConversionRatio = 0;

            _options = options ?? new Hx711Options();

            _reader = new Hx711Reader(_gpioController, _options, pinDout, pinPdSck, _readLock);
        }

        /// <summary>
        /// Load cells always return different values also based on their range and sensitivity.
        /// For this reason, a first calibration step with a known weight is required.
        /// You can repeat it several times to get a more precise value.
        /// </summary>
        /// <param name="knowWeight">Known weight currently on load cell and detected by the Hx711.</param>
        /// <param name="numberOfReads">Number of readings to take from which to average, to get a more accurate value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throw if know weight have invalid value</exception>
        public void SetCalibration(Mass knowWeight, int numberOfReads = 15)
        {
            if (knowWeight.Grams == 0)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(knowWeight), message: "Param value must be greater than zero!");
            }

            var readValue = _reader.Read(numberOfReads, _offsetFormZero);

            lock (_readLock)
            {
                var referenceValue = readValue / knowWeight.Grams;

                // If we do several calibrations, the most accurate value is the average.
                _conversionRatioList.Add(referenceValue);
                ConversionRatio = _conversionRatioList.Average();

                _isCalibrated = true;
            }
        }

        /// <summary>
        /// If you already know the reference unit between the Hx711 value and grams, you can set it and skip the calibration.
        /// </summary>
        /// <param name="conversionRatio">Conversion ratio between Hx711 units and grams</param>
        /// <exception cref="ArgumentOutOfRangeException">Throw if know weight have invalid value</exception>
        public void SetConversionRatio(double conversionRatio)
        {
            if (conversionRatio == 0)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(conversionRatio), message: "Param value must be greater than zero!");
            }

            lock (_readLock)
            {
                ConversionRatio = conversionRatio;

                // Calibration no longer required
                _isCalibrated = true;
            }
        }

        /// <summary>
        /// Read the weight from the Hx711 through channel A to which the load cell is connected,
        /// range and precision depend on load cell connected.
        /// </summary>
        /// <param name="numberOfReads">Number of readings to take from which to average, to get a more accurate value.</param>
        /// <returns>Return a weigh read from Hx711</returns>
        public Mass GetWeight(int numberOfReads = 3)
        {
            if (!_isCalibrated)
            {
                throw new Hx711CalibrationNotDoneException();
            }

            // Lock is internal in fisical read
            var value = GetNetWeight(numberOfReads);

            return Mass.FromGrams(Math.Round(value / ConversionRatio, digits: 0));
        }

        /// <summary>
        /// Sets tare for channel A for compatibility purposes
        /// </summary>
        /// <param name="numberOfReads">Number of readings to take from which to average, to get a more accurate value.</param>
        public void Tare(int numberOfReads = 15)
        {
            lock (_readLock)
            {
                if (!_isCalibrated)
                {
                    throw new Hx711CalibrationNotDoneException();
                }

                _tareValue = GetNetWeight(numberOfReads);
            }
        }

        /// <summary>
        /// Power up Hx711 and set it ready to work
        /// </summary>
        public void PowerUp()
        {
            // Wait for and get the Read Lock, incase another thread is already
            // driving the Hx711 serial interface.
            lock (_readLock)
            {
                // Lower the Hx711 Digital Serial Clock (PD_SCK) line.
                // Docs says "When PD_SCK Input is low, chip is in normal working mode."
                // page 5
                // https://html.alldatasheet.com/html-pdf/1132222/AVIA/Hx711/573/5/Hx711.html
                _gpioController.Write(_pinPdSck, PinValue.Low);

                // Wait 100µs for the Hx711 to power back up.
                DelayHelper.DelayMicroseconds(microseconds: 100, allowThreadYield: true);

                // Release the Read Lock, now that we've finished driving the Hx711
                // serial interface.
            }

            // Hx711 will now be defaulted to Channel A with gain of 128. If this
            // isn't what client software has requested from us, take a sample and
            // throw it away, so that next sample from the Hx711 will be from the
            // correct channel/gain.
            if (_options.Mode != Hx711Mode.ChannelAGain128 || !_isInitialize)
            {
                _ = _reader.Read(numberOfReads: 15, offsetFromZero: 0);
                _isInitialize = true;
            }

            // Read offset from 0
            var value = _reader.Read(numberOfReads: 15, offsetFromZero: 0);
            _offsetFormZero = value;
        }

        /// <summary>
        /// Power down Hx711
        /// </summary>
        public void PowerDown()
        {
            // Wait for and get the Read Lock, incase another thread is already
            // driving the Hx711 serial interface.
            lock (_readLock)
            {
                // Cause a rising edge on Hx711 Digital Serial Clock (PD_SCK). We then
                // leave it held up and wait 100µs. After 60µs the Hx711 should be
                // powered down.
                // Docs says "When PD_SCK pin changes from low to high
                // and stays at high for longer than 60µs, Hx711
                // enters power down mode", page 5 https://html.alldatasheet.com/html-pdf/1132222/AVIA/Hx711/573/5/Hx711.html
                _gpioController.Write(_pinPdSck, PinValue.Low);
                _gpioController.Write(_pinPdSck, PinValue.High);

                DelayHelper.DelayMicroseconds(microseconds: 65, allowThreadYield: true);

                // Release the Read Lock, now that we've finished driving the Hx711
                // serial interface.
            }
        }

        /// <summary>
        /// PowerDown and restart component
        /// </summary>
        public void Reset()
        {
            PowerDown();
            PowerUp();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _gpioController?.Dispose();
            }
            else
            {
                _gpioController?.ClosePin(_pinPdSck);
                _gpioController?.ClosePin(_pinDout);
            }
        }

        /// <summary>
        /// Read weight from Hx711
        /// </summary>
        /// <param name="numberOfReads">Number of readings to take from which to average, to get a more accurate value.</param>
        /// <returns>Return total weight - tare weight</returns>
        private int GetNetWeight(int numberOfReads) => _reader.Read(numberOfReads, _offsetFormZero) - _tareValue;
    }
}

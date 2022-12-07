// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using UnitsNet;

namespace Iot.Device.HX711
{
    /// <summary>
    /// HX711 - Weight scale Module
    /// </summary>
    /// <remarks>DataSheet: https://html.alldatasheet.com/html-pdf/1132222/AVIA/HX711/109/1/HX711.html</remarks>
    public sealed class HX711 : IDisposable
    {
        private readonly GpioController _gpioController;
        private readonly bool _shouldDispose;

        private readonly int _pinDout;
        private readonly int _pinPD_Sck;

        private readonly HX711Options _options;
        private readonly object _readLock;
        private readonly HX711Reader _reader;

        private bool _isInitialize;
        private bool _isCalibrated;

        /// <summary>
        /// Conversion ratio between Hx711 units and grams
        /// </summary>
        public double ReferanceUnit { get; private set; }
        private readonly List<double> _referenceUnitList = new();

        /// <summary>
        /// Offset value from 0 at startup
        /// </summary>
        private int _offsetFormZero;

        private int _tareValue;
        public Mass TareValue => Mass.FromGrams(Math.Round(_tareValue / this.ReferanceUnit));

        /// <summary>
        /// Creates a new instance of the HX711 module.
        /// </summary>
        /// <param name="pinDout">Trigger pulse output. (Digital OUTput)</param>
        /// <param name="pinPD_Sck">Trigger pulse input. (Power Down control and Serial Clock input)</param>
        /// <param name="mode">How to use the HX711 module.</param>
        /// <param name="gpioController">GPIO controller related with the pins.</param>
        /// <param name="pinNumberingScheme">Scheme and numeration used by controller.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller.</param>
        public HX711(int pinDout, int pinPD_Sck, HX711Options? options = null, GpioController? gpioController = null,
            PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
        {
            _pinDout = pinDout;
            _pinPD_Sck = pinPD_Sck;

            _shouldDispose = shouldDispose || gpioController is null;
            _gpioController = gpioController ?? new(pinNumberingScheme);

            // Mutex for reading from the HX711, in case multiple threads in client
            // software try to access get values from the class at the same time.
            _readLock = new object();

            _gpioController.OpenPin(_pinPD_Sck, PinMode.Output);
            _gpioController.OpenPin(_pinDout, PinMode.Input);

            // Needed initialize
            _isInitialize = false;

            // Needed calibration
            _isCalibrated = false;

            _offsetFormZero = 0;
            _tareValue = 0;

            _options = options ?? new HX711Options();

            _reader = new HX711Reader(_gpioController, _options, pinDout, pinPD_Sck, _readLock);
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
                _referenceUnitList.Add(referenceValue);
                this.ReferanceUnit = _referenceUnitList.Average();

                _isCalibrated = true;
            }
        }

        /// <summary>
        /// If you already know the reference unit between the Hx711 value and grams, you can set it and skip the calibration.
        /// </summary>
        /// <param name="referenceUnit">Conversion ratio between Hx711 units and grams</param>
        public void SetReferenceUnit(double referenceUnit)
        {
            lock (_readLock)
            {
                this.ReferanceUnit = referenceUnit;

                // Calibration no longer required
                _isCalibrated = true;
            }
        }

        /// <summary>
        /// Read the weight from the HX711 through channel A to which the load cell is connected,
        /// range and precision depend on load cell connected.
        /// </summary>
        /// <param name="numberOfReads">Number of readings to take from which to average, to get a more accurate value.</param>
        /// <returns>Return a weigh read from HX711</returns>
        public Mass GetWeight(int numberOfReads = 3)
        {
            if (!_isCalibrated)
            {
                throw new HX711CalibrationNotDoneException();
            }

            // Lock is internal in fisical read
            var value = this.GetNetWeight(numberOfReads);
            return Mass.FromGrams(Math.Round(value / this.ReferanceUnit, digits: 0));
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
                    throw new HX711CalibrationNotDoneException();
                }

                _tareValue = this.GetNetWeight(numberOfReads);
            }
        }

        /// <summary>
        /// Power up HX711 and set it ready to work
        /// </summary>
        public void PowerUp()
        {
            // Wait for and get the Read Lock, incase another thread is already
            // driving the HX711 serial interface.
            lock (_readLock)
            {
                // Lower the HX711 Digital Serial Clock (PD_SCK) line.
                // Docs says "When PD_SCK Input is low, chip is in normal working mode."
                // page 5
                // https://html.alldatasheet.com/html-pdf/1132222/AVIA/HX711/573/5/HX711.html
                _gpioController.Write(_pinPD_Sck, PinValue.Low);

                // Wait 100µs for the HX711 to power back up.
                DelayHelper.DelayMicroseconds(microseconds: 100, allowThreadYield: true);

                // Release the Read Lock, now that we've finished driving the HX711
                // serial interface.
            }

            // HX711 will now be defaulted to Channel A with gain of 128. If this
            // isn't what client software has requested from us, take a sample and
            // throw it away, so that next sample from the HX711 will be from the
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
        /// Power down HX711
        /// </summary>
        public void PowerDown()
        {
            // Wait for and get the Read Lock, incase another thread is already
            // driving the HX711 serial interface.
            lock (_readLock)
            {
                // Cause a rising edge on HX711 Digital Serial Clock (PD_SCK). We then
                // leave it held up and wait 100µs. After 60µs the HX711 should be
                // powered down.
                // Docs says "When PD_SCK pin changes from low to high
                // and stays at high for longer than 60µs, HX711
                // enters power down mode", page 5 https://html.alldatasheet.com/html-pdf/1132222/AVIA/HX711/573/5/HX711.html
                _gpioController.Write(_pinPD_Sck, PinValue.Low);
                _gpioController.Write(_pinPD_Sck, PinValue.High);

                DelayHelper.DelayMicroseconds(microseconds: 65, allowThreadYield: true);

                // Release the Read Lock, now that we've finished driving the HX711
                // serial interface.
            }
        }

        /// <summary>
        /// PowerDown and restart component
        /// </summary>
        public void Reset()
        {
            this.PowerDown();
            this.PowerUp();
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
                _gpioController?.ClosePin(_pinPD_Sck);
                _gpioController?.ClosePin(_pinDout);
            }
        }

        /// <summary>
        /// Read weight from HX711 
        /// </summary>
        /// <param name="numberOfReads">Number of readings to take from which to average, to get a more accurate value.</param>
        /// <returns>Return total weight - tare weight</returns>
        private int GetNetWeight(int numberOfReads) => _reader.Read(numberOfReads, _offsetFormZero) - _tareValue;
    }
}
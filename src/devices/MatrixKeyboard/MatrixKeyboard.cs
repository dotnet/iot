// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;

namespace Iot.Device.MatrixKeyboard
{
    /// <summary>
    /// GPIO Matrix Keyboard Driver
    /// </summary>
    public class MatrixKeyboard : IDisposable
    {
        /// <summary>
        /// Get row pins
        /// </summary>
        public IEnumerable<int> RowPins => _rowPins;

        /// <summary>
        /// Get column pins
        /// </summary>
        public IEnumerable<int> ColumnPins => _columnPins;

        /// <summary>
        /// Get or set scanning frequency in Hertz
        /// </summary>
        public double ScanFrequency
        {
            get => _scanFrequency;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Scanning frequency must be positive");
                }
                _scanFrequency = value;
                _scanInterval = CalcScanInterval(_scanFrequency);
            }
        }

        /// <summary>
        /// Get whether this keyboard is scanning
        /// </summary>
        public bool IsScanning { get; private set; }

        private readonly int[] _rowPins;
        private readonly int[] _columnPins;
        private int _currentRow;
        private readonly PinValue[] _buttonValues;
        private double _scanFrequency;

        /// <summary>
        /// Initialize keyboard
        /// </summary>
        /// <param name="gpioController">GPIO controller</param>
        /// <param name="rowPins">Row pins</param>
        /// <param name="colPins">Column pins</param>
        /// <param name="scanFreq">Scanning frequency</param>
        public MatrixKeyboard(IGpioController gpioController, IEnumerable<int> rowPins, IEnumerable<int> colPins, double scanFreq = 50)
        {
            _gpio = gpioController ?? throw new ArgumentNullException(nameof(gpioController));

            if (rowPins == null)
            {
                throw new ArgumentNullException(nameof(rowPins));
            }
            if (rowPins.Count() < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rowPins), "The number of rows is at least 1");
            }

            if (colPins == null)
            {
                throw new ArgumentNullException(nameof(colPins));
            }
            if (colPins.Count() < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(colPins), "The number of columns is at least 1");
            }

            if (scanFreq <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scanFreq), "Scanning frequency must be positive");
            }

            _rowPins = rowPins.ToArray();
            _columnPins = colPins.ToArray();
            _buttonValues = new PinValue[_rowPins.Length * _columnPins.Length];

            _scanThread = new Thread(ScanThreadStart);
            IsScanning = false;
            ScanFrequency = scanFreq;
        }

        /// <summary>
        /// Open GPIO pins then start keyboard scanning
        /// </summary>
        public void StartScan()
        {
            if (!IsScanning)
            {
                try
                {
                    for (var i = 0; i < _rowPins.Length; i++)
                    {
                        _gpio.OpenPin(_rowPins[i], PinMode.Output);
                    }
                    for (var i = 0; i < _columnPins.Length; i++)
                    {
                        _gpio.OpenPin(_columnPins[i], PinMode.Input);
                    }
                }
                catch (Exception ex)
                {
                    IsScanning = false;
                    throw new Exception("Error while openning GPIO pins.", ex);
                }

                IsScanning = true;
                try
                {
                    _scanThread.Start();
                }
                catch (Exception ex)
                {
                    IsScanning = false;
                    throw new Exception("Error while starting scanning thread.", ex);
                }
            }
        }

        /// <summary>
        /// Stop keyboard scanning then close GPIO pins
        /// </summary>
        public void StopScan()
        {
            if (IsScanning)
            {
                IsScanning = false;
                Thread.Sleep(_scanInterval); // wait for thread ends

                try
                {
                    for (var i = 0; i < _rowPins.Length; i++)
                    {
                        _gpio.ClosePin(_rowPins[i]);
                    }
                    for (var i = 0; i < _columnPins.Length; i++)
                    {
                        _gpio.ClosePin(_columnPins[i]);
                    }
                }
                catch (Exception ex)
                {
                    IsScanning = false;
                    throw new Exception("Error while closing GPIO pins.", ex);
                }
            }
        }

        /// <summary>
        /// Get all buttons' values
        /// </summary>
        public ReadOnlySpan<PinValue> Values => _buttonValues.AsSpan();

        /// <summary>
        /// Get buttons' values by row
        /// </summary>
        /// <param name="row">Row index</param>
        public ReadOnlySpan<PinValue> RowValues(int row) => _buttonValues.AsSpan(row * _rowPins.Length, _columnPins.Length);

        /// <summary>
        /// Keyboard event
        /// </summary>
        public event PinChangeEventHandler PinChangeEvent;

        /// <summary>
        /// Delegate of keyboard event
        /// </summary>
        public delegate void PinChangeEventHandler(object sender, MatrixKeyboardEventArgs pinValueChangedEventArgs);

        public void Dispose()
        {
            IsScanning = false;
            _scanThread = null;

            _gpio?.Dispose();
            _gpio = null;
        }

        private IGpioController _gpio;
        private Thread _scanThread;
        private TimeSpan _scanInterval;
        private TimeSpan CalcScanInterval(double scanFrequency)
        {
            // "scanFrequency * _rowPins.Length" is how many rows to scan per second
            // it divide by 10000000(ticks) so we can get interval of the timer
            return new TimeSpan((long)(10000000 / (scanFrequency * _rowPins.Length)));
        }
        private void ScanThreadStart()
        {
            while (IsScanning)
            {
                for (var i = 0; i < _columnPins.Length; i++)
                {
                    var index = _currentRow * _columnPins.Length + i;

                    var oldValue = _buttonValues[index];
                    var newValue = _gpio.Read(_columnPins[i]);

                    _buttonValues[index] = newValue;
                    if (newValue != oldValue)
                    {
                        var args = new MatrixKeyboardEventArgs(newValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling, _currentRow, i);
                        PinChangeEvent(this, args);
                    }
                }

                _gpio.Write(_rowPins[_currentRow], PinValue.Low);
                _currentRow = (_currentRow + 1) % _rowPins.Length;
                _gpio.Write(_rowPins[_currentRow], PinValue.High);

                Thread.Sleep(_scanInterval);
            }
        }
    }
}

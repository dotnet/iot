// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        /// Get all buttons' values
        /// </summary>
        public ReadOnlySpan<PinValue> Values => _buttonValues.AsSpan();

        /// <summary>
        /// Get or set interval in milliseconds
        /// </summary>
        public int ScanInterval { get; set; }

        /// <summary>
        /// Delegate of keyboard event
        /// </summary>
        public delegate void PinChangeEventHandler(object sender, MatrixKeyboardEventArgs pinValueChangedEventArgs);

        private int _currentRow;
        private int[] _rowPins;
        private int[] _columnPins;
        private GpioController _masterGpioController;
        private PinValue[] _buttonValues;

        /// <summary>
        /// Initialize keyboard
        /// </summary>
        /// <param name="masterController">GPIO controller</param>
        /// <param name="rowPins">Row pins</param>
        /// <param name="colPins">Column pins</param>
        /// <param name="scanInterval">Scanning interval in milliseconds</param>
        public MatrixKeyboard(GpioController masterController, IEnumerable<int> rowPins, IEnumerable<int> colPins, int scanInterval)
        {
            _masterGpioController = masterController ?? throw new ArgumentNullException(nameof(masterController));

            if (rowPins == null)
            {
                throw new ArgumentNullException(nameof(rowPins));
            }
            if (rowPins.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(rowPins), "The number of rows is at least 1");
            }

            if (colPins == null)
            {
                throw new ArgumentNullException(nameof(colPins));
            }
            if (colPins.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(colPins), "The number of columns is at least 1");
            }

            if (scanInterval <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scanInterval), "Scanning interval must be positive");
            }

            _rowPins = rowPins.ToArray();
            _columnPins = colPins.ToArray();
            _buttonValues = new PinValue[_rowPins.Length * _columnPins.Length];

            ScanInterval = scanInterval;
        }

        /// <summary>
        /// Start a task to open GPIO pins then start keyboard scanning. Raises MatrixKeyboardEventArgs.
        /// </summary>
        /// <param name="token">A cancellation token that can be used to cancel the work</param>
        public Task ScanAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                OpenPins();
                while (!token.IsCancellationRequested)
                {
                    for (int i = 0; i < _columnPins.Length; i++)
                    {
                        int index = _currentRow * _columnPins.Length + i;

                        PinValue oldValue = _buttonValues[index];
                        PinValue newValue = _masterGpioController.Read(_columnPins[i]);

                        _buttonValues[index] = newValue;
                        if (newValue != oldValue)
                        {
                            MatrixKeyboardEventArgs args = new MatrixKeyboardEventArgs(newValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling, _currentRow, i);
                            PinChangeEvent(this, args);
                        }
                    }

                    _masterGpioController.Write(_rowPins[_currentRow], PinValue.Low);
                    _currentRow = (_currentRow + 1) % _rowPins.Length;
                    _masterGpioController.Write(_rowPins[_currentRow], PinValue.High);

                    Thread.Sleep(ScanInterval);
                }
                ClosePins();
            }, token);
        }

        /// <summary>
        /// Start a task to open GPIO pins then start keyboard scanning. End with a MatrixKeyboardEventArgs raises.
        /// </summary>
        /// <param name="token">A cancellation token that can be used to cancel the work</param>
        public Task<MatrixKeyboardEventArgs> ReadKeyAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                MatrixKeyboardEventArgs args = null;

                OpenPins();
                while (!token.IsCancellationRequested)
                {
                    for (int i = 0; i < _columnPins.Length; i++)
                    {
                        int index = _currentRow * _columnPins.Length + i;

                        PinValue oldValue = _buttonValues[index];
                        PinValue newValue = _masterGpioController.Read(_columnPins[i]);

                        _buttonValues[index] = newValue;
                        if (newValue != oldValue)
                        {
                            args = new MatrixKeyboardEventArgs(newValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling, _currentRow, i);
                        }
                    }

                    _masterGpioController.Write(_rowPins[_currentRow], PinValue.Low);
                    _currentRow = (_currentRow + 1) % _rowPins.Length;
                    _masterGpioController.Write(_rowPins[_currentRow], PinValue.High);

                    Thread.Sleep(ScanInterval);
                }
                ClosePins();

                return args;
            }, token);
        }

        /// <summary>
        /// Get buttons' values by row
        /// </summary>
        /// <param name="row">Row index</param>
        public ReadOnlySpan<PinValue> RowValues(int row)
        {
            return _buttonValues.AsSpan(row * _rowPins.Length, _columnPins.Length);
        }

        /// <summary>
        /// Keyboard event
        /// </summary>
        public event PinChangeEventHandler PinChangeEvent;

        public void Dispose()
        {
            _masterGpioController.Dispose();
            _masterGpioController = null;

            _rowPins = null;
            _columnPins = null;
            _buttonValues = null;
        }

        private void OpenPins()
        {
            for (int i = 0; i < _rowPins.Length; i++)
            {
                _masterGpioController.OpenPin(_rowPins[i], PinMode.Output);
            }
            for (int i = 0; i < _columnPins.Length; i++)
            {
                _masterGpioController.OpenPin(_columnPins[i], PinMode.Input);
            }
        }
        private void ClosePins()
        {
            Thread.Sleep(ScanInterval); // wait for thread ends    

            for (int i = 0; i < _rowPins.Length; i++)
            {
                _masterGpioController.ClosePin(_rowPins[i]);
            }
            for (int i = 0; i < _columnPins.Length; i++)
            {
                _masterGpioController.ClosePin(_columnPins[i]);
            }
        }

    }
}

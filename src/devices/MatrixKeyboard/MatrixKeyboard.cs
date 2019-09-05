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
        /// Get output pins
        /// </summary>
        public IEnumerable<int> OutputPins => _outputPins;

        /// <summary>
        /// Get input pins
        /// </summary>
        public IEnumerable<int> InputPins => _inputPins;

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

        private int _currentOutput;
        private int[] _outputPins;
        private int[] _inputPins;
        private GpioController _masterGpioController;
        private PinValue[] _buttonValues;

        /// <summary>
        /// Initialize keyboard
        /// </summary>
        /// <param name="masterController">GPIO controller</param>
        /// <param name="outputPins">Output pins</param>
        /// <param name="colPins">Input pins</param>
        /// <param name="scanInterval">Scanning interval in milliseconds</param>
        public MatrixKeyboard(GpioController masterController, IEnumerable<int> outputPins, IEnumerable<int> colPins, int scanInterval)
        {
            _masterGpioController = masterController ?? throw new ArgumentNullException(nameof(masterController));

            if (outputPins == null)
            {
                throw new ArgumentNullException(nameof(outputPins));
            }
            if (outputPins.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(outputPins), "The number of outputs is at least 1");
            }

            if (colPins == null)
            {
                throw new ArgumentNullException(nameof(colPins));
            }
            if (colPins.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(colPins), "The number of inputs is at least 1");
            }

            if (scanInterval <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scanInterval), "Scanning interval must be positive");
            }

            _outputPins = outputPins.ToArray();
            _inputPins = colPins.ToArray();
            _buttonValues = new PinValue[_outputPins.Length * _inputPins.Length];

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
                    for (int i = 0; i < _inputPins.Length; i++)
                    {
                        int index = _currentOutput * _inputPins.Length + i;

                        PinValue oldValue = _buttonValues[index];
                        PinValue newValue = _masterGpioController.Read(_inputPins[i]);

                        _buttonValues[index] = newValue;
                        if (newValue != oldValue)
                        {
                            MatrixKeyboardEventArgs args = new MatrixKeyboardEventArgs(newValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling, _currentOutput, i);
                            PinChangeEvent(this, args);
                        }
                    }

                    _masterGpioController.Write(_outputPins[_currentOutput], PinValue.Low);
                    _currentOutput = (_currentOutput + 1) % _outputPins.Length;
                    _masterGpioController.Write(_outputPins[_currentOutput], PinValue.High);

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
                    for (int i = 0; i < _inputPins.Length; i++)
                    {
                        int index = _currentOutput * _inputPins.Length + i;

                        PinValue oldValue = _buttonValues[index];
                        PinValue newValue = _masterGpioController.Read(_inputPins[i]);

                        _buttonValues[index] = newValue;
                        if (newValue != oldValue)
                        {
                            args = new MatrixKeyboardEventArgs(newValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling, _currentOutput, i);
                        }
                    }

                    _masterGpioController.Write(_outputPins[_currentOutput], PinValue.Low);
                    _currentOutput = (_currentOutput + 1) % _outputPins.Length;
                    _masterGpioController.Write(_outputPins[_currentOutput], PinValue.High);

                    Thread.Sleep(ScanInterval);
                }
                ClosePins();

                return args;
            }, token);
        }

        /// <summary>
        /// Get buttons' values by output
        /// </summary>
        /// <param name="output">Output index</param>
        public ReadOnlySpan<PinValue> ValuesByOutput(int output)
        {
            return _buttonValues.AsSpan(output * _outputPins.Length, _inputPins.Length);
        }

        /// <summary>
        /// Keyboard event
        /// </summary>
        public event PinChangeEventHandler PinChangeEvent;
        
        /// <inheritdoc/>
        public void Dispose()
        {
            _masterGpioController.Dispose();
            _masterGpioController = null;

            _outputPins = null;
            _inputPins = null;
            _buttonValues = null;
        }

        private void OpenPins()
        {
            for (int i = 0; i < _outputPins.Length; i++)
            {
                _masterGpioController.OpenPin(_outputPins[i], PinMode.Output);
            }
            for (int i = 0; i < _inputPins.Length; i++)
            {
                _masterGpioController.OpenPin(_inputPins[i], PinMode.Input);
            }
        }
        private void ClosePins()
        {
            Thread.Sleep(ScanInterval); // wait for thread ends    

            for (int i = 0; i < _outputPins.Length; i++)
            {
                _masterGpioController.ClosePin(_outputPins[i]);
            }
            for (int i = 0; i < _inputPins.Length; i++)
            {
                _masterGpioController.ClosePin(_inputPins[i]);
            }
        }
    }
}

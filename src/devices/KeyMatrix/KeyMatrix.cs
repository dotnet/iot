// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.KeyMatrix
{
    /// <summary>
    /// GPIO key matrix Driver
    /// </summary>
    public class KeyMatrix : IDisposable
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
        /// Get buttons' values by output
        /// </summary>
        /// <param name="output">Output index</param>
        public ReadOnlySpan<PinValue> this[int output] => _buttonValues.AsSpan(output * _outputPins.Length, _inputPins.Length);

        private int[] _outputPins;
        private int[] _inputPins;
        private GpioController _gpioController;
        private PinValue[] _buttonValues;
        private bool _pinsOpened;
        private int _currentOutput = 0;

        /// <summary>
        /// Initialize key matrix
        /// </summary>
        /// <param name="masterController">GPIO controller</param>
        /// <param name="outputPins">Output pins</param>
        /// <param name="inputPins">Input pins</param>
        /// <param name="scanInterval">Scanning interval in milliseconds</param>
        public KeyMatrix(GpioController masterController, IEnumerable<int> outputPins, IEnumerable<int> inputPins, int scanInterval)
        {
            _gpioController = masterController ?? throw new ArgumentNullException(nameof(masterController));

            if (outputPins == null)
            {
                throw new ArgumentNullException(nameof(outputPins));
            }

            if (!outputPins.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(outputPins), "The number of outputs must be at least 1");
            }

            if (inputPins == null)
            {
                throw new ArgumentNullException(nameof(inputPins));
            }

            if (!inputPins.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(inputPins), "The number of inputs must be at least 1");
            }

            if (scanInterval < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scanInterval), "Scanning interval must be 0 or positive");
            }

            _outputPins = outputPins.ToArray();
            _inputPins = inputPins.ToArray();
            _buttonValues = new PinValue[_outputPins.Length * _inputPins.Length];
            _pinsOpened = false;

            ScanInterval = scanInterval;

            OpenPins();
        }

        /// <summary>
        /// Blocks execution until a key event is received
        /// </summary>
        public KeyMatrixEvent ReadKey()
        {
            KeyMatrixEvent args = null;
            _currentOutput--;

            do
            {
                Thread.Sleep(ScanInterval);

                _currentOutput = (_currentOutput + 1) % _outputPins.Length;
                _gpioController.Write(_outputPins[_currentOutput], PinValue.High);

                for (var i = 0; i < _inputPins.Length; i++)
                {
                    int index = _currentOutput * _inputPins.Length + i;

                    PinValue oldValue = _buttonValues[index];
                    PinValue newValue = _gpioController.Read(_inputPins[i]);
                    _buttonValues[index] = newValue;

                    if (newValue != oldValue)
                    {
                        args = new KeyMatrixEvent(newValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling, _currentOutput, i);
                        return args;
                    }
                }

                _gpioController.Write(_outputPins[_currentOutput], PinValue.Low);
            }
            while (_pinsOpened);

            return args;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ClosePins();

            _gpioController?.Dispose();
            _gpioController = null;

            _outputPins = null;
            _inputPins = null;
            _buttonValues = null;
        }

        private void OpenPins()
        {
            for (int i = 0; i < _outputPins.Length; i++)
            {
                _gpioController.OpenPin(_outputPins[i], PinMode.Output);
            }

            for (int i = 0; i < _inputPins.Length; i++)
            {
                _gpioController.OpenPin(_inputPins[i], PinMode.Input);
            }

            _pinsOpened = true;
        }

        private void ClosePins()
        {
            _pinsOpened = false;

            for (int i = 0; i < _outputPins.Length; i++)
            {
                _gpioController.ClosePin(_outputPins[i]);
            }

            for (int i = 0; i < _inputPins.Length; i++)
            {
                _gpioController.ClosePin(_inputPins[i]);
            }
        }
    }
}

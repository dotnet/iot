// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        public TimeSpan ScanInterval { get; set; }

        /// <summary>
        /// Get buttons' values by output
        /// </summary>
        /// <param name="output">Output index</param>
        public ReadOnlySpan<PinValue> this[int output] => _buttonValues.AsSpan(output * _outputPins.Length, _inputPins.Length);

        private int[] _outputPins;
        private int[] _inputPins;
        private GpioController? _gpioController;
        private PinValue[] _buttonValues;
        private bool _pinsOpened;
        private int _currentOutput = 0;
        private bool _shouldDispose;
        private bool _isRunning = false;
        private KeyMatrixEvent? _lastKeyEvent;

        /// <summary>
        /// Fire an event when a key is pressed or released
        /// </summary>
        /// <param name="sender">The sender KeyMatrix</param>
        /// <param name="keyMatrixEvent">The key event</param>
        public delegate void KeyEventHandler(object sender, KeyMatrixEvent keyMatrixEvent);

        /// <summary>
        /// The raised event
        /// </summary>
        public event KeyEventHandler? KeyEvent;

        /// <summary>
        /// Initialize key matrix
        /// </summary>
        /// <param name="outputPins">Output pins</param>
        /// <param name="inputPins">Input pins</param>
        /// <param name="scanInterval">Scanning interval in milliseconds</param>
        /// <param name="gpioController">GPIO controller</param>
        /// <param name="shouldDispose">True to dispose the GpioController</param>
        public KeyMatrix(IEnumerable<int> outputPins, IEnumerable<int> inputPins, TimeSpan scanInterval, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _shouldDispose = shouldDispose || gpioController == null;
            _gpioController = gpioController ?? new();

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

            _outputPins = outputPins.ToArray();
            _inputPins = inputPins.ToArray();
            _buttonValues = new PinValue[_outputPins.Length * _inputPins.Length];
            for (int i = 0; i < _buttonValues.Length; i++)
            {
                _buttonValues[i] = PinValue.Low;
            }

            _pinsOpened = false;
            ScanInterval = scanInterval;
            OpenPins();
        }

        /// <summary>
        /// Start listening to key events
        /// </summary>
        public void StartListeningKeyEvent()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            new Thread(() =>
            {
                LoopReadKey();
            }).Start();
        }

        /// <summary>
        /// Stop listening to key events
        /// </summary>
        public void StopListeningKeyEvent()
        {
            _isRunning = false;
        }

        private void LoopReadKey()
        {
            do
            {
                Thread.Sleep(ScanInterval);

                _currentOutput = (_currentOutput + 1) % _outputPins.Length;
                _gpioController!.Write(_outputPins[_currentOutput], PinValue.High);

                for (var i = 0; i < _inputPins.Length; i++)
                {
                    int index = _currentOutput * _inputPins.Length + i;

                    PinValue oldValue = _buttonValues[index];
                    PinValue newValue = _gpioController.Read(_inputPins[i]);
                    _buttonValues[index] = newValue;

                    if (newValue != oldValue)
                    {
                        KeyEvent?.Invoke(this, new KeyMatrixEvent(newValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling, _currentOutput, i));
                    }
                }

                _gpioController.Write(_outputPins[_currentOutput], PinValue.Low);
            }
            while (_pinsOpened && _isRunning);
        }

        /// <summary>
        /// Blocks execution until a key event is received
        /// </summary>
        public KeyMatrixEvent? ReadKey()
        {
            _currentOutput--;
            KeyEvent += KeyMatrixKeyEvent;
            _isRunning = true;
            LoopReadKey();
            KeyEvent -= KeyMatrixKeyEvent;
            return _lastKeyEvent;
        }

        private void KeyMatrixKeyEvent(object sender, KeyMatrixEvent keyMatrixEvent)
        {
            _isRunning = false;
            _lastKeyEvent = keyMatrixEvent;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ClosePins();

            if (_shouldDispose)
            {
                _gpioController?.Dispose();
                _gpioController = null;
            }
            else
            {
                if (_gpioController is object && _pinsOpened)
                {
                    ClosePins();
                }
            }
        }

        private void OpenPins()
        {
            for (int i = 0; i < _outputPins.Length; i++)
            {
                _gpioController!.OpenPin(_outputPins[i], PinMode.Output);
            }

            for (int i = 0; i < _inputPins.Length; i++)
            {
                _gpioController!.OpenPin(_inputPins[i], PinMode.Input);
            }

            _pinsOpened = true;
        }

        private void ClosePins()
        {
            _isRunning = false;
            _pinsOpened = false;
            Thread.Sleep(ScanInterval); // wait for current scan to complete

            for (int i = 0; i < _outputPins.Length; i++)
            {
                _gpioController!.ClosePin(_outputPins[i]);
            }

            for (int i = 0; i < _inputPins.Length; i++)
            {
                _gpioController!.ClosePin(_inputPins[i]);
            }
        }
    }
}

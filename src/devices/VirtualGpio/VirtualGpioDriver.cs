// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;

namespace Iot.Device.VirtualGpio
{
    internal class VirtualGpioDriver : GpioDriver
    {
        internal PinValue this[int pinNumber] => _pinValues[pinNumber];

        protected override int PinCount { get; }

        internal event PinChangeEventHandler? InputPinValueChanged;

        internal event PinChangeEventHandler? OutputPinValueChanged;

        private readonly bool[] _openStatus;
        private readonly PinMode[] _pinModes;
        private readonly PinValue[] _pinValues;

        internal VirtualGpioDriver(int pinCount)
        {
            PinCount = pinCount;

            _openStatus = new bool[PinCount];
            _pinModes = new PinMode[PinCount];
            _pinValues = new PinValue[PinCount];
        }

        internal void Input(int pinNumber, PinValue? value)
        {
            if (_pinModes[pinNumber] == PinMode.Output && _pinValues[pinNumber] != value)
            {
                throw new SystemException("The pin is shorted.");
            }

            PinValue lastPinValue = _pinValues[pinNumber];

            PinValue actualValue = value ?? (_pinModes[pinNumber] == PinMode.InputPullUp ? PinValue.High : _pinModes[pinNumber] == PinMode.InputPullDown ? PinValue.Low : _pinValues[pinNumber]);
            if (actualValue != lastPinValue)
            {
                _pinValues[pinNumber] = actualValue;
                InputPinValueChanged?.Invoke(this, new PinValueChangedEventArgs(value == PinValue.Low ? PinEventTypes.Falling : PinEventTypes.Rising, pinNumber));
            }
        }

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            InputPinValueChanged += callback;
        }

        protected override void ClosePin(int pinNumber)
        {
            if (_openStatus[pinNumber])
            {
                _openStatus[pinNumber] = false;
            }
            else
            {
                throw new InvalidOperationException("Cannot close pin vale while it is closed.");
            }
        }

        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        protected override PinMode GetPinMode(int pinNumber)
        {
            return _pinModes[pinNumber];
        }

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return true;
        }

        protected override void OpenPin(int pinNumber)
        {
            if (_openStatus[pinNumber])
            {
                throw new InvalidOperationException("Cannot open pin vale while it is opened.");
            }
            else
            {
                _openStatus[pinNumber] = true;
            }
        }

        protected override PinValue Read(int pinNumber)
        {
            if (_openStatus[pinNumber])
            {
                return _pinValues[pinNumber];
            }
            else
            {
                throw new InvalidOperationException("Cannot read pin vale while the pin is closed.");
            }
        }

        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            InputPinValueChanged -= callback;
        }

        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            _pinModes[pinNumber] = mode;
        }

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            PinValue lastPinValue = _pinValues[pinNumber];
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return new WaitForEventResult
                    {
                        EventTypes = PinEventTypes.None,
                        TimedOut = true
                    };
                }
                else if (_pinValues[pinNumber] != lastPinValue)
                {
                    return new WaitForEventResult
                    {
                        EventTypes = lastPinValue == PinValue.Low ? PinEventTypes.Rising : PinEventTypes.Falling,
                        TimedOut = true
                    };
                }

                Thread.Sleep(0);
            }
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            if (_pinModes[pinNumber] == PinMode.Output)
            {
                PinValue lastPinValue = _pinValues[pinNumber];
                _pinValues[pinNumber] = value;

                if (value != lastPinValue)
                {
                    OutputPinValueChanged?.Invoke(this, new PinValueChangedEventArgs(lastPinValue == PinValue.Low ? PinEventTypes.Rising : PinEventTypes.Falling, pinNumber));
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot write pin value while the pin is not in output mode.");
            }
        }
    }
}

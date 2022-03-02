// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;

namespace Iot.Device.VirtualGpio
{
    /// <summary>
    /// Program-control-input GPIO driver. For simulation, testing, etc.
    /// </summary>
    public class VirtualGpioDriver : GpioDriver
    {
        /// <summary>
        /// Triggered when a input pin value changes.
        /// </summary>
        public event PinChangeEventHandler? InputPinValueChanged;

        /// <summary>
        /// Triggered when a output pin value changes.
        /// </summary>
        public event PinChangeEventHandler? OutputPinValueChanged;

        /// <summary>
        /// Get pin value by pin number.
        /// </summary>
        /// <param name="pinNumber">Pin number</param>
        /// <returns>Pin value</returns>
        public PinValue this[int pinNumber] => pinValues[pinNumber];

        /// <summary>
        /// Number of pins.
        /// </summary>
        protected override int PinCount { get; }

        private readonly bool[] openStatus;
        private readonly PinMode[] pinModes;
        private readonly PinValue[] pinValues;

        /// <summary>
        /// Initialize a virtual GPIO driver with a specific number of pins.
        /// </summary>
        /// <param name="pinCount">Number of pins</param>
        public VirtualGpioDriver(int pinCount)
        {
            PinCount = pinCount;

            openStatus = new bool[PinCount];
            pinModes = new PinMode[PinCount];
            pinValues = new PinValue[PinCount];
        }

        /// <summary>
        /// Simulates input value for a pin.
        /// </summary>
        /// <param name="pinNumber">Pin number that accepts input</param>
        /// <param name="value">Input value</param>
        /// <exception cref="SystemException">Throws when the pin is in output mode and try to set a different pin value.</exception>
        public void Input(int pinNumber, PinValue? value)
        {
            if (pinModes[pinNumber] == PinMode.Output && pinValues[pinNumber] != value)
            {
                throw new SystemException("The pin is shorted.");
            }

            var lastPinValue = pinValues[pinNumber];

            var actualValue = value ?? (pinModes[pinNumber] == PinMode.InputPullUp ? PinValue.High : pinModes[pinNumber] == PinMode.InputPullDown ? PinValue.Low : pinValues[pinNumber]);
            if (actualValue != lastPinValue)
            {
                pinValues[pinNumber] = actualValue;
                InputPinValueChanged?.Invoke(this, new PinValueChangedEventArgs(value == PinValue.Low ? PinEventTypes.Falling : PinEventTypes.Rising, pinNumber));
            }
        }

        /// <inheritdoc/>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            InputPinValueChanged += callback;
        }

        /// <inheritdoc/>
        protected override void ClosePin(int pinNumber)
        {
            if (openStatus[pinNumber])
            {
                openStatus[pinNumber] = false;
            }
            else
            {
                throw new InvalidOperationException("Cannot close pin vale while it is closed.");
            }
        }

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        /// <inheritdoc/>
        protected override PinMode GetPinMode(int pinNumber)
        {
            return pinModes[pinNumber];
        }

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return true;
        }

        /// <inheritdoc/>
        protected override void OpenPin(int pinNumber)
        {
            if (openStatus[pinNumber])
            {
                throw new InvalidOperationException("Cannot open pin vale while it is opened.");
            }
            else
            {
                openStatus[pinNumber] = true;
            }
        }

        /// <inheritdoc/>
        protected override PinValue Read(int pinNumber)
        {
            if (openStatus[pinNumber])
            {
                return pinValues[pinNumber];
            }
            else
            {
                throw new InvalidOperationException("Cannot read pin vale while the pin is closed.");
            }
        }

        /// <inheritdoc/>
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            InputPinValueChanged -= callback;
        }

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            pinModes[pinNumber] = mode;
        }

        /// <inheritdoc/>
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            var lastPinValue = pinValues[pinNumber];
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
                else if (pinValues[pinNumber] != lastPinValue)
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

        /// <inheritdoc/>
        protected override void Write(int pinNumber, PinValue value)
        {
            if (pinModes[pinNumber] == PinMode.Output)
            {
                var lastPinValue = pinValues[pinNumber];
                pinValues[pinNumber] = value;

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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.Button
{
    /// <summary>
    /// GPIO implementation of Button.
    /// Inherits from ButtonBase.
    /// </summary>
    public class GpioButton : ButtonBase
    {
        private GpioController _gpioController;
        private PinMode _gpioPinMode;
        private PinMode _eventPinMode;
        private TimeSpan _debounceTime;

        private int _buttonPin;
        private bool _shouldDispose;

        private bool _disposed = false;

        /// <summary>
        /// Specify whether the Gpio associated with the button has an external resistor acting as pull-up or pull-down.
        /// </summary>
        public bool IsExternalResistor { get; private set; } = false;

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="buttonPin">GPIO pin of the button.</param>
        /// <param name="pinMode">Pin mode of the system. If the Gpio is an input and an external resistor is used, it must be specified PinMode.InputPullUp or PinMode.InputPullDown so that the correct transition can be detected by the event handler. </param>
        /// <param name="gpio">Gpio Controller.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        /// <param name="debounceTime">The amount of time during which the transitions are ignored, or zero</param>
        /// <param name="isExternalResistor">This parameter is ignored when PinMode is PinMode.Input. When False, if PinMode is PinMode.InputPullUp or PinMode.InputPullDown, the Gpio is configured with the internal resistor (if supported by the board). Otherwise the Gpio is configured as it was specified PinMode.Input.</param>
        public GpioButton(int buttonPin, GpioController? gpio = null, bool shouldDispose = true, PinMode pinMode = PinMode.InputPullUp,
            TimeSpan debounceTime = default(TimeSpan), bool isExternalResistor = false)
            : this(buttonPin, TimeSpan.FromTicks(DefaultDoublePressTicks), TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds), gpio, shouldDispose, pinMode, debounceTime, isExternalResistor)
        {
        }

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="buttonPin">GPIO pin of the button.</param>
        /// <param name="pinMode">Pin mode of the system. If the Gpio is an input and an external resistor is used, it must be specified PinMode.InputPullUp or PinMode.InputPullDown so that the correct transition can be detected by the event handler.</param>
        /// <param name="doublePress">Max ticks between button presses to count as doublepress.</param>
        /// <param name="holding">Min ms a button is pressed to count as holding.</param>
        /// <param name="gpio">Gpio Controller.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        /// <param name="debounceTime">The amount of time during which the transitions are ignored, or zero</param>
        /// <param name="isExternalResistor">This parameter is ignored when PinMode is PinMode.Input. When False, if PinMode is PinMode.InputPullUp or PinMode.InputPullDown, the Gpio is configured with the internal resistor (if supported by the board). Otherwise the Gpio is configured as it was specified PinMode.Input</param>
        public GpioButton(int buttonPin, TimeSpan doublePress, TimeSpan holding, GpioController? gpio = null, bool shouldDispose = true, PinMode pinMode = PinMode.InputPullUp, TimeSpan debounceTime = default(TimeSpan), bool isExternalResistor = false)
            : base(doublePress, holding, debounceTime)
        {
            _gpioController = gpio ?? new GpioController();
            _shouldDispose = shouldDispose;
            _buttonPin = buttonPin;
            _debounceTime = debounceTime;
            IsExternalResistor = isExternalResistor;

            _eventPinMode = pinMode;
            _gpioPinMode = isExternalResistor && (pinMode == PinMode.InputPullUp || pinMode == PinMode.InputPullDown)
                ? _gpioPinMode = PinMode.Input
                : _gpioPinMode = pinMode;

            if (_gpioPinMode == PinMode.Input | _gpioPinMode == PinMode.InputPullDown | _gpioPinMode == PinMode.InputPullUp)
            {
                _gpioController.OpenPin(_buttonPin, _gpioPinMode);
                _gpioController.RegisterCallbackForPinValueChangedEvent(_buttonPin, PinEventTypes.Falling | PinEventTypes.Rising, PinStateChanged);
            }
            else
            {
                throw new ArgumentException("GPIO pin can only be set to input, not to output.");
            }
        }

        /// <summary>
        /// Handles changes in GPIO pin, based on whether the system is pullup or pulldown.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="pinValueChangedEventArgs">The pin argument changes.</param>
        private void PinStateChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            switch (pinValueChangedEventArgs.ChangeType)
            {
                case PinEventTypes.Falling:
                    if (_eventPinMode == PinMode.InputPullUp)
                    {
                        HandleButtonPressed();
                    }
                    else
                    {
                        HandleButtonReleased();
                    }

                    break;
                case PinEventTypes.Rising:
                    if (_eventPinMode == PinMode.InputPullUp)
                    {
                        HandleButtonReleased();
                    }
                    else
                    {
                        HandleButtonPressed();
                    }

                    break;
            }
        }

        /// <summary>
        /// Internal cleanup.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _gpioController.UnregisterCallbackForPinValueChangedEvent(_buttonPin, PinStateChanged);
                if (_shouldDispose)
                {
                    _gpioController?.Dispose();
                    _gpioController = null!;
                }
                else
                {
                    _gpioController.ClosePin(_buttonPin);
                }
            }

            base.Dispose(disposing);
            _disposed = true;
        }
    }
}

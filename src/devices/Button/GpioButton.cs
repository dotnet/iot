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
        private PinMode _pinMode;
        private TimeSpan _debounceTime;

        private int _buttonPin;
        private bool _shouldDispose;

        private bool _disposed = false;

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="buttonPin">GPIO pin of the button.</param>
        /// <param name="pinMode">Pin mode of the system.</param>
        /// <param name="gpio">Gpio Controller.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        /// <param name="debounceTime">The amount of time during which the transitions are ignored, or zero</param>
        public GpioButton(int buttonPin, GpioController? gpio = null, bool shouldDispose = true, PinMode pinMode = PinMode.InputPullUp,
            TimeSpan debounceTime = default(TimeSpan))
            : this(buttonPin, TimeSpan.FromTicks(DefaultDoublePressTicks), TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds), gpio, shouldDispose, pinMode, debounceTime)
        {
        }

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="buttonPin">GPIO pin of the button.</param>
        /// <param name="pinMode">Pin mode of the system.</param>
        /// <param name="doublePress">Max ticks between button presses to count as doublepress.</param>
        /// <param name="holding">Min ms a button is pressed to count as holding.</param>
        /// <param name="gpio">Gpio Controller.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        /// <param name="debounceTime">The amount of time during which the transitions are ignored, or zero</param>
        public GpioButton(int buttonPin, TimeSpan doublePress, TimeSpan holding, GpioController? gpio = null, bool shouldDispose = true, PinMode pinMode = PinMode.InputPullUp, TimeSpan debounceTime = default(TimeSpan))
            : base(doublePress, holding, debounceTime)
        {
            _gpioController = gpio ?? new GpioController();
            _shouldDispose = shouldDispose;
            _buttonPin = buttonPin;
            _pinMode = pinMode;
            _debounceTime = debounceTime;

            if (_pinMode == PinMode.Input | _pinMode == PinMode.InputPullDown | _pinMode == PinMode.InputPullUp)
            {
                _gpioController.OpenPin(_buttonPin, _pinMode);
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
                    if (_pinMode == PinMode.InputPullUp)
                    {
                        HandleButtonPressed();
                    }
                    else
                    {
                        HandleButtonReleased();
                    }

                    break;
                case PinEventTypes.Rising:
                    if (_pinMode == PinMode.InputPullUp)
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

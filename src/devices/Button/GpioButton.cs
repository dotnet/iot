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

        private int _buttonPin;
        private bool _shouldDispose;

        private bool _disposed = false;

        /// <summary>
        /// Specify whether the Gpio associated with the button has an external resistor acting as pull-up or pull-down.
        /// </summary>
        public bool HasExternalResistor { get; private set; } = false;

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="buttonPin">GPIO pin of the button.</param>
        /// <param name="isPullUp">True if the Gpio is either pulled up in hardware or in the Gpio configuration (see <paramref name="hasExternalResistor"/>. False if instead the Gpio is pulled down.</param>
        /// <param name="hasExternalResistor">When False the pull resistor is configured using the Gpio PinMode.InputPullUp or PinMode.InputPullDown (if supported by the board). Otherwise the Gpio is configured as PinMode.Input.</param>
        /// <param name="gpio">Gpio Controller.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        /// <param name="debounceTime">The amount of time during which the transitions are ignored, or zero</param>
        public GpioButton(int buttonPin, bool isPullUp = true, bool hasExternalResistor = false,
            GpioController? gpio = null, bool shouldDispose = true, TimeSpan debounceTime = default)
            : this(buttonPin, TimeSpan.FromTicks(DefaultDoublePressTicks), TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds), isPullUp, hasExternalResistor, gpio, shouldDispose, debounceTime)
        {
        }

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="buttonPin">GPIO pin of the button.</param>
        /// <param name="doublePress">Max ticks between button presses to count as doublepress.</param>
        /// <param name="holding">Min ms a button is pressed to count as holding.</param>
        /// <param name="isPullUp">True if the Gpio is either pulled up in hardware or in the Gpio configuration (see <paramref name="hasExternalResistor"/>. False if instead the Gpio is pulled down.</param>
        /// <param name="hasExternalResistor">When False the pull resistor is configured using the Gpio PinMode.InputPullUp or PinMode.InputPullDown (if supported by the board). Otherwise the Gpio is configured as PinMode.Input.</param>
        /// <param name="gpio">Gpio Controller.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        /// <param name="debounceTime">The amount of time during which the transitions are ignored, or zero</param>
        public GpioButton(int buttonPin,
            TimeSpan doublePress,
            TimeSpan holding,
            bool isPullUp = true,
            bool hasExternalResistor = false,
            GpioController? gpio = null,
            bool shouldDispose = true,
            TimeSpan debounceTime = default)
            : base(doublePress, holding, debounceTime)
        {
            _gpioController = gpio ?? new GpioController();
            _shouldDispose = gpio == null ? true : shouldDispose;
            _buttonPin = buttonPin;
            HasExternalResistor = hasExternalResistor;

            _eventPinMode = isPullUp ? PinMode.InputPullUp : PinMode.InputPullDown;
            _gpioPinMode = hasExternalResistor
                ? _gpioPinMode = PinMode.Input
                : _gpioPinMode = _eventPinMode;

            if (!_gpioController.IsPinModeSupported(_buttonPin, _gpioPinMode))
            {
                if (_gpioPinMode == PinMode.Input)
                {
                    throw new ArgumentException($"The pin {_buttonPin} cannot be configured as Input");
                }

                throw new ArgumentException($"The pin {_buttonPin} cannot be configured as {(isPullUp ? "pull-up" : "pull-down")}. Use an external resistor and set {nameof(HasExternalResistor)}=true");
            }

            try
            {
                _gpioController.OpenPin(_buttonPin, _gpioPinMode);
                _gpioController.RegisterCallbackForPinValueChangedEvent(
                    _buttonPin,
                    PinEventTypes.Falling | PinEventTypes.Rising,
                    PinStateChanged);
            }
            catch (Exception)
            {
                if (shouldDispose)
                {
                    _gpioController.Dispose();
                }

                throw;
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

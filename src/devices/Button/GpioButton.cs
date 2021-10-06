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
        private int _buttonPin;
        private bool _pullUp;
        private bool _shouldDispose;

        private bool _disposed = false;

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="buttonPin">GPIO pin of the button.</param>
        /// <param name="pullUp">If the system is pullup (false = pulldown).</param>
        /// <param name="gpio">Gpio Controller.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        public GpioButton(int buttonPin, bool pullUp = true, GpioController? gpio = null, bool shouldDispose = true)
            : this(buttonPin, pullUp, TimeSpan.FromTicks(DefaultDoublePressTicks), TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds), gpio, shouldDispose)
        {
        }

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="buttonPin">GPIO pin of the button.</param>
        /// <param name="pullUp">If the system is pullup (false = pulldown).</param>
        /// <param name="doublePress">Max ticks between button presses to count as doublepress.</param>
        /// <param name="holding">Min ms a button is pressed to count as holding.</param>
        /// <param name="gpio">Gpio Controller.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        public GpioButton(int buttonPin, bool pullUp, TimeSpan doublePress, TimeSpan holding, GpioController? gpio = null, bool shouldDispose = true)
            : base(doublePress, holding)
        {
            _gpioController = gpio ?? new GpioController();
            _shouldDispose = shouldDispose;
            _buttonPin = buttonPin;
            _pullUp = pullUp;

            _gpioController.OpenPin(_buttonPin, PinMode.Input);
            _gpioController.RegisterCallbackForPinValueChangedEvent(_buttonPin, PinEventTypes.Falling | PinEventTypes.Rising, PinStateChanged);
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
                    if (_pullUp)
                    {
                        HandleButtonPressed();
                    }
                    else
                    {
                        HandleButtonReleased();
                    }

                    break;
                case PinEventTypes.Rising:
                    if (_pullUp)
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
        /// Cleanup.
        /// </summary>
        public new void Dispose()
        {
            Dispose(true);
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
                if (_shouldDispose)
                {
                    _gpioController?.Dispose();
                    _gpioController = null!;
                }
                else
                {
                    _gpioController.UnregisterCallbackForPinValueChangedEvent(_buttonPin, PinStateChanged);
                    _gpioController.ClosePin(_buttonPin);
                }

                base.Dispose(disposing);
                _disposed = true;
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WinGpio = global::Windows.Devices.Gpio;

namespace System.Device.Gpio.Drivers
{
    internal class Windows10DriverPin : IDisposable
    {
        private const int ReasonableDebounceTimeoutMillseconds = 50;
        private WeakReference<Windows10Driver> _driver;
        private WinGpio.GpioPin _pin;
        private readonly int _pinNumber;
        private PinChangeEventHandler _risingCallbacks;
        private PinChangeEventHandler _fallingCallbacks;

        public Windows10DriverPin(Windows10Driver driver, WinGpio.GpioPin pin)
        {
            _driver = new WeakReference<Windows10Driver>(driver);
            _pin = pin;
            _pinNumber = _pin.PinNumber;

            // Set a reasonable default for DebounceTimeout (until .NET Core API adds a DebouceTimeout property)
            _pin.DebounceTimeout = TimeSpan.FromMilliseconds(ReasonableDebounceTimeoutMillseconds);
        }

        ~Windows10DriverPin()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_pin != null)
            {
                _pin.ValueChanged -= Pin_ValueChanged;
                _pin.Dispose();
                _pin = null;
            }
            _driver = null;
            _fallingCallbacks = null;
            _risingCallbacks = null;
            GC.SuppressFinalize(this);
        }

        public void AddCallbackForPinValueChangedEvent(PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            if (eventTypes == PinEventTypes.None)
            {
                throw new ArgumentException($"{PinEventTypes.None} is an invalid value.", nameof(eventTypes));
            }

            bool isFirstCallback = _risingCallbacks == null && _fallingCallbacks == null;

            if (eventTypes.HasFlag(PinEventTypes.Rising))
            {
                _risingCallbacks += callback;
            }

            if (eventTypes.HasFlag(PinEventTypes.Falling))
            {
                _fallingCallbacks += callback;
            }

            if (isFirstCallback)
            {
                _pin.ValueChanged += this.Pin_ValueChanged;
            }
        }

        private void Pin_ValueChanged(WinGpio.GpioPin sender, WinGpio.GpioPinValueChangedEventArgs args)
        {
            if (!_driver.TryGetTarget(out Windows10Driver driver))
            {
                return;
            }

            switch (args.Edge)
            {
                case WinGpio.GpioPinEdge.FallingEdge:
                    _fallingCallbacks?.Invoke(_driver, new PinValueChangedEventArgs(PinEventTypes.Falling, _pinNumber));
                    break;

                case WinGpio.GpioPinEdge.RisingEdge:
                    _risingCallbacks?.Invoke(_driver, new PinValueChangedEventArgs(PinEventTypes.Rising, _pinNumber));
                    break;
            }
        }

        public void ClosePin() => Dispose();

        public bool IsPinModeSupported(PinMode mode) => _pin.IsDriveModeSupported(PinModeToGpioDriveMode(mode));

        public PinValue Read() => GpioPinValueToPinValue(_pin.Read());

        public void RemoveCallbackForPinValueChangedEvent(PinChangeEventHandler callback)
        {
            _fallingCallbacks -= callback;
            _risingCallbacks -= callback;

            if (_fallingCallbacks == null && _risingCallbacks == null)
            {
                _pin.ValueChanged -= Pin_ValueChanged;
            }
        }

        public void SetPinMode(PinMode mode) => _pin.SetDriveMode(PinModeToGpioDriveMode(mode));

        public PinMode GetPinMode() => GpioDriveModeToPinMode(_pin.GetDriveMode());

        public WaitForEventResult WaitForEvent(PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            using (ManualResetEvent completionEvent = new ManualResetEvent(false))
            {
                PinEventTypes pinEventTypes = PinEventTypes.None;
                void handler(WinGpio.GpioPin s, WinGpio.GpioPinValueChangedEventArgs a)
                {
                    pinEventTypes = GpioEdgeToPinEventType(a.Edge);

                    if ((pinEventTypes & eventTypes) != 0)
                    {
                        completionEvent.Set();
                    }
                }

                WaitHandle[] waitHandles =
                {
                    completionEvent,
                    cancellationToken.WaitHandle
                };

                _pin.ValueChanged += handler;
                bool eventOccurred = 0 == WaitHandle.WaitAny(waitHandles);
                _pin.ValueChanged -= handler;

                return new WaitForEventResult
                {
                    EventTypes = pinEventTypes,
                    TimedOut = !eventOccurred
                };
            }
        }

        public void Write(PinValue value) => _pin.Write(PinValueToGpioPinValue(value));

        #region Enumeration conversion methods

        private static WinGpio.GpioPinDriveMode PinModeToGpioDriveMode(PinMode mode)
        {
            return mode switch
            {
                PinMode.Input => WinGpio.GpioPinDriveMode.Input,
                PinMode.Output => WinGpio.GpioPinDriveMode.Output,
                PinMode.InputPullDown => WinGpio.GpioPinDriveMode.InputPullDown,
                PinMode.InputPullUp => WinGpio.GpioPinDriveMode.InputPullUp,
                _ => throw new ArgumentException($"GPIO pin mode {mode} not supported.", nameof(mode))
            };
        }

        private static PinMode GpioDriveModeToPinMode(WinGpio.GpioPinDriveMode mode)
        {
            return mode switch
            {
                WinGpio.GpioPinDriveMode.Input => PinMode.Input,
                WinGpio.GpioPinDriveMode.Output => PinMode.Output,
                WinGpio.GpioPinDriveMode.InputPullDown => PinMode.InputPullDown,
                WinGpio.GpioPinDriveMode.InputPullUp => PinMode.InputPullUp,
                _ => throw new ArgumentException($"GPIO pin mode {mode} not supported.", nameof(mode))
            };
        }

        private static PinValue GpioPinValueToPinValue(WinGpio.GpioPinValue value)
        {
            return value switch
            {
                WinGpio.GpioPinValue.Low => PinValue.Low,
                WinGpio.GpioPinValue.High => PinValue.High,
                _ => throw new ArgumentException($"GPIO pin value {value} not supported.", nameof(value))
            };
        }

        private static WinGpio.GpioPinValue PinValueToGpioPinValue(PinValue value)
            => value == PinValue.High ? WinGpio.GpioPinValue.High : WinGpio.GpioPinValue.Low;

        private static PinEventTypes GpioEdgeToPinEventType(WinGpio.GpioPinEdge edge)
        {
            return edge switch
            {
                WinGpio.GpioPinEdge.FallingEdge => PinEventTypes.Falling,
                WinGpio.GpioPinEdge.RisingEdge => PinEventTypes.Rising,
                _ => throw new ArgumentException($"GPIO pin edge value {edge} not supported.", nameof(edge))
            };
        }

        #endregion Enumeration conversion methods
    }
}

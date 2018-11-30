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

        public void AddCallbackForPinValueChangedEvent(PinEventTypes eventType, PinChangeEventHandler callback)
        {
            if (eventType == PinEventTypes.None)
            {
                throw new ArgumentException($"{PinEventTypes.None} is an invalid value", nameof(eventType));
            }

            bool isFirstCallback = _risingCallbacks == null && _fallingCallbacks == null;

            if (eventType.HasFlag(PinEventTypes.Rising))
            {
                _risingCallbacks += callback;
            }

            if (eventType.HasFlag(PinEventTypes.Falling))
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

        public WaitForEventResult WaitForEvent(PinEventTypes eventType, CancellationToken cancellationToken)
        {
            using (ManualResetEvent completionEvent = new ManualResetEvent(false))
            {
                PinEventTypes pinEventType = PinEventTypes.None;
                void handler(WinGpio.GpioPin s, WinGpio.GpioPinValueChangedEventArgs a)
                {
                    pinEventType = GpioEdgeToPinEventType(a.Edge);

                    if ((pinEventType & eventType) != 0)
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
                    EventType = pinEventType,
                    TimedOut = !eventOccurred
                };
            }
        }

        public void Write(PinValue value) => _pin.Write(PinValueToGpioPinValue(value));

        #region Enumeration conversion methods

        private static WinGpio.GpioPinDriveMode PinModeToGpioDriveMode(PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input:
                    return WinGpio.GpioPinDriveMode.Input;
                case PinMode.Output:
                    return WinGpio.GpioPinDriveMode.Output;
                case PinMode.InputPullDown:
                    return WinGpio.GpioPinDriveMode.InputPullDown;
                case PinMode.InputPullUp:
                    return WinGpio.GpioPinDriveMode.InputPullUp;
                default:
                    throw new ArgumentException($"GPIO pin mode not supported: {mode}", nameof(mode));
            }
        }

        private static PinMode GpioDriveModeToPinMode(WinGpio.GpioPinDriveMode mode)
        {
            switch (mode)
            {
                case WinGpio.GpioPinDriveMode.Input:
                    return PinMode.Input;
                case WinGpio.GpioPinDriveMode.Output:
                    return PinMode.Output;
                case WinGpio.GpioPinDriveMode.InputPullDown:
                    return PinMode.InputPullDown;
                case WinGpio.GpioPinDriveMode.InputPullUp:
                    return PinMode.InputPullUp;
                default:
                    throw new ArgumentException($"GPIO pin mode not supported: {mode}", nameof(mode));
            }
        }

        private static PinValue GpioPinValueToPinValue(WinGpio.GpioPinValue value)
        {
            switch (value)
            {
                case WinGpio.GpioPinValue.Low:
                    return PinValue.Low;
                case WinGpio.GpioPinValue.High:
                    return PinValue.High;
                default:
                    throw new ArgumentException($"GPIO pin value not supported: {value}", nameof(value));
            }
        }

        private static WinGpio.GpioPinValue PinValueToGpioPinValue(PinValue value)
        {
            switch (value)
            {
                case PinValue.Low:
                    return WinGpio.GpioPinValue.Low;
                case PinValue.High:
                    return WinGpio.GpioPinValue.High;
                default:
                    throw new ArgumentException($"GPIO pin value not supported: {value}", nameof(value));
            }
        }

        private static PinEventTypes GpioEdgeToPinEventType(WinGpio.GpioPinEdge edge)
        {
            switch (edge)
            {
                case WinGpio.GpioPinEdge.FallingEdge:
                    return PinEventTypes.Falling;
                case WinGpio.GpioPinEdge.RisingEdge:
                    return PinEventTypes.Rising;
                default:
                    throw new ArgumentException($"GPIO pin edge value not supported: {edge}", nameof(edge));
            }
        }

        #endregion Enumeration conversion methods
    }
}

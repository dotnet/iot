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
    public partial class Windows10Driver
    {
        private class DevicePin : IDisposable
        {
            private const int ReasonableDebouceTimeoutMillseconds = 100;

            private WeakReference<Windows10Driver> _driver;
            private WinGpio.GpioPin _pin;
            private readonly int _pinNumber;
            private PinChangeEventHandler _risingCallbacks;
            private PinChangeEventHandler _fallingCallbacks;

            public DevicePin(Windows10Driver driver, WinGpio.GpioPin pin)
            {
                _driver = new WeakReference<Windows10Driver>(driver);
                _pin = pin;
                _pinNumber = _pin.PinNumber;

                // Set a reasonable default for DebounceTimeout (until .NET Core API adds a DebouceTimeout property)
                _pin.DebounceTimeout = TimeSpan.FromMilliseconds(ReasonableDebouceTimeoutMillseconds);
            }

            ~DevicePin()
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
                ManualResetEventSlim waitEvent = new ManualResetEventSlim();

                void handler(WinGpio.GpioPin s, WinGpio.GpioPinValueChangedEventArgs a)
                {
                    if (a.Edge == WinGpio.GpioPinEdge.FallingEdge && eventType.HasFlag(PinEventTypes.Falling) ||
                        a.Edge == WinGpio.GpioPinEdge.RisingEdge && eventType.HasFlag(PinEventTypes.Rising))
                    {
                        waitEvent.Set();
                    }
                }

                _pin.ValueChanged += handler;
                bool eventOccurred = false; // waitEvent.Wait(timeout);
                _pin.ValueChanged -= handler;

                return new WaitForEventResult
                {
                    EventType = eventType,
                    TimedOut = !eventOccurred
                };
            }

            public void Write(PinValue value) => _pin.Write(PinValueToGpioPinValue(value));
        }
    }
}

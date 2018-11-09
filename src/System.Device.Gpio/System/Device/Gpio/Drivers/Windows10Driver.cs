// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WinGpio = global::Windows.Devices.Gpio;

namespace System.Device.Gpio

namespace System.Device.Gpio.Drivers
{
    public class Windows10Driver : GpioDriver
    {
        #region Fields
        private static readonly WinGpio.GpioController _winGpioController = WinGpio.GpioController.GetDefault();
        private readonly Dictionary<int, DevicePin> _openPins = new Dictionary<int, DevicePin>();
        #endregion Fields

        #region GpioDrive Overrides

        protected internal override int PinCount => _winGpioController.PinCount;

        protected override void Dispose(bool disposing)
        {
            foreach (DevicePin devicePin in _openPins.Values)
            {
                devicePin.Dispose();
            }

            base.Dispose(disposing);
        }

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).AddCallbackForPinValueChangedEvent(eventType, callback);

        protected internal override void ClosePin(int pinNumber)
        {
            VerifyPinIsOpen(pinNumber, nameof(pinNumber)).ClosePin();
            _openPins.Remove(pinNumber);
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
            => throw new NotSupportedException($"The {GetType().Name} driver does not support physical (board) numbering, since it's generic.");

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).IsPinModeSupported(mode);

        protected internal override void OpenPin(int pinNumber)
        {
            VerifyPinIsClosed(pinNumber, nameof(pinNumber));

            if (pinNumber.CompareTo(0) < 0 || PinCount.CompareTo(pinNumber) <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber));
            }

            WinGpio.GpioPin windowsPin = _winGpioController.OpenPin(pinNumber, WinGpio.GpioSharingMode.Exclusive);
            _openPins[pinNumber] = new DevicePin(this, windowsPin);
        }

        protected internal override PinValue Read(int pinNumber) => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).Read();

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).RemoveCallbackForPinValueChangedEvent(callback);

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).SetPinMode(mode);

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, int timeoutMilliseconds)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).WaitForEvent(eventType, timeoutMilliseconds);

        protected internal override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventType, int timeoutMilliseconds)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).WaitForEventAsync(eventType, timeoutMilliseconds);

        protected internal override void Write(int pinNumber, PinValue value)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).Write(value);

        #endregion GpioDrive Overrides

        #region Private Implementation

        private DevicePin VerifyPinIsOpen(int pinNumber, string argumentName)
        {
            if (!_openPins.TryGetValue(pinNumber, out DevicePin devicePin))
            {
                throw new ArgumentException($"Specified GPIO pin is not open: {pinNumber}", argumentName);
            }

            return devicePin;
        }

        private void VerifyPinIsClosed(int pinNumber, string argumentName)
        {
            if (_openPins.ContainsKey(pinNumber))
            {
                throw new ArgumentException($"Specified GPIO pin is already open: {pinNumber}", argumentName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                    throw new NotSupportedException($"GPIO pin mode not supported: {mode}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                    throw new NotSupportedException($"GPIO pin mode not supported: {mode}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PinValue GpioPinValueToPinValue(WinGpio.GpioPinValue value)
        {
            switch (value)
            {
                case WinGpio.GpioPinValue.Low:
                    return PinValue.Low;
                case WinGpio.GpioPinValue.High:
                    return PinValue.High;
                default:
                    throw new NotSupportedException($"GPIO pin value not supported: {value}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static WinGpio.GpioPinValue PinValueToGpioPinValue(PinValue value)
        {
            switch (value)
            {
                case PinValue.Low:
                    return WinGpio.GpioPinValue.Low;
                case PinValue.High:
                    return WinGpio.GpioPinValue.High;
                default:
                    throw new NotSupportedException($"GPIO pin value not supported: {value}");
            }
        }

        #endregion Private Implementation

        #region Nested Types

        private class DevicePin : IDisposable
        {
            private const int _reasonableDebouceTimeoutMillseconds = 100;

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
                _pin.DebounceTimeout = TimeSpan.FromMilliseconds(_reasonableDebouceTimeoutMillseconds);
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

            public WaitForEventResult WaitForEvent(PinEventTypes eventType, int timeout)
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
                bool eventOccurred = waitEvent.Wait(timeout);
                _pin.ValueChanged -= handler;

                return new WaitForEventResult
                {
                    EventType = eventType,
                    TimedOut = !eventOccurred
                };
            }

            public ValueTask<WaitForEventResult> WaitForEventAsync(PinEventTypes eventType, int timeout)
            {
                return new ValueTask<WaitForEventResult>(Task<WaitForEventResult>.Run(() => WaitForEvent(eventType, timeout)));
            }

            public void Write(PinValue value) => _pin.Write(PinValueToGpioPinValue(value));
        }

        #endregion Nested Types
    }
}

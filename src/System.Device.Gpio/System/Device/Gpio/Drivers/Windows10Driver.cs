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
    public partial class Windows10Driver : GpioDriver
    {
        #region Fields
        private static readonly WinGpio.GpioController s_winGpioController = WinGpio.GpioController.GetDefault();
        private readonly Dictionary<int, DevicePin> _openPins = new Dictionary<int, DevicePin>();
        #endregion Fields

        #region GpioDrive Overrides

        protected internal override int PinCount => s_winGpioController.PinCount;

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
            if (_openPins.TryGetValue(pinNumber, out DevicePin pin))
            {
                pin.ClosePin();
                _openPins.Remove(pinNumber);
            }
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
            => throw new NotSupportedException($"The {GetType().Name} driver does not support physical (board) numbering, since it's generic.");

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).IsPinModeSupported(mode);

        protected internal override void OpenPin(int pinNumber)
        {
            // Ignore calls to open an already open pin
            if (_openPins.ContainsKey(pinNumber))
            {
                return;
            }

            if (pinNumber < 0 || pinNumber >= PinCount)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber));
            }

            WinGpio.GpioPin windowsPin = s_winGpioController.OpenPin(pinNumber, WinGpio.GpioSharingMode.Exclusive);
            _openPins[pinNumber] = new DevicePin(this, windowsPin);
        }

        protected internal override PinValue Read(int pinNumber) => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).Read();

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).RemoveCallbackForPinValueChangedEvent(callback);

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).SetPinMode(mode);

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
            => VerifyPinIsOpen(pinNumber, nameof(pinNumber)).WaitForEvent(eventType, cancellationToken);

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
    }
}

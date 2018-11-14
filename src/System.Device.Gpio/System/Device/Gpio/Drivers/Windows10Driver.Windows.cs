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
            => throw new PlatformNotSupportedException($"The {GetType().Name} driver does not support physical (board) numbering, since it's generic.");

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
                throw new InvalidOperationException($"Specified GPIO pin is not open: {pinNumber}");
            }

            return devicePin;
        }

        #endregion Private Implementation
    }
}

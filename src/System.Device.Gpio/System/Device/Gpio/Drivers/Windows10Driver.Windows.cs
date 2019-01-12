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
    public class Windows10Driver : GpioDriver
    {
        private static readonly WinGpio.GpioController s_winGpioController = WinGpio.GpioController.GetDefault();
        private readonly Dictionary<int, Windows10DriverPin> _openPins = new Dictionary<int, Windows10DriverPin>();
        
        public Windows10Driver()
        {
            if (s_winGpioController == null)
            {
                throw new NotSupportedException("No GPIO controllers exist on this system.");
            }
        }

        protected internal override int PinCount => s_winGpioController.PinCount;

        protected override void Dispose(bool disposing)
        {
            foreach (Windows10DriverPin devicePin in _openPins.Values)
            {
                devicePin.Dispose();
            }

            _openPins.Clear();

            base.Dispose(disposing);
        }

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
            => LookupOpenPin(pinNumber).AddCallbackForPinValueChangedEvent(eventType, callback);

        protected internal override void ClosePin(int pinNumber)
        {
            if (_openPins.TryGetValue(pinNumber, out Windows10DriverPin pin))
            {
                pin.ClosePin();
                _openPins.Remove(pinNumber);
            }
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
            => throw new PlatformNotSupportedException($"The {GetType().Name} driver does not support physical (board) numbering, since it's generic.");

        protected internal override PinMode GetPinMode(int pinNumber) => LookupOpenPin(pinNumber).GetPinMode();

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode) => LookupOpenPin(pinNumber).IsPinModeSupported(mode);

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
            _openPins[pinNumber] = new Windows10DriverPin(this, windowsPin);
        }

        protected internal override PinValue Read(int pinNumber) => LookupOpenPin(pinNumber).Read();

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
            => LookupOpenPin(pinNumber).RemoveCallbackForPinValueChangedEvent(callback);

        protected internal override void SetPinMode(int pinNumber, PinMode mode) => LookupOpenPin(pinNumber).SetPinMode(mode);

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
            => LookupOpenPin(pinNumber).WaitForEvent(eventType, cancellationToken);

        protected internal override void Write(int pinNumber, PinValue value) => LookupOpenPin(pinNumber).Write(value);

        private Windows10DriverPin LookupOpenPin(int pinNumber) => _openPins[pinNumber];
    }
}

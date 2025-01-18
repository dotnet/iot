// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;

namespace Iot.Device.Gpio
{
    /// <summary>
    /// A virtual GpioPin to allow creating a new GpioPin and a new pin number
    /// </summary>
    internal class VirtualGpioPin : GpioPin
    {
        private readonly GpioController _virtualController;
        private readonly GpioPin _oldPin;

        /// <summary>
        /// Create a virtual GPIO Pin from a real one
        /// </summary>
        /// <param name="oldPin">The existing pin</param>
        /// <param name="newPinNumber">The new pin number</param>
        /// <param name="virtualController">The controller containing the virtual pin numbering</param>
        protected internal VirtualGpioPin(GpioPin oldPin, int newPinNumber, GpioController virtualController)
            : base(newPinNumber, virtualController)
        {
            _virtualController = virtualController;
            _oldPin = oldPin;
        }

        private event PinChangeEventHandler? InternalValueChanged;

        internal int OldPinNumber => _oldPin.PinNumber;

        internal GpioController OldController => _oldPin.Controller;

        public override event PinChangeEventHandler ValueChanged
        {
            add
            {
                _oldPin.Controller.RegisterCallbackForPinValueChangedEvent(_oldPin.PinNumber, PinEventTypes.Falling | PinEventTypes.Rising, OldValueChanged);
                InternalValueChanged += value;
            }

            remove
            {
                _oldPin.Controller.UnregisterCallbackForPinValueChangedEvent(_oldPin.PinNumber, OldValueChanged);
                InternalValueChanged -= value;
            }
        }

        private void OldValueChanged(object sender, PinValueChangedEventArgs pinvaluechangedeventargs)
        {
            if (pinvaluechangedeventargs.PinNumber == _oldPin.PinNumber)
            {
                // Forward the event, but with the new number
                var toForward = new PinValueChangedEventArgs(pinvaluechangedeventargs.ChangeType, PinNumber);
                InternalValueChanged?.Invoke(sender, toForward);
            }
        }

        public override PinMode GetPinMode()
        {
            return _oldPin.GetPinMode();
        }

        public override bool IsPinModeSupported(PinMode pinMode)
        {
            return _oldPin.IsPinModeSupported(pinMode);
        }

        public override PinValue Read()
        {
            return _oldPin.Read();
        }

        public override void SetPinMode(PinMode value)
        {
            _oldPin.SetPinMode(value);
        }

        public override void Toggle()
        {
            _oldPin.Toggle();
        }

        public override void Write(PinValue value)
        {
            _oldPin.Write(value);
        }

        public override void Close()
        {
            _oldPin.Close();
        }

        public override bool Equals(GpioPin? other)
        {
            if (other is VirtualGpioPin otherPin)
            {
                return base.Equals(other) && _oldPin.Equals(otherPin._oldPin);
            }

            return false;
        }
    }
}

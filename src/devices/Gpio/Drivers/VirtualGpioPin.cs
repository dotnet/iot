// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.Gpio
{
    /// <summary>
    /// Represents a general-purpose I/O (GPIO) pin.
    /// </summary>
    public class VirtualGpioPin : GpioPin
    {
        private int _virtualPinNumber;
        private GpioPin _pin;

        /// <inheritdoc/>
        public override event PinChangeEventHandler? ValueChanged;

        /// <inheritdoc/>
        public override int PinNumber => _virtualPinNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGpioPin"/> class.
        /// </summary>
        /// <param name="gpioPin">A valid <see cref="GpioPin"/>.</param>
        /// <param name="virtualPinNumber">The new virutal pin number.</param>
        public VirtualGpioPin(GpioPin gpioPin, int virtualPinNumber)
        {
            _virtualPinNumber = virtualPinNumber;
            _pin = gpioPin ?? throw new ArgumentNullException(nameof(gpioPin));
            _pin.ValueChanged += PinValueChanged;
        }

        private void PinValueChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            ValueChanged?.Invoke(this, pinValueChangedEventArgs);
        }

        /// <inheritdoc/>
        public override PinMode GetPinMode() => _pin.GetPinMode();

        /// <inheritdoc/>
        public override bool IsPinModeSupported(PinMode pinMode) => _pin.IsPinModeSupported(pinMode);

        /// <inheritdoc/>
        public override void SetPinMode(PinMode value) => _pin.SetPinMode(value);

        /// <inheritdoc/>
        public override PinValue Read() => _pin.Read();

        /// <inheritdoc/>
        public override void Write(PinValue value) => _pin.Write(value);

        /// <inheritdoc/>
        public override void Toggle() => _pin.Toggle();
    }
}

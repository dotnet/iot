// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Represents one of the buffered, 5V tolerant digital inputs of the Explorer HAT
    /// </summary>
    /// <remarks>
    /// The input is behind a buffer (SN74LVC125A) that protects the Raspberry Pi from 5V signals, so the internal
    /// pull-up and pull-down resistors of the Raspberry Pi have no effect. The buffer drives the GPIO pin, so the pin
    /// cannot be written or configured as an output.
    /// The pin is opened the first time it is used (<see cref="Read"/> or <see cref="ValueChanged"/>), so the inputs
    /// that are not used can be used by other bindings. An input never takes over a pin that was already open: it throws
    /// <see cref="InvalidOperationException"/> instead, and <see cref="Close"/> and <see cref="GpioPin.Dispose()"/>
    /// close only a pin that this input opened.
    /// </remarks>
    public class DigitalInput : GpioPin
    {
        private bool _isPinOpen;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a <see cref="DigitalInput"/> instance
        /// </summary>
        /// <param name="pin">Underlying rpi GPIO pin number</param>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="DigitalInput"/> to manage GPIO resources</param>
        internal DigitalInput(int pin, GpioController controller)
            : base(pin, controller)
        {
        }

        /// <summary>
        /// Reads the value of the input. The first call opens the pin.
        /// </summary>
        /// <returns><see cref="PinValue.High"/> if the input receives a high signal, otherwise <see cref="PinValue.Low"/></returns>
        /// <exception cref="InvalidOperationException">The pin was already open</exception>
        public override PinValue Read()
        {
            EnsurePinIsOpen();
            return base.Read();
        }

        /// <summary>
        /// Gets the pin mode, which is always <see cref="PinMode.Input"/>. It does not open the pin.
        /// </summary>
        /// <returns><see cref="PinMode.Input"/></returns>
        public override PinMode GetPinMode()
        {
            ThrowIfDisposed();
            return PinMode.Input;
        }

        /// <summary>
        /// Not supported to set an output mode: the input is behind a buffer that drives the GPIO pin
        /// </summary>
        /// <param name="value">The pin mode</param>
        /// <exception cref="NotSupportedException">The mode is <see cref="PinMode.Output"/></exception>
        public override void SetPinMode(PinMode value)
        {
            if (value == PinMode.Output)
            {
                throw new NotSupportedException("The Explorer HAT inputs are driven by a buffer and cannot be outputs.");
            }

            EnsurePinIsOpen();
            base.SetPinMode(value);
        }

        /// <summary>
        /// Not supported: the input is behind a buffer that drives the GPIO pin, so the pin cannot be written
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <exception cref="NotSupportedException">Always</exception>
        public override void Write(PinValue value) =>
            throw new NotSupportedException("The Explorer HAT inputs are driven by a buffer and cannot be written.");

        /// <summary>
        /// Not supported: the input is behind a buffer that drives the GPIO pin, so the pin cannot be written
        /// </summary>
        /// <exception cref="NotSupportedException">Always</exception>
        public override void Toggle() =>
            throw new NotSupportedException("The Explorer HAT inputs are driven by a buffer and cannot be written.");

        /// <inheritdoc />
        public override event PinChangeEventHandler ValueChanged
        {
            add
            {
                EnsurePinIsOpen();
                base.ValueChanged += value;
            }

            remove
            {
                if (_isPinOpen)
                {
                    base.ValueChanged -= value;
                }
            }
        }

        /// <summary>
        /// Closes the pin, if this input opened it. A pin that was open before the input was used is not closed.
        /// The next <see cref="Read"/> opens the pin again.
        /// </summary>
        public override void Close()
        {
            if (_isPinOpen)
            {
                _isPinOpen = false;
                Controller.ClosePin(PinNumber);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        private void EnsurePinIsOpen()
        {
            ThrowIfDisposed();
            if (_isPinOpen)
            {
                return;
            }

            if (Controller.IsPinOpen(PinNumber))
            {
                throw new InvalidOperationException($"GPIO {PinNumber} is already open. Close it before using it through the Explorer HAT.");
            }

            Controller.OpenPin(PinNumber, PinMode.Input);
            _isPinOpen = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(DigitalInput));
            }
        }
    }
}

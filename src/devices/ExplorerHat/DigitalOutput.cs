// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Represents one of the open collector outputs of the Explorer HAT
    /// </summary>
    /// <remarks>
    /// The outputs are driven by a Darlington array (ULN2003A, up to 500mA in total across the four outputs).
    /// When the output is on, it sinks current to ground: connect the device between a positive supply (for example
    /// 5V) and the output. When it is off, the output is left floating, not driven high. That is why <see cref="On"/>
    /// and <see cref="Off"/> exist besides <see cref="Write"/>: writing <see cref="PinValue.High"/> switches the output on.
    /// The pin is opened the first time it is used (for example <see cref="On"/>), so the outputs that are not used can
    /// be used by other bindings. An output never takes over a pin that was already open: it throws
    /// <see cref="InvalidOperationException"/> instead, and <see cref="Close"/> and <see cref="GpioPin.Dispose()"/>
    /// switch the output off and close only a pin that this output opened.
    /// </remarks>
    public class DigitalOutput : GpioPin
    {
        private bool _isPinOpen;
        private bool _isDisposed;

        /// <summary>
        /// Gets if the output is switched on or not
        /// </summary>
        public bool IsOn { get; private set; }

        /// <summary>
        /// Initializes a <see cref="DigitalOutput"/> instance
        /// </summary>
        /// <param name="pin">Underlying rpi GPIO pin number</param>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="DigitalOutput"/> to manage GPIO resources</param>
        internal DigitalOutput(int pin, GpioController controller)
            : base(pin, controller)
        {
        }

        /// <summary>
        /// Switches on the output: it sinks current to ground. The first call opens the pin.
        /// </summary>
        /// <exception cref="InvalidOperationException">The pin was already open</exception>
        public void On() => Write(PinValue.High);

        /// <summary>
        /// Switches off the output: it is left floating. The first call opens the pin.
        /// </summary>
        /// <exception cref="InvalidOperationException">The pin was already open</exception>
        public void Off() => Write(PinValue.Low);

        /// <summary>
        /// Switches the output on (<see cref="PinValue.High"/>) or off (<see cref="PinValue.Low"/>). The first call opens the pin.
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <exception cref="InvalidOperationException">The pin was already open</exception>
        public override void Write(PinValue value)
        {
            ThrowIfDisposed();
            if (!_isPinOpen)
            {
                if (Controller.IsPinOpen(PinNumber))
                {
                    throw new InvalidOperationException($"GPIO {PinNumber} is already open. Close it before using it through the Explorer HAT.");
                }

                // The initial value is the one requested, so the output does not flash off before it is switched on
                Controller.OpenPin(PinNumber, PinMode.Output, value);
                _isPinOpen = true;
            }
            else
            {
                base.Write(value);
            }

            IsOn = value == PinValue.High;
        }

        /// <summary>
        /// Switches the output off if it is on, and on if it is off. The first call opens the pin.
        /// </summary>
        /// <exception cref="InvalidOperationException">The pin was already open</exception>
        public override void Toggle() => Write(IsOn ? PinValue.Low : PinValue.High);

        /// <summary>
        /// Reads the value of the pin: <see cref="PinValue.High"/> if the output is on. The first call opens the pin.
        /// </summary>
        /// <returns>The value of the pin</returns>
        /// <exception cref="InvalidOperationException">The pin was already open</exception>
        public override PinValue Read()
        {
            if (!_isPinOpen)
            {
                Off();
            }

            return base.Read();
        }

        /// <summary>
        /// Gets the pin mode, which is always <see cref="PinMode.Output"/>. It does not open the pin.
        /// </summary>
        /// <returns><see cref="PinMode.Output"/></returns>
        public override PinMode GetPinMode()
        {
            ThrowIfDisposed();
            return PinMode.Output;
        }

        /// <summary>
        /// Not supported to set an input mode: the output is driven by a Darlington array
        /// </summary>
        /// <param name="value">The pin mode</param>
        /// <exception cref="NotSupportedException">The mode is not <see cref="PinMode.Output"/></exception>
        public override void SetPinMode(PinMode value)
        {
            if (value != PinMode.Output)
            {
                throw new NotSupportedException("The Explorer HAT outputs cannot be inputs.");
            }

            ThrowIfDisposed();
        }

        /// <summary>
        /// Switches off the output and closes the pin, if this output opened it. A pin that was open before the output
        /// was used is not closed. The next <see cref="On"/> or <see cref="Off"/> opens the pin again.
        /// </summary>
        public override void Close()
        {
            if (_isPinOpen)
            {
                _isPinOpen = false;
                Controller.Write(PinNumber, PinValue.Low);
                Controller.ClosePin(PinNumber);
            }

            IsOn = false;
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

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(DigitalOutput));
            }
        }
    }
}

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
    /// 5V) and the output. When it is off, the output is left floating, not driven high.
    /// The pin is opened the first time <see cref="On"/> or <see cref="Off"/> is called, so it can also be used
    /// through <see cref="Pin"/> with other bindings.
    /// </remarks>
    public class DigitalOutput : IDisposable
    {
        private GpioController? _controller;
        private bool _isPinOpen;

        /// <summary>
        /// GPIO pin to which the output is attached
        /// </summary>
        public int Pin { get; }

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
        {
            Pin = pin;
            _controller = controller;
        }

        /// <summary>
        /// Switches on the output: it sinks current to ground. The first call opens the pin.
        /// </summary>
        public void On()
        {
            Write(PinValue.High);
            IsOn = true;
        }

        /// <summary>
        /// Switches off the output: it is left floating. The first call opens the pin.
        /// </summary>
        public void Off()
        {
            Write(PinValue.Low);
            IsOn = false;
        }

        private void Write(PinValue value)
        {
            GpioController controller = _controller ?? throw new ObjectDisposedException(nameof(DigitalOutput));
            if (!_isPinOpen)
            {
                controller.OpenPin(Pin, PinMode.Output, value);
                _isPinOpen = true;
            }
            else
            {
                controller.Write(Pin, value);
            }
        }

        /// <summary>
        /// Switches off the output and closes the pin if <see cref="On"/> or <see cref="Off"/> opened it
        /// </summary>
        public void Dispose()
        {
            if (_isPinOpen && _controller is object)
            {
                _controller.Write(Pin, PinValue.Low);
                _controller.ClosePin(Pin);
            }

            _isPinOpen = false;
            IsOn = false;
            _controller = null;
        }
    }
}

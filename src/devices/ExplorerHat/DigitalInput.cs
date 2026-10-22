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
    /// pull-up and pull-down resistors of the Raspberry Pi have no effect. The pin is opened the first time
    /// <see cref="Read"/> is called, so it can also be used through <see cref="Pin"/> with other bindings.
    /// </remarks>
    public class DigitalInput : IDisposable
    {
        private GpioController? _controller;
        private bool _isPinOpen;

        /// <summary>
        /// GPIO pin to which the input is attached
        /// </summary>
        public int Pin { get; }

        /// <summary>
        /// Initializes a <see cref="DigitalInput"/> instance
        /// </summary>
        /// <param name="pin">Underlying rpi GPIO pin number</param>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="DigitalInput"/> to manage GPIO resources</param>
        internal DigitalInput(int pin, GpioController controller)
        {
            Pin = pin;
            _controller = controller;
        }

        /// <summary>
        /// Reads the value of the input. The first call opens the pin.
        /// </summary>
        /// <returns><see cref="PinValue.High"/> if the input receives a high signal, otherwise <see cref="PinValue.Low"/></returns>
        public PinValue Read()
        {
            GpioController controller = _controller ?? throw new ObjectDisposedException(nameof(DigitalInput));
            if (!_isPinOpen)
            {
                controller.OpenPin(Pin, PinMode.Input);
                _isPinOpen = true;
            }

            return controller.Read(Pin);
        }

        /// <summary>
        /// Closes the pin if <see cref="Read"/> opened it
        /// </summary>
        public void Dispose()
        {
            if (_isPinOpen && _controller is object)
            {
                _controller.ClosePin(Pin);
            }

            _isPinOpen = false;
            _controller = null;
        }
    }
}

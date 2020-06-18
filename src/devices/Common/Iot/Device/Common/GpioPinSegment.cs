// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.Common
{
    /// <summary>
    /// IWritablePinSegment implementation that uses GpioController.
    /// </summary>
    public class GpioSegment : IWritablePinSegment
    {
        private GpioController _controller;
        private int[] _pins;

        /// <summary>
        /// IWritablePinSegment implementation that uses GpioController.
        /// </summary>
        /// <param name="pins">The GPIO pins that should be used and are connected.</param>
        /// <param name="gpioController">The GpioController to use. If one isn't provided, one will be created.</param>
        public GpioSegment(int[] pins, GpioController gpioController = null)
        {
            _pins = pins;

            if (gpioController is null)
            {
                gpioController = new GpioController();
            }

            _controller = gpioController;

            foreach (var pin in _pins)
            {
                _controller.OpenPin(pin, PinMode.Output);
            }
        }

        /// <summary>
        /// The length of the segment; the number of GPIO pins it exposes.
        /// </summary>
        public int Length => _pins.Length;

        /// <summary>
        /// Writes a PinValue to the underlying GpioController.
        /// </summary>
        public void Write(int pin, PinValue value)
        {
            _controller.Write(_pins[pin], value);
        }

        /// <summary>
        /// Disposes the underlying GpioController.
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)_controller).Dispose();
        }
    }
}

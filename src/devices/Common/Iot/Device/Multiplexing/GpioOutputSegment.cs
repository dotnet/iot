// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Multiplexing.Utility;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// IOutputSegment implementation that uses GpioController.
    /// </summary>
    public class GpioOutputSegment : IOutputSegment, IDisposable
    {
        private readonly int[] _pins;
        private readonly bool _shouldDispose;
        private readonly VirtualOutputSegment _segment;
        private GpioController _controller;

        /// <summary>
        /// IOutputSegment implementation that uses GpioController.
        /// </summary>
        /// <param name="pins">The GPIO pins that should be used and are connected.</param>
        /// <param name="gpioController">The GpioController to use. If one isn't provided, one will be created.</param>
        /// <param name="shouldDispose">The policy to use (true, by default) for disposing the GPIO controller when disposing this instance.</param>
        public GpioOutputSegment(int[] pins, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _shouldDispose = shouldDispose || gpioController is null;
            _controller = gpioController ?? new GpioController();
            _pins = pins;
            _segment = new VirtualOutputSegment(_pins.Length);

            foreach (var pin in _pins)
            {
                _controller.OpenPin(pin, PinMode.Output);
            }
        }

        /// <summary>
        /// The length of the segment; the number of GPIO pins it exposes.
        /// </summary>
        public int Length => _segment.Length;

        /// <summary>
        /// Segment values.
        /// </summary>
        public PinValue this[int index] => _segment[index];

        /// <summary>
        /// Writes a PinValue to the underlying GpioController.
        /// </summary>
        public void Write(int pin, PinValue value)
        {
            _segment.Write(pin, value);
        }

        /// <summary>
        /// Writes a byte to the underlying GpioController.
        /// </summary>
        public void Write(int value)
        {
            _segment.Write(value);
        }

        /// <summary>
        /// Writes a byte to the underlying GpioController.
        /// </summary>
        public void Clear()
        {
            _segment.Clear();
            Display();
        }

        /// <summary>
        /// Displays segment until token receives a cancellation signal, possibly due to a specificated duration.
        /// Publishes (latches) values.
        /// </summary>
        public void Display(CancellationToken token)
        {
            Display();
            _segment.Display(token);
        }

        private void Display()
        {
            for (int i = 0; i < _pins.Length; i++)
            {
                _controller.Write(_pins[i], _segment[i]);
            }
        }

        /// <summary>
        /// Disposes the underlying GpioController.
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null!;
            }
        }
    }
}

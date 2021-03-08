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
        private readonly CancellationToken _token;
        private readonly VirtualOutputSegment _segment;
        private GpioController _controller;

        /// <summary>
        /// IOutputSegment implementation that uses GpioController.
        /// </summary>
        /// <param name="pins">The GPIO pins that should be used and are connected.</param>
        /// <param name="token">Cancellation token to use to notify cancelling the output segment.</param>
        /// <param name="gpioController">The GpioController to use. If one isn't provided, one will be created.</param>
        /// <param name="shouldDispose">The policy to use (true, by default) for disposing the GPIO controller when disposing this instance.</param>
        public GpioOutputSegment(int[] pins, CancellationToken token, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _shouldDispose = shouldDispose || gpioController is null;
            _controller = gpioController ?? new GpioController();
            _pins = pins;
            _token = token;
            _segment = new VirtualOutputSegment(_pins.Length, _token);

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
        /// Displays segment for a given duration.
        /// Alternative to Thread.Sleep
        /// </summary>
        public void Display(TimeSpan time)
        {
            Display();
            _segment.Display(time);
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

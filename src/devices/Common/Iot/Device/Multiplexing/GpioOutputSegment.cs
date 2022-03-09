// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        public PinValue this[int index]
        {
            get => _segment[index];
            set => _segment[index] = value;
        }

        /// <summary>
        /// Writes a PinValue to a virtual segment.
        /// Does not display output.
        /// </summary>
        public void Write(int pin, PinValue value)
        {
            _segment.Write(pin, value);
        }

        /// <summary>
        /// Writes discrete underlying bits to a virtual segment.
        /// Writes each bit, left to right. Least significant bit will written to index 0.
        /// Does not display output.
        /// </summary>
        public void Write(byte value)
        {
            _segment.Write(value);
        }

        /// <summary>
        /// Writes discrete underlying bits to a virtual segment.
        /// Writes each byte, left to right. Least significant bit will written to index 0.
        /// Does not display output.
        /// </summary>
        public void Write(ReadOnlySpan<byte> value)
        {
            _segment.Write(value);
        }

        /// <summary>
        /// Writes a byte to the underlying GpioController.
        /// </summary>
        public void TurnOffAll()
        {
            _segment.TurnOffAll();
            Display();
        }

        /// <summary>
        /// Displays current state of segment.
        /// Segment is displayed at least until token receives a cancellation signal, possibly due to a specified duration expiring.
        /// </summary>
        public void Display(CancellationToken token)
        {
            Display();
            _segment.Display(token);
        }

        /// <summary>
        /// Displays current state of segment.
        /// Segment is displayed at least until token receives a cancellation signal, possibly due to a specified duration expiring.
        /// </summary>
        public Task DisplayAsync(CancellationToken token)
        {
            Display();
            return _segment.DisplayAsync(token);
        }

        private void Display()
        {
            PinValuePair[] pairs = new PinValuePair[_pins.Length];
            for (int i = 0; i < _pins.Length; i++)
            {
                pairs[i] = new PinValuePair(_pins[i], _segment[i]);
            }

            _controller.Write(pairs);
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

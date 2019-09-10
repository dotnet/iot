// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Diagnostics;

namespace Iot.Device.RotaryEncoder
{
    /// <summary>
    /// Binding that exposes a quadrature rotary encoder
    /// </summary>
    public class QuadratureRotaryEncoder
    {
        private GpioController _controller;
        private int _pinA;
        private int _pinB;
        private bool _disposeController = true;
        private Stopwatch debouncer = new Stopwatch();

        /// <summary>
        /// The number of pulses expected per rotation of the encoder
        /// </summary>
        public int PulsesPerRotation { get; private set; }

        /// <summary>
        /// The number of pulses before or after the start position of the encoder
        /// </summary>
        public long PulseCount { get; set; }

        /// <summary>
        /// The number of rotations backwards or forwards from the initial position of the encoder
        /// </summary>
        public float Rotations { get => (float)PulseCount / PulsesPerRotation; }

        /// <summary>The DebounceMilliseconds property represents the minimum amount of delay allowed between falling edges of the A (clk) pin.</summary>
        public uint DebounceMilliseconds { get; set; } = 5;

        /// <summary>
        /// EventHandler to allow the notification of value changes.
        /// </summary>
        public event EventHandler<RotaryEncoderEventArgs<long>> PulseCountChanged;

        /// <summary>
        /// QuadratureRotaryEncoder constructor
        /// </summary>
        /// <param name="controller">GpioController that hosts Pins A and B.</param>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="edges">The pin event types to 'listen' for.</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        public QuadratureRotaryEncoder(GpioController controller, int pinA, int pinB, PinEventTypes edges, int pulsesPerRotation)
        {
            _disposeController = false;

            PulsesPerRotation = pulsesPerRotation;

            Initialize(controller, pinA, pinB, edges);
        }

        /// <summary>
        /// QuadratureRotaryEncoder constructor
        /// </summary>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        public QuadratureRotaryEncoder(int pinA, int pinB, int pulsesPerRotation) : this(new GpioController(), pinA, pinB, PinEventTypes.Falling, pulsesPerRotation) { }

        /// <summary>
        /// Modify the current value on receipt of a pulse from the rotary encoder.
        /// </summary>
        /// <param name="blnUp">When true then the value should be incremented otherwise it should be decremented.</param>
        /// <param name="milliSecondsSinceLastPulse">The number of miliseconds since the last pulse.</param>
        protected virtual void OnPulse(bool blnUp, int milliSecondsSinceLastPulse)
        {
            PulseCount += blnUp ? 1 : -1;

            // fire an event if an event handler has been attached
            if (PulseCountChanged != null)
            {
                PulseCountChanged.Invoke(this, new RotaryEncoderEventArgs<long>(PulseCount));
            }
        }

        /// <summary>
        /// Initialize an QuadratureRotaryEncoder
        /// </summary>
        /// <param name="controller">GpioController that hosts Pins A and B.</param>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="edges">The pin event types to 'listen' for.</param>
        private void Initialize(GpioController controller, int pinA, int pinB, PinEventTypes edges)
        {
            _controller = controller;
            _pinA = pinA;
            _pinB = pinB;

            _controller.OpenPin(_pinA, PinMode.Input);
            _controller.OpenPin(_pinB, PinMode.Input);

            debouncer.Start();

            _controller.RegisterCallbackForPinValueChangedEvent(_pinA, edges, (o, e) =>
            {
                if (DebounceMilliseconds == 0 | debouncer.ElapsedMilliseconds > DebounceMilliseconds)
                {
                    OnPulse(_controller.Read(_pinA) == _controller.Read(_pinB), (int)debouncer.ElapsedMilliseconds);
                }
                debouncer.Restart();
            });
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _controller?.ClosePin(_pinA);
            _controller?.ClosePin(_pinB);

            if (_disposeController)
            {
                _controller?.Dispose();
            }
        }
    }
}

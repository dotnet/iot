// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Diagnostics;

namespace Iot.Device.RotaryEncoder
{
    /// <summary>
    /// Binding that exposes a quadrature rotary encoder
    /// </summary>
    public class QuadratureRotaryEncoder : IDisposable
    {
        private GpioController _controller;
        private int _pinA;
        private int _pinB;
        private bool _disposeController = true;
        private Stopwatch _debouncer = new Stopwatch();
        private uint _debounceMillisec;

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
        public TimeSpan DebounceMilliseconds
        {
            get => TimeSpan.FromMilliseconds(_debounceMillisec);

            set
            {
                _debounceMillisec = (uint)value.TotalMilliseconds;
            }
        }

        /// <summary>
        /// EventHandler to allow the notification of value changes.
        /// </summary>
        public event EventHandler<RotaryEncoderEventArgs>? PulseCountChanged;

        /// <summary>
        /// QuadratureRotaryEncoder constructor
        /// </summary>
        /// <param name="controller">GpioController that hosts Pins A and B.</param>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="edges">The pin event types to 'listen' for.</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        /// <param name="shouldDispose">True to dispose the controller</param>
        public QuadratureRotaryEncoder(GpioController? controller, int pinA, int pinB, PinEventTypes edges, int pulsesPerRotation, bool shouldDispose = true)
        {
            _disposeController = controller == null | shouldDispose;
            _controller = controller ?? new GpioController();

            PulsesPerRotation = pulsesPerRotation;
            _debounceMillisec = 5;
            Initialize(pinA, pinB, edges);
        }

        /// <summary>
        /// QuadratureRotaryEncoder constructor
        /// </summary>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        public QuadratureRotaryEncoder(int pinA, int pinB, int pulsesPerRotation)
            : this(new GpioController(), pinA, pinB, PinEventTypes.Falling, pulsesPerRotation, false)
        {
        }

        /// <summary>
        /// Modify the current value on receipt of a pulse from the rotary encoder.
        /// </summary>
        /// <param name="blnUp">When true then the value should be incremented otherwise it should be decremented.</param>
        /// <param name="milliSecondsSinceLastPulse">The number of miliseconds since the last pulse.</param>
        protected virtual void OnPulse(bool blnUp, int milliSecondsSinceLastPulse)
        {
            PulseCount += blnUp ? 1 : -1;

            // fire an event if an event handler has been attached
            PulseCountChanged?.Invoke(this, new RotaryEncoderEventArgs(PulseCount));
        }

        /// <summary>
        /// Initialize an QuadratureRotaryEncoder
        /// </summary>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="edges">The pin event types to 'listen' for.</param>
        private void Initialize(int pinA, int pinB, PinEventTypes edges)
        {
            _pinA = pinA;
            _pinB = pinB;

            _controller.OpenPin(_pinA, PinMode.Input);
            _controller.OpenPin(_pinB, PinMode.Input);

            _debouncer.Start();

            _controller.RegisterCallbackForPinValueChangedEvent(_pinA, edges, (o, e) =>
            {
                if (_debounceMillisec == 0 | _debouncer.ElapsedMilliseconds > _debounceMillisec)
                {
                    OnPulse(_controller.Read(_pinA) == _controller.Read(_pinB), (int)_debouncer.ElapsedMilliseconds);
                }

                _debouncer.Restart();
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

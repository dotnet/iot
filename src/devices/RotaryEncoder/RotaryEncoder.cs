// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Diagnostics;

namespace Iot.Device.RotaryEncoder
{
    /// <summary>
    /// EventArgs used with the RotaryEncode binding to pass event information when the Value changes.
    /// </summary>
    /// <typeparam name="T">The element type of the Value controlled by the RotaryEncoder</typeparam>
    public class RotaryEncoderEventArgs<T> : EventArgs
    {
        /// <summary>The Value property represents current value associated with the RotaryEncoder.</summary>
        public T Value { get; private set; }

        /// <summary>
        /// Construct a new RotaryEncoderEventArgs
        /// </summary>
        /// <param name="value">Current value associated with the rotary encoder</param>
        public RotaryEncoderEventArgs(T value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Bindinding that exposes a 2 wire incremental rotary encode as a Value that varies as the encoder is turned.
    /// </summary>
    /// <typeparam name="T">The element type of the Value controlled by the RotaryEncoder</typeparam>
    public class RotaryEncoder<T> where T : IComparable<T>
    {
        private GpioController _controller;
        private int _pinA;
        private int _pinB;

        /// <summary>The Value property represents current value associated with the RotaryEncoder.</summary>
        public T Value { get; set; }

        /// <summary>The DebounceMilliseconds property represents the minimum amount of delay allowed between falling edges of the A (clk) pin.</summary>
        public uint DebounceMilliseconds { get; set; } = 5;

        /// <summary>The AccelerationSlope property along with the AccelerationOffset property represents how the
        /// increase or decrease in value should grow as the incremental encoder is turned faster.</summary>
        public float AccelerationSlope { get; set; } = -0.05F;

        /// <summary>The AccelerationOffset property along with the AccelerationSlope property represents how the
        /// increase or decrease in value should grow as the incremental encoder is turned faster.</summary>
        public float AccelerationOffset { get; set; } = 6.0F;

        private T _rangeMax;
        private T _rangeMin;
        private T _pulseIncrement;

        private Stopwatch debouncer = new Stopwatch();

        /// <summary>
        /// EventHandler to allow the notification of value changes.
        /// </summary>
        public event EventHandler<RotaryEncoderEventArgs<T>> ValueChanged;

        /// <summary>
        /// RotaryEncoder constructor
        /// </summary>
        /// <param name="controller">GpioController that hosts Pins A and B.</param>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="pulseIncrement">The amount that the value increases or decreases on each pulse from the rotary encoder</param>
        /// <param name="rangeMin">Minimum value permitted. The value is clamped to this.</param>
        /// <param name="rangeMax">Maximum value permitted. The value is clamped to this.</param>
        public RotaryEncoder(GpioController controller, int pinA, int pinB, T pulseIncrement, T rangeMin, T rangeMax)
        {
            Initialize(controller, pinA, pinB, pulseIncrement, rangeMin, rangeMax);
        }

        /// <summary>
        /// RotaryEncoder constructor
        /// </summary>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="pulseIncrement">The amount that the value increases or decreases on each pulse from the rotary encoder</param>
        /// <param name="rangeMin">Minimum value permitted. The value is clamped to this.</param>
        /// <param name="rangeMax">Maximum value permitted. The value is clamped to this.</param>
        public RotaryEncoder(int pinA, int pinB, T pulseIncrement, T rangeMin, T rangeMax) : this(new GpioController(), pinA, pinB, pulseIncrement, rangeMin, rangeMax) { }

        /// <summary>
        /// RotaryEncoder constructor for a 0..100 range with 100 steps
        /// </summary>
        /// <param name="controller">GpioController that hosts Pins A and B.</param>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        public RotaryEncoder(GpioController controller, int pinA, int pinB)
        {
            Initialize(controller, pinA, pinB, (dynamic)1, (dynamic)0, (dynamic)100);
        }

        /// <summary>
        /// Read the current Value
        /// </summary>
        /// <returns>The value associated with the rotary encoder.</returns>
        public T ReadValue() => Value;

        /// <summary>
        /// RotaryEncoder constructor for a 0..100 range with 100 steps
        /// </summary>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        public RotaryEncoder(int pinA, int pinB) : this(new GpioController(), pinA, pinB) { }

        /// <summary>
        /// Calculate the amount of acceleration to be applied to the increment of the rotary encoder.
        /// </summary>
        /// <remarks>
        /// This uses a straight line function output = input * AccelerationSlope + Acceleration offset but can be overridden 
        /// to perform different algorithms.
        /// </remarks>
        /// <param name="pulseWidthMilliSeconds">The amount of time elapsed since the last data pulse from the encoder in milliseconds.</param>
        /// <returns>A value that can be used to apply acceleration to the rotary encoder.</returns>
        protected virtual int Acceleration(int pulseWidthMilliSeconds)
        {
            // apply a straight line line function to the pulseWidth to determine the acceleration but clamp the lower value to 1
            return Math.Max(1, (int)(pulseWidthMilliSeconds * AccelerationSlope + AccelerationOffset));
        }

        /// <summary>
        /// Modify the current value on receipt of a pulse from the rotary encoder.
        /// </summary>
        /// <param name="blnUp">When true then the value should be incremented otherwise it should be decremented.</param>
        /// <param name="acceleration">A value that indicates how much acceleration should be applied to the pulse.</param>
        protected virtual void OnPulse(bool blnUp, int acceleration)
        {
            // calculate how much to change the value by
            dynamic valueChange = (blnUp ? (dynamic)_pulseIncrement : -(dynamic)_pulseIncrement) * acceleration;

            // set the value to the new value clamped by the maximum and minumum of the range.
            Value = Math.Max(Math.Min(Value + valueChange, (dynamic) _rangeMax), (dynamic) _rangeMin);

            // fire an event if an event handler has been attached
            if (ValueChanged != null)
            {
                ValueChanged.Invoke(this, new RotaryEncoderEventArgs<T>(Value));
            }
        }

        /// <summary>
        /// Initialize a RotaryEncoder
        /// </summary>
        /// <param name="controller">GpioController that hosts Pins A and B.</param>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="pulseIncrement">The amount that the value increases or decreases on each pulse from the rotary encoder</param>
        /// <param name="rangeMin">Minimum value permitted. The value is clamped to this.</param>
        /// <param name="rangeMax">Maximum value permitted. The value is clamped to this.</param>
        private void Initialize(GpioController controller, int pinA, int pinB, T pulseIncrement, T rangeMin, T rangeMax)
        {
            _controller = controller;
            _pinA = pinA;
            _pinB = pinB;
            _pulseIncrement = pulseIncrement;
            _rangeMin = rangeMin;
            _rangeMax = rangeMax;
           
            Value = rangeMin;

            _controller.OpenPin(_pinA, PinMode.Input);
            _controller.OpenPin(_pinB, PinMode.Input);

            debouncer.Start();

            _controller.RegisterCallbackForPinValueChangedEvent(_pinA, PinEventTypes.Falling, (o, e) =>
            {
                if (DebounceMilliseconds == 0 | debouncer.ElapsedMilliseconds > DebounceMilliseconds)
                {
                    OnPulse(_controller.Read(_pinA) == _controller.Read(_pinB), Acceleration((int) debouncer.ElapsedMilliseconds));
                }
                debouncer.Restart();
            });
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _controller.ClosePin(_pinA);
            _controller.ClosePin(_pinB);
        }
    }
}

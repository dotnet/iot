// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.RotaryEncoder
{
    /// <summary>
    /// Scaled Quadrature Rotary Controller binding
    /// </summary>
    /// <typeparam name="T">The type that represents the scaled valie</typeparam>
    public class ScaledQuadratureEncoder<T> : QuadratureRotaryEncoder where T : IComparable<T>
    {
        private T _rangeMax;
        private T _rangeMin;
        private T _pulseIncrement;

        /// <summary>The Value property represents current value associated with the RotaryEncoder.</summary>
        public T Value { get; set; }

        /// <summary>The AccelerationSlope property along with the AccelerationOffset property represents how the
        /// increase or decrease in value should grow as the incremental encoder is turned faster.</summary>
        public float AccelerationSlope { get; set; } = -0.05F;

        /// <summary>The AccelerationOffset property along with the AccelerationSlope property represents how the
        /// increase or decrease in value should grow as the incremental encoder is turned faster.</summary>
        public float AccelerationOffset { get; set; } = 6.0F;

        /// <summary>
        /// EventHandler to allow the notification of value changes.
        /// </summary>
        public event EventHandler<RotaryEncoderEventArgs<T>> ValueChanged;

        /// <summary>
        /// ScaledQuadratureEncoder constructor
        /// </summary>
        /// <param name="controller">GpioController that hosts Pins A and B.</param>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="edges">The pin event types to 'listen' for.</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        /// <param name="pulseIncrement">The amount that the value increases or decreases on each pulse from the rotary encoder</param>
        /// <param name="rangeMin">Minimum value permitted. The value is clamped to this.</param>
        /// <param name="rangeMax">Maximum value permitted. The value is clamped to this.</param>
        public ScaledQuadratureEncoder(GpioController controller, int pinA, int pinB, PinEventTypes edges, int pulsesPerRotation, T pulseIncrement, T rangeMin, T rangeMax) : base(controller, pinA, pinB, edges, pulsesPerRotation)
        {
            Initialize(pulseIncrement, rangeMin, rangeMax);
        }

        /// <summary>
        /// ScaledQuadratureEncoder constructor
        /// </summary>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="edges">The pin event types to 'listen' for.</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        /// <param name="pulseIncrement">The amount that the value increases or decreases on each pulse from the rotary encoder</param>
        /// <param name="rangeMin">Minimum value permitted. The value is clamped to this.</param>
        /// <param name="rangeMax">Maximum value permitted. The value is clamped to this.</param>       
        public ScaledQuadratureEncoder(int pinA, int pinB, PinEventTypes edges, int pulsesPerRotation, T pulseIncrement, T rangeMin, T rangeMax) : this(new GpioController(), pinA, pinB, edges, pulsesPerRotation, pulseIncrement, rangeMin, rangeMax) { }

        /// <summary>
        /// ScaledQuadratureEncoder constructor for a 0..100 range with 100 steps
        /// </summary>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="edges">The pin event types to 'listen' for.</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        public ScaledQuadratureEncoder(int pinA, int pinB, PinEventTypes edges, int pulsesPerRotation) : this(new GpioController(), pinA, pinB, edges, pulsesPerRotation) { }

        /// <summary>
        /// ScaledQuadratureEncoder constructor for a 0..100 range with 100 steps
        /// </summary>
        /// <param name="controller">GpioController that hosts Pins A and B.</param>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data</param>
        /// <param name="edges">The pin event types to 'listen' for.</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        public ScaledQuadratureEncoder(GpioController controller, int pinA, int pinB, PinEventTypes edges, int pulsesPerRotation) : base(controller, pinA, pinB, edges, pulsesPerRotation)
        {
            Initialize((dynamic)1, (dynamic)0, (dynamic)100);
        }

        /// <summary>
        /// Read the current Value
        /// </summary>
        /// <returns>The value associated with the rotary encoder.</returns>
        public T ReadValue() => Value;

        /// <summary>
        /// Calculate the amount of acceleration to be applied to the increment of the encoder.
        /// </summary>
        /// <remarks>
        /// This uses a straight line function output = input * AccelerationSlope + Acceleration offset but can be overridden 
        /// to perform different algorithms.
        /// </remarks>
        /// <param name="milliSecondsSinceLastPulse">The amount of time elapsed since the last data pulse from the encoder in milliseconds.</param>
        /// <returns>A value that can be used to apply acceleration to the rotary encoder.</returns>
        protected virtual int Acceleration(int milliSecondsSinceLastPulse)
        {
            // apply a straight line line function to the pulseWidth to determine the acceleration but clamp the lower value to 1
            return Math.Max(1, (int)(milliSecondsSinceLastPulse * AccelerationSlope + AccelerationOffset));
        }

        /// <summary>
        /// Modify the current value on receipt of a pulse from the encoder.
        /// </summary>
        /// <param name="blnUp">When true then the value should be incremented otherwise it should be decremented.</param>
        /// <param name="milliSecondsSinceLastPulse">The amount of time elapsed since the last data pulse from the encoder in milliseconds.</param>
        protected override void OnPulse(bool blnUp, int milliSecondsSinceLastPulse)
        {
            // call the OnPulse method on the base class to ensure the pulsecount is kept up to date
            base.OnPulse(blnUp, milliSecondsSinceLastPulse);

            // calculate how much to change the value by
            dynamic valueChange = (blnUp ? (dynamic)_pulseIncrement : -(dynamic)_pulseIncrement) * Acceleration(milliSecondsSinceLastPulse);

            // set the value to the new value clamped by the maximum and minumum of the range.
            Value = Math.Max(Math.Min(Value + valueChange, (dynamic)_rangeMax), (dynamic)_rangeMin);

            // fire an event if an event handler has been attached
            if (ValueChanged != null)
            {
                ValueChanged.Invoke(this, new RotaryEncoderEventArgs<T>(Value));
            }
        }

        /// <summary>
        /// Initialize a ScaledQuadratureEncoder
        /// </summary>
        /// <param name="pulseIncrement">The amount that the value increases or decreases on each pulse from the rotary encoder</param>
        /// <param name="rangeMin">Minimum value permitted. The value is clamped to this.</param>
        /// <param name="rangeMax">Maximum value permitted. The value is clamped to this.</param>
        private void Initialize(T pulseIncrement, T rangeMin, T rangeMax)
        {
            _pulseIncrement = pulseIncrement;
            _rangeMin = rangeMin;
            _rangeMax = rangeMax;

            Value = rangeMin;
        }
    }
}

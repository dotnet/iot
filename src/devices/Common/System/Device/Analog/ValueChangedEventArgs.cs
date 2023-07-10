// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace System.Device.Analog
{
    /// <summary>
    /// Arguments passed in when an event is triggered by the GPIO.
    /// </summary>
    public class ValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueChangedEventArgs"/> class.
        /// </summary>
        /// <param name="rawValue">The raw analog sensor reading</param>
        /// <param name="value">The analog sensor reading, converted to voltage.</param>
        /// <param name="pinNumber">The pin number that triggered the event.</param>
        /// <param name="triggerReason">The reason for the event</param>
        public ValueChangedEventArgs(uint rawValue, ElectricPotential value, int pinNumber, TriggerReason triggerReason)
        {
            RawValue = rawValue;
            Value = value;
            PinNumber = pinNumber;
            TriggerReason = triggerReason;
        }

        /// <summary>
        /// Raw value, unscaled.
        /// </summary>
        public uint RawValue
        {
            get;
        }

        /// <summary>
        /// The absolute voltage of the new value.
        /// For this to be valid, the voltage reference of the <see cref="AnalogController"/> must be set correctly.
        /// </summary>
        public ElectricPotential Value
        {
            get;
        }

        /// <summary>
        /// The physical pin number that triggered the event.
        /// </summary>
        public int PinNumber
        {
            get;
        }

        /// <summary>
        /// The reason that triggered this message.
        /// </summary>
        public TriggerReason TriggerReason { get; }
    }
}

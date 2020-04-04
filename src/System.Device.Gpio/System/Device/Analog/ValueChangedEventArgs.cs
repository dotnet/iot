// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        /// <param name="newValue">The analog sensor reading, converted to voltage.</param>
        /// <param name="pinNumber">The pin number that triggered the event.</param>
        /// <param name="triggerReason">The reason for the event</param>
        public ValueChangedEventArgs(uint rawValue, double newValue, int pinNumber, TriggerReason triggerReason)
        {
            RawValue = rawValue;
            Value = newValue;
            PinNumber = pinNumber;
            TriggerReason = triggerReason;
        }

        public uint RawValue
        {
            get;
        }

        /// <summary>
        /// The change type that triggered the event.
        /// </summary>
        public double Value
        {
            get;
        }

        /// <summary>
        /// The pin number that triggered the event.
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

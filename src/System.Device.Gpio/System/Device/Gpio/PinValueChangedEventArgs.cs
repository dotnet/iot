// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    /// <summary>
    /// Arguments passed in when an event is triggered by the GPIO.
    /// </summary>
    public class PinValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinValueChangedEventArgs"/> class.
        /// </summary>
        /// <param name="changeType">The change type that triggered the event.</param>
        /// <param name="pinNumber">The pin number that triggered the event.</param>
        public PinValueChangedEventArgs(PinEventTypes changeType, int pinNumber)
        {
            ChangeType = changeType;
            PinNumber = pinNumber;
        }

        /// <summary>
        /// The change type that triggered the event.
        /// </summary>
        public PinEventTypes ChangeType { get; }
        /// <summary>
        /// The pin number that triggered the event.
        /// </summary>
        public int PinNumber { get; }
    }
}

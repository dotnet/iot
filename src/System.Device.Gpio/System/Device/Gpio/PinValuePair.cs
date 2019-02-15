// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    /// <summary>
    /// Simple wrapper for a pin/value pair.
    /// </summary>
    public readonly struct PinValuePair
    {
        /// <summary>
        /// The pin number.
        /// </summary>
        public int PinNumber { get; }

        /// <summary>
        /// The pin value.
        /// </summary>
        public PinValue PinValue { get; }

        public PinValuePair(int pin, PinValue value)
        {
            PinNumber = pin;
            PinValue = value;
        }

        /// <summary>
        /// Deconstructor for convenience. Allows using as a "return Tuple".
        /// </summary>
        public void Deconstruct(out int pinNumber, out PinValue pinValue)
        {
            pinNumber = PinNumber;
            pinValue = PinValue;
        }
    }
}

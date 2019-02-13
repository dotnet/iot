// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    /// <summary>
    /// Simple wrapper for a pin/value pair
    /// </summary>
    public readonly struct PinValuePair
    {
        public int PinNumber { get; }
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

        /// <summary>
        /// Returns a bit mask of designated pins. Supports pin numbers from 0 to 31.
        /// </summary>
        /// <remarks>
        /// On some devices (such as GPIO extenders) pin numbers are low and often consecutive. This
        /// is a helper to turn sets of pins into bits/bitmasks for storage and/or sending over the
        /// wire to devices that want pin numbers in a bit format.
        /// </remarks>
        /// <param name="pinValues">A set of pin value pairs.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if given a negative pin or pin number of 32 or higher.
        /// </exception>
        /// <returns>Bit mask of designated pins and actual values. "1" means the pin for that bit position was selected.</returns>
        public static (uint PinMask, uint Values) ToBits(ReadOnlySpan<PinValuePair> pinValues)
        {
            uint pinMask = 0;
            uint values = 0;
            foreach ((int pin, PinValue value) in pinValues)
            {
                if (pin < 0 || pin >= sizeof(uint) * 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(pinValues));
                }
                uint bit = (uint)(1 << pin);
                pinMask |= bit;
                if (value == PinValue.High)
                {
                    values |= bit;
                }
            }
            return (pinMask, values);
        }
    }
}

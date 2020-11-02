// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio
{
    /// <summary>
    /// 64-bit vector of pins and values.
    /// </summary>
    internal struct PinVector64
    {
        /// <summary>
        /// Bit vector of pin numbers from 0 (bit 0) to 63 (bit 63).
        /// </summary>
        public ulong Pins { get; set; }

        /// <summary>
        /// Bit vector of values for each pin number from 0 (bit 0) to 63 (bit 63).
        /// 1 is high, 0 is low.
        /// </summary>
        public ulong Values { get; set; }

        /// <summary>
        /// Construct from a vector of pins and values.
        /// </summary>
        /// <param name="pins">Bit vector of pin numbers from 0 (bit 0) to 63 (bit 63).</param>
        /// <param name="values">Bit vector of values for each pin number from 0 (bit 0) to 63 (bit 63).</param>
        public PinVector64(ulong pins, ulong values)
        {
            Pins = pins;
            Values = values;
        }

        /// <summary>
        /// Construct from a span of <see cref="PinValuePair"/>s.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="pinValues"/> contains negative pin numbers or pin numbers higher than 63.
        /// </exception>
        public PinVector64(ReadOnlySpan<PinValuePair> pinValues)
        {
            Pins = 0;
            Values = 0;

            foreach ((int pin, PinValue value) in pinValues)
            {
                if (pin < 0 || pin >= sizeof(ulong) * 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(pinValues));
                }

                ulong bit = (ulong)(1 << pin);
                Pins |= bit;
                if (value == PinValue.High)
                {
                    Values |= bit;
                }
            }
        }

        /// <summary>
        /// Convenience deconstructor. Allows using as a "return tuple".
        /// </summary>
        public void Deconstruct(out ulong pins, out ulong values)
        {
            pins = Pins;
            values = Values;
        }
    }
}

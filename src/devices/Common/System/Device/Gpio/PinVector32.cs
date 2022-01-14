// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio
{
    /// <summary>
    /// 32-bit vector of pins and values.
    /// </summary>
    partial struct PinVector32
    {
        /// <summary>
        /// Bit vector of pin numbers from 0 (bit 0) to 31 (bit 31).
        /// </summary>
        public uint Pins { get; set; }

        /// <summary>
        /// Bit vector of values for each pin number from 0 (bit 0) to 31 (bit 31).
        /// 1 is high, 0 is low.
        /// </summary>
        public uint Values { get; set; }

        /// <summary>
        /// Construct from a vector of pins and values.
        /// </summary>
        /// <param name="pins">Bit vector of pin numbers from 0 (bit 0) to 31 (bit 31).</param>
        /// <param name="values">Bit vector of values for each pin number from 0 (bit 0) to 31 (bit 31).</param>
        public PinVector32(uint pins, uint values)
        {
            Pins = pins;
            Values = values;
        }

        /// <summary>
        /// Construct from a span of <see cref="PinValuePair"/>s.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="pinValues"/> contains negative pin numbers or pin numbers higher than 31.
        /// </exception>
        public PinVector32(ReadOnlySpan<PinValuePair> pinValues)
        {
            Pins = 0;
            Values = 0;

            foreach ((int pin, PinValue value) in pinValues)
            {
                if (pin < 0 || pin >= sizeof(uint) * 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(pinValues));
                }

                uint bit = (uint)(1 << pin);
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
        public void Deconstruct(out uint pins, out uint values)
        {
            pins = Pins;
            values = Values;
        }
    }
}

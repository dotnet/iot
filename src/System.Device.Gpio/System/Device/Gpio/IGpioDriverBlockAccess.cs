// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio
{
    /// <summary>
    /// Interface to mark drivers that support block access to GPIO pins.
    /// Block access in this case meaning the ability to read/write multiple pins in one call.
    /// </summary>
    public interface IGpioDriverBlockAccess
    {
        /// <summary>
        /// Write the given pins with the given values.
        /// </summary>
        /// <param name="pinValuePairs">The pin/value pairs to write.</param>
        public void Write(ReadOnlySpan<PinValuePair> pinValuePairs);

        /// <summary>
        /// Read the given pins with the given pin numbers.
        /// </summary>
        /// <param name="pinValuePairs">The pin/value pairs to read.</param>
        public void Read(Span<PinValuePair> pinValuePairs);
    }
}

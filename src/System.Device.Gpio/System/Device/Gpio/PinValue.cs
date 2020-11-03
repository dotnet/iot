// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    /// <summary>
    /// Represents a value for a pin.
    /// </summary>
    public readonly struct PinValue : IEquatable<PinValue>
    {
        // This isn't bool so the struct will retain blittable. This
        // allows arrays of PinValue and structs that contain PinValue to
        // be stack allocated.
        private readonly byte _value;

        private PinValue(byte value) => _value = value;

        /// <summary>
        /// The value of the pin is high.
        /// </summary>
        public static PinValue High => new PinValue(1);

        /// <summary>
        /// The value of the pin is low.
        /// </summary>
        public static PinValue Low => new PinValue(0);

        /// <summary>
        /// Implicit conversion from int. 0 means <see cref="Low"/>, everything else means <see cref="High"/>.
        /// </summary>
        /// <param name="value">Value to set</param>
        public static implicit operator PinValue(int value) => value == 0 ? Low : High;

        /// <summary>
        /// Implicit conversion from bool. <see langword="false"/> means <see cref="Low"/>, <see langword="true"/> means <see cref="High"/>
        /// </summary>
        /// <param name="value">Value to set</param>
        public static implicit operator PinValue(bool value) => value ? High : Low;

        /// <summary>
        /// Conversion to byte. Returns 1 on <see cref="High"/>, 0 on <see cref="Low"/>
        /// </summary>
        /// <param name="value">PinValue to convert</param>
        public static explicit operator byte(PinValue value) => value._value;

        /// <summary>
        /// Conversion to int. Returns 1 on <see cref="High"/>, 0 on <see cref="Low"/>
        /// </summary>
        /// <param name="value">PinValue to convert</param>
        public static explicit operator int(PinValue value) => value._value;

        /// <summary>
        /// Conversion to byte. Returns <see langword="true"/> on <see cref="High"/>, <see langword="false"/> on <see cref="Low"/>
        /// </summary>
        /// <param name="value">PinValue to convert</param>
        public static explicit operator bool(PinValue value) => value._value == 0 ? false : true;

        /// <summary>
        /// Returns true if the other instance represents the same <see cref="PinValue"/> than this.
        /// </summary>
        public bool Equals(PinValue other) => other._value == _value;

        /// <inheritdoc cref="ValueType.Equals(object)"/>
        public override bool Equals(object? obj)
        {
            if (obj is PinValue)
            {
                return Equals((PinValue)obj);
            }

            return false;
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(PinValue a, PinValue b) => a.Equals(b);

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(PinValue a, PinValue b) => !a.Equals(b);

        /// <summary>
        /// Returns the inverse of the value
        /// </summary>
        /// <param name="v">Input value</param>
        /// <returns>High, when the input is low, low when the input is high</returns>
        public static PinValue operator !(PinValue v) => (v._value == 0) ? High : Low;

        /// <inheritdoc />
        public override int GetHashCode() => _value.GetHashCode();

        /// <summary>
        /// Returns "Low" for Low and "High" for High
        /// </summary>
        public override string ToString() => _value == 0 ? "Low" : "High";
    }
}

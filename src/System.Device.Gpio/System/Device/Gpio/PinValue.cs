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

        public static implicit operator PinValue(int value) => value == 0 ? Low : High;
        public static implicit operator PinValue(bool value) => value ? High : Low;
        public static explicit operator byte(PinValue value) => value._value;
        public static explicit operator int(PinValue value) => value._value;
        public static explicit operator bool(PinValue value) => value._value == 0 ? false : true;

        public bool Equals(PinValue other) => other._value == _value;
        public override bool Equals(object obj)
        {
            if (obj is PinValue)
            {
                return Equals((PinValue)obj);
            }
            return false;
        }

        public static bool operator ==(PinValue a, PinValue b) => a.Equals(b);
        public static bool operator !=(PinValue a, PinValue b) => !a.Equals(b);
        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value == 0 ? "Low" : "High";
    }
}

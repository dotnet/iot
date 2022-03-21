// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Nmea0183
{
    public partial class Identification : IEquatable<Identification>
    {
        /// <inheritdoc />
        public bool Equals(Identification? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return shipNameField == other.shipNameField
                   && callsignField == other.callsignField
                   && mMSIField == other.mMSIField
                   && calibrationDateField.Equals(other.calibrationDateField);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals((Identification)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(shipNameField, callsignField, mMSIField, calibrationDateField);
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(Identification? left, Identification? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(Identification? left, Identification? right)
        {
            return !Equals(left, right);
        }
    }
}

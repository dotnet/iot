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
            unchecked
            {
                var hashCode = (shipNameField != null ? shipNameField.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (callsignField != null ? callsignField.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (mMSIField != null ? mMSIField.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ calibrationDateField.GetHashCode();
                return hashCode;
            }
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

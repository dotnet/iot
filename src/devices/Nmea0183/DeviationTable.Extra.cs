// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Iot.Device.Nmea0183
{
    public partial class DeviationPoint : IEquatable<DeviationPoint>
    {
        private const float AngleEpsilon = 1E8f;

        /// <summary>
        /// Generates a string representation of this object
        /// </summary>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Heading {0}, Deviation {1}", MagneticHeading, Deviation);
        }

        /// <inheritdoc />
        public bool Equals(DeviationPoint? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Math.Abs(compassReadingField - other.compassReadingField) < AngleEpsilon
                   && Math.Abs(compassReadingSmoothField - other.compassReadingSmoothField) < AngleEpsilon
                   && Math.Abs(deviationField - other.deviationField) < AngleEpsilon
                   && Math.Abs(deviationSmoothField - other.deviationSmoothField) < AngleEpsilon;
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((DeviationPoint)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(compassReadingField, compassReadingSmoothField, magneticHeadingField, deviationField, deviationSmoothField);
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(DeviationPoint? left, DeviationPoint? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(DeviationPoint? left, DeviationPoint? right)
        {
            return !Equals(left, right);
        }
    }
}

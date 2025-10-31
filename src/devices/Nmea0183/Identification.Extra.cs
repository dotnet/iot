// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Identifies a ship or radio station by its name, callsign or MMSI
    /// </summary>
    public partial class Identification : IEquatable<Identification>
    {
        /// <summary>
        /// Create an empty instance of this class
        /// </summary>
        internal Identification()
        {
            shipNameField = string.Empty;
            callsignField = string.Empty;
            mMSIField = string.Empty;
            calibrationDateField = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates an identifier for a ship
        /// </summary>
        /// <param name="shipName">The name of the vessel</param>
        /// <param name="callsign">The callsign (should only be letters and digits)</param>
        /// <param name="Mmsi">The MMSI (9 digits)</param>
        /// <param name="date">A date associated with the ship (meaning depends on context)</param>
        public Identification(string shipName, string callsign, string Mmsi, DateTime date)
        {
            ShipName = shipName;
            Callsign = callsign;
            MMSI = Mmsi;
            CalibrationDate = date;
        }

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
                   && calibrationDateField.Date.Equals(other.calibrationDateField.Date);
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

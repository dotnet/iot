// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Geo Record geo:latitude,longitude where both latitude and longitude are double with . for decimal point
    /// </summary>
    public class GeoRecord : UriRecord
    {
        private double _latitude;
        private double _longitude;

        /// <summary>
        /// The latitude
        /// </summary>
        public double Latitude
        {
            get
            {
                return _latitude;
            }

            set
            {
                _latitude = value;
                SetPayload();
            }
        }

        /// <summary>
        /// The longitude
        /// </summary>
        public double Longitude
        {
            get
            {
                return _longitude;
            }

            set
            {
                _longitude = value;
                SetPayload();
            }
        }

        /// <summary>
        /// Create a Geo Record from a latitude and a longitude
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        public GeoRecord(double latitude, double longitude)
            : base(UriType.NoFormat, string.Empty)
        {
            _latitude = latitude;
            _longitude = longitude;
            SetPayload();
        }

        /// <summary>
        /// Create a Geo Record from a NDEF record
        /// </summary>
        /// <param name="ndefRecord">The NDEF record to create the Geo Record</param>
        public GeoRecord(NdefRecord ndefRecord)
            : base(ndefRecord)
        {
            ExtractAll();
        }

        /// <summary>
        /// Create a Geo Record from a span of bytes
        /// </summary>
        /// <param name="record">A raw span of byte containing the Geo Record</param>
        public GeoRecord(ReadOnlySpan<byte> record)
            : base(record)
        {
            ExtractAll();
        }

        private void ExtractAll()
        {
            if (!IsGeoRecord(this))
            {
                throw new ArgumentException($"Record is not a valid {nameof(GeoRecord)}, they should start with 'geo:'.");
            }

            var strLatLong = Uri.Substring(4).Split(',');
            if (strLatLong.Length != 2)
            {
                throw new ArgumentException($"Record is not a valid {nameof(GeoRecord)}, can't find a proper latitude and longitude in the payload");
            }

            try
            {
                _latitude = Convert.ToDouble(strLatLong[0], CultureInfo.InvariantCulture);
                _longitude = Convert.ToDouble(strLatLong[1], CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                throw new ArgumentException($"Record is not a valid {nameof(GeoRecord)}, can't find a proper latitude and longitude in the payload");
            }
        }

        /// <summary>
        /// Check if it's a valid NDEF Geo Record
        /// </summary>
        /// <param name="ndefRecord">A valid NDEF Record</param>
        /// <returns></returns>
        public static bool IsGeoRecord(NdefRecord ndefRecord)
        {
            if (!IsUriRecord(ndefRecord))
            {
                return false;
            }

            var uri = new UriRecord(ndefRecord);

            if (!uri.Uri.ToLower().StartsWith("geo:"))
            {
                return false;
            }

            return true;
        }

        private void SetPayload()
        {
            Uri = $"geo:{_latitude.ToString(CultureInfo.InvariantCulture)},{_longitude.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}

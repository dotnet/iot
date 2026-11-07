// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// The NMEA2000 Fast Position Update message. Includes latitude and longitude only.
    /// </summary>
    public class FastPositionUpdate : Nmea2000PackedMessage
    {
        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0x01F801;

        private readonly double _latitude;
        private readonly double _longitude;

        /// <summary>
        /// Create an instance of this class from a position
        /// </summary>
        public FastPositionUpdate(GeographicPosition position)
        {
            _latitude = position.Latitude;
            _longitude = position.Longitude;
            Valid = true;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public FastPositionUpdate(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public FastPositionUpdate(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadSignedFromHexString(data, 0, 8, true, out int latitude))
            {
                _latitude = latitude * 1E-7;
            }

            if (ReadSignedFromHexString(data, 8, 8, true, out int longitude))
            {
                _longitude = longitude * 1E-7;
            }

            _latitude = Math.Clamp(_latitude, -90, 90);
            _longitude = Math.Clamp(_longitude, -360, 360);

            Valid = true;
        }

        /// <summary>
        /// The latitude in this message
        /// </summary>
        public double Latitude => _latitude;

        /// <summary>
        /// The longitude in this message
        /// </summary>
        public double Longitude => _longitude;

        /// <inheritdoc/>
        public override uint Identifier => HexId;

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

        /// <inheritdoc/>
        public override string ToNmeaParameterList()
        {
            string lat = InverseEndianness((int)Math.Round(_latitude / 1E-7)).ToString("X8", CultureInfo.InvariantCulture);
            string lon = InverseEndianness((int)Math.Round(_longitude / 1E-7)).ToString("X8", CultureInfo.InvariantCulture);
            return base.ToNmeaParameterList() + lat + lon;
        }

        /// <inheritdoc/>
        public override string ToReadableContent()
        {
            return new GeographicPosition(_latitude, _longitude, 0).ToString();
        }
    }
}

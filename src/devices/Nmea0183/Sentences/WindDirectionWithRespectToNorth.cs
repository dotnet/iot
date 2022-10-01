// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// The MWD sequence gives the wind direction and speed in true directions. That means the direction is given in
    /// degrees from true north and the speed is the true wind speed. Calculating the input data for this sentence
    /// requires heading or, as a backup, COG. However, with COG, the data is unreliable when the ship is moored.
    /// See <see cref="WindSpeedAndAngle"/> for relative wind directions (as measured by the wind instrument)
    /// </summary>
    public class WindDirectionWithRespectToNorth : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("MWD");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new MDA sentence
        /// </summary>
        /// <param name="trueWindDirection">Wind direction relative to true north</param>
        /// <param name="magneticWindDirection">Wind direction relative to magnetic north</param>
        /// <param name="windSpeed">Wind speed</param>
        public WindDirectionWithRespectToNorth(Angle? trueWindDirection, Angle? magneticWindDirection, Speed? windSpeed)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            TrueWindDirection = trueWindDirection;
            MagneticWindDirection = magneticWindDirection;
            WindSpeed = windSpeed;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public WindDirectionWithRespectToNorth(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Decode a message
        /// </summary>
        public WindDirectionWithRespectToNorth(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? windDirectionTrue = ReadValue(field);
            string referenceT = ReadString(field) ?? string.Empty;
            double? windDirectionMagnetic = ReadValue(field);
            string referenceM = ReadString(field) ?? string.Empty;
            double? windSpeedKnots = ReadValue(field);
            string referenceN = ReadString(field) ?? string.Empty;
            double? windSpeedMetersPerSecond = ReadValue(field);
            string referenceMeters = ReadString(field) ?? string.Empty;

            if (windDirectionTrue.HasValue && referenceT == "T")
            {
                TrueWindDirection = Angle.FromDegrees(windDirectionTrue.Value);
            }

            if (windDirectionMagnetic.HasValue && referenceM == "M")
            {
                MagneticWindDirection = Angle.FromDegrees(windDirectionMagnetic.Value);
            }

            if (windSpeedKnots.HasValue && referenceN == "N")
            {
                WindSpeed = Speed.FromKnots(windSpeedKnots.Value);
            }
            else if (windSpeedMetersPerSecond.HasValue && referenceMeters == "M")
            {
                WindSpeed = Speed.FromMetersPerSecond(windSpeedMetersPerSecond.Value);
            }

            Valid = true;
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Wind direction, in degrees true, from north.
        /// </summary>
        public Angle? TrueWindDirection { get; }

        /// <summary>
        /// Wind direction, in degrees magnetic, from north
        /// </summary>
        public Angle? MagneticWindDirection { get; }

        /// <summary>
        /// Wind Speed, true
        /// </summary>
        public Speed? WindSpeed { get; }

        /// <summary>
        /// Presents this message as output
        /// </summary>
        public override string ToNmeaParameterList()
        {
            if (Valid)
            {
                string windDirectionTrue = TrueWindDirection.HasValue
                    ? TrueWindDirection.Value.Degrees.ToString("F1", CultureInfo.InvariantCulture)
                    : string.Empty;
                string windDirectionMagnetic = MagneticWindDirection.HasValue
                    ? MagneticWindDirection.Value.Degrees.ToString("F1", CultureInfo.InvariantCulture)
                    : string.Empty;
                string speedKnots = WindSpeed.HasValue
                    ? WindSpeed.Value.Knots.ToString("F1", CultureInfo.InvariantCulture)
                    : string.Empty;
                string speedMs = WindSpeed.HasValue
                    ? WindSpeed.Value.MetersPerSecond.ToString("F1", CultureInfo.InvariantCulture)
                    : string.Empty;
                return FormattableString.Invariant($"{windDirectionTrue},T,{windDirectionMagnetic},M,{speedKnots},N,{speedMs},M");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Wind Direction True: {TrueWindDirection.GetValueOrDefault(Angle.Zero).As(AngleUnit.Degree)}, " +
                       $"Wind speed: {WindSpeed.GetValueOrDefault(Speed.Zero)}";
            }

            return "No valid data";
        }
    }
}

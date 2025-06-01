// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// MWV sentence: Wind speed and wind angle (true or apparent)
    /// Note that the wind angle is always given relative to the ship's bow, so to get the wind direction
    /// in cardinal direction, the heading is required (or with some error, COG can be used).
    /// See <see cref="WindDirectionWithRespectToNorth"/> for geographic wind direction.
    /// </summary>
    public class WindSpeedAndAngle : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("MWV");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new MWV sentence
        /// </summary>
        public WindSpeedAndAngle(Angle angle, Speed speed, bool relative)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            Speed = speed;
            Relative = relative;
            Angle = angle;
            Valid = true;
            if (Speed.Unit == UnitsNet.Units.SpeedUnit.Knot)
            {
                SpeedUnit = "N";
            }
            else
            {
                SpeedUnit = "M";
            }
        }

        /// <summary>
        /// Sent twice: With true and with apparent wind
        /// </summary>
        public override bool ReplacesOlderInstance => false;

        /// <summary>
        /// Internal constructor
        /// </summary>
        public WindSpeedAndAngle(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public WindSpeedAndAngle(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? angle = ReadValue(field);
            string reference = ReadString(field) ?? string.Empty;
            double? speed = ReadValue(field);

            string unit = ReadString(field) ?? string.Empty;
            string status = ReadString(field) ?? string.Empty;

            if (status == "A" && angle.HasValue && speed.HasValue)
            {
                Angle = Angle.FromDegrees(angle.Value);
                if (unit == "N")
                {
                    Speed = Speed.FromKnots(speed.Value);
                    SpeedUnit = unit;
                }
                else if (unit == "M")
                {
                    Speed = Speed.FromMetersPerSecond(speed.Value);
                    SpeedUnit = unit;
                }
                else
                {
                    Speed = Speed.FromMetersPerSecond(0);
                    SpeedUnit = "M";
                }

                if (reference == "T")
                {
                    Relative = false;
                    Angle = Angle.Normalize(true);
                }
                else
                {
                    // Default, since that's what the actual wind instrument delivers
                    Relative = true;
                    // Relative angles are +/- 180
                    Angle = Angle.Normalize(false);
                }

                Valid = true;
            }
            else
            {
                Angle = Angle.Zero;
                Speed = Speed.Zero;
                SpeedUnit = string.Empty;
                Valid = false;
            }
        }

        /// <summary>
        /// Angle of the wind
        /// </summary>
        public Angle Angle
        {
            get;
            private set;
        }

        /// <summary>
        /// Wind speed
        /// </summary>
        public Speed Speed
        {
            get;
            private set;
        }

        /// <summary>
        /// True if the values are relative, false if they are absolute
        /// </summary>
        public bool Relative
        {
            get;
            private set;
        }

        /// <summary>
        /// Unit of speed: "M" for m/s, "N" for knots.
        /// The same message is sometimes sent with both units, so we might want to keep the difference.
        /// </summary>
        public string SpeedUnit
        {
            get;
            private set;
        }

        /// <summary>
        /// Presents this message as output
        /// </summary>
        public override string ToNmeaParameterList()
        {
            if (Valid)
            {
                // It seems that angles should always be written 0..360.
                var normalized = Angle.Normalize(true);
                if (SpeedUnit == "N")
                {
                    return FormattableString.Invariant(
                        $"{normalized.Degrees:F1},{(Relative ? "R" : "T")},{Speed.Knots:F1},N,A");
                }
                else
                {
                    return FormattableString.Invariant(
                        $"{normalized.Degrees:F1},{(Relative ? "R" : "T")},{Speed.MetersPerSecond:F1},M,A");
                }
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                if (SpeedUnit == "N")
                {
                    if (Relative)
                    {
                        return $"Apparent wind direction: {Angle.Degrees:F1}° Speed: {Speed.Knots:F1}kts";
                    }
                    else
                    {
                        return $"Absolute wind direction: {Angle.Degrees:F1}° Speed: {Speed.Knots:F1}kts";
                    }
                }
                else
                {
                    if (Relative)
                    {
                        return $"Apparent wind direction: {Angle.Degrees:F1}° Speed: {Speed.MetersPerSecond:F1}m/s";
                    }
                    else
                    {
                        return $"Absolute wind direction: {Angle.Degrees:F1}° Speed: {Speed.MetersPerSecond:F1}m/s";
                    }
                }
            }

            return "Wind speed/direction unknown";
        }
    }
}

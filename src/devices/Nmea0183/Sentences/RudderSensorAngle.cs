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
    /// Rudder angle from rudder angle sensor (if fitted)
    /// </summary>
    public class RudderSensorAngle : NmeaSentence
    {
        /// <summary>
        /// This sentence ID "RSA"
        /// </summary>
        public static SentenceId Id => new SentenceId("RSA");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new RSA sentence
        /// </summary>
        /// <param name="starboard">Starboard or single rudder angle (0 = centered)</param>
        /// <param name="port">Port rudder angle, if fitted</param>
        public RudderSensorAngle(Angle starboard, Angle? port)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            Starboard = starboard;
            Port = port;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public RudderSensorAngle(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public RudderSensorAngle(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? starboard = ReadValue(field);
            string valid = ReadString(field);
            if (starboard.HasValue && valid == "A")
            {
                Starboard = Angle.FromDegrees(starboard.Value);
                Valid = true; // The message is only valid if there's at least a stb sensor.
            }

            double? port = ReadValue(field);
            valid = ReadString(field);
            if (port.HasValue && valid == "A")
            {
                Port = Angle.FromDegrees(port.Value);
            }
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Starboard or single rudder angle 0 = center, negative for turning to port. A negative rudder angle will thus reduce the heading.
        /// </summary>
        public Angle Starboard
        {
            get;
            private set;
        }

        /// <summary>
        /// Port rudder angle. Null if not fitted (most boats will only have one rudder sensor, if at all)
        /// </summary>
        public Angle? Port
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
                StringBuilder sb = new StringBuilder();
                sb.Append(Starboard.Degrees.ToString("F1", CultureInfo.InvariantCulture) + ",A,");
                if (Port.HasValue)
                {
                    sb.Append(Port.Value.Degrees.ToString("F1", CultureInfo.InvariantCulture) + ",A");
                }
                else
                {
                    sb.Append(",V");
                }

                return sb.ToString();
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Rudder Angle: {Starboard.Normalize(false).Degrees:F1}°";
            }

            return "No rudder sensor";
        }
    }
}

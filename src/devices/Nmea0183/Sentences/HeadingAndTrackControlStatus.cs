// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// This is the status reply from the autopilot. It is similar to the HTC message <see cref="HeadingAndTrackControl"/> with 4 extra fields.
    /// </summary>
    public class HeadingAndTrackControlStatus : HeadingAndTrackControl
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static new SentenceId Id => new SentenceId("HTD");

        private static bool Matches(SentenceId sentence) => Id == sentence;

        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Initialize a new instance of this type.
        /// See the individual properties for further explanation to the parameters.
        /// </summary>
        /// <param name="status">Status of Autopilot</param>
        /// <param name="commandedRudderAngle">Commanded rudder angle</param>
        /// <param name="commandedRudderDirection">Rudder direction</param>
        /// <param name="turnMode">Turn mode</param>
        /// <param name="rudderLimit">Rudder limit</param>
        /// <param name="offHeadingLimit">Off-Heading limit</param>
        /// <param name="turnRadius">Desired turn radius</param>
        /// <param name="rateOfTurn">Desired rate of turn</param>
        /// <param name="desiredHeading">Desired heading</param>
        /// <param name="offTrackLimit">Desired off-track limit</param>
        /// <param name="commandedTrack">Commanded track direction</param>
        /// <param name="headingIsTrue">Heading uses true angles</param>
        /// <param name="rudderLimitExceeded">True if rudder limit is exceeded</param>
        /// <param name="headingLimitExceeded">True if the heading is out of limits</param>
        /// <param name="trackLimitExceeded">True if the track limits are exceeded</param>
        /// <param name="actualHeading">Heading from the Autopilot's onw heading sensor</param>
        public HeadingAndTrackControlStatus(string status, Angle? commandedRudderAngle, string commandedRudderDirection, string turnMode,
            Angle? rudderLimit, Angle? offHeadingLimit, Length? turnRadius, RotationalSpeed? rateOfTurn, Angle? desiredHeading,
            Length? offTrackLimit, Angle? commandedTrack, bool headingIsTrue,
            bool rudderLimitExceeded, bool headingLimitExceeded, bool trackLimitExceeded, Angle? actualHeading)
            : base(status, commandedRudderAngle, commandedRudderDirection, turnMode, rudderLimit, offHeadingLimit, turnRadius,
                rateOfTurn, desiredHeading, offTrackLimit, commandedTrack, headingIsTrue)
        {
            RudderLimitExceeded = rudderLimitExceeded;
            HeadingLimitExceeded = headingLimitExceeded;
            TrackLimitExceeded = trackLimitExceeded;
            ActualHeading = actualHeading;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public HeadingAndTrackControlStatus(TalkerSentence sentence, DateTimeOffset time)
            : base(sentence, time)
        {
        }

        /// <summary>
        /// Decoding constructor
        /// </summary>
        public HeadingAndTrackControlStatus(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, fields, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();
            for (int i = 0; i < 13; i++)
            {
                field.MoveNext();
            }

            RudderLimitExceeded = ReadString(field) == "V";
            HeadingLimitExceeded = ReadString(field) == "V";
            TrackLimitExceeded = ReadString(field) == "V";
            ActualHeading = AsAngle(ReadValue(field));
        }

        /// <summary>
        /// True if the rudder limit is exceeded
        /// </summary>
        public bool RudderLimitExceeded { get; }

        /// <summary>
        /// True if the heading limit is exceeded
        /// </summary>
        public bool HeadingLimitExceeded { get; }

        /// <summary>
        /// True if the track limit is exceeded
        /// </summary>
        public bool TrackLimitExceeded { get; }

        /// <summary>
        /// The actual heading, from the Autopilot's own heading sensor
        /// </summary>
        public Angle? ActualHeading { get; }

        /// <inheritdoc />
        public override string ToNmeaParameterList()
        {
            string ret = base.ToNmeaParameterList();
            var angleString = FromAngle(ActualHeading).Replace(",", string.Empty); // the last comma is not needed
            return ret + FormattableString.Invariant($",{(RudderLimitExceeded ? "V" : "A")},{(HeadingLimitExceeded ? "V" : "A")},{(TrackLimitExceeded ? "V" : "A")},{angleString}");
        }
    }
}

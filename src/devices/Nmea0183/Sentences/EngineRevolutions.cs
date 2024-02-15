// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Engine revolutions (RPM) sentence.
    /// Note: This is superseeded by NMEA2000 commands, which provide a lot more details for engine parameters
    /// </summary>
    public class EngineRevolutions : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("RPM");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new RPM sentence
        /// </summary>
        /// <param name="source">RPM source, typically <see cref="RotationSource.Engine"/></param>
        /// <param name="speed">Engine rotational speed</param>
        /// <param name="engineNumber">Engine Number, counting from 1</param>
        /// <param name="pitch">Propeller pitch, if applicable</param>
        public EngineRevolutions(RotationSource source, RotationalSpeed speed, int engineNumber, Ratio pitch)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            RotationalSpeed = speed;
            RotationSource = source;
            EngineNumber = engineNumber;
            PropellerPitch = pitch;
            Valid = true;
        }

        /// <summary>
        /// Constructs a new RPM sentence
        /// </summary>
        /// <param name="talker">Custom talker Id</param>
        /// <param name="source">RPM source, typically <see cref="RotationSource.Engine"/></param>
        /// <param name="speed">Engine rotational speed</param>
        /// <param name="engineNumber">Engine Number, counting from 1</param>
        /// <param name="pitch">Propeller pitch, if applicable</param>
        public EngineRevolutions(TalkerId talker, RotationSource source, RotationalSpeed speed, int engineNumber, Ratio pitch)
            : base(talker, Id, DateTimeOffset.UtcNow)
        {
            RotationalSpeed = speed;
            RotationSource = source;
            EngineNumber = engineNumber;
            PropellerPitch = pitch;
            Valid = true;
        }

        /// <summary>
        /// Constructs an RPM sentence from an <see cref="EngineData"/> instance
        /// </summary>
        /// <param name="talker">Custom talker Id</param>
        /// <param name="engineData">Engine data set</param>
        public EngineRevolutions(TalkerId talker, EngineData engineData)
            : base(talker, Id, DateTimeOffset.UtcNow)
        {
            RotationSource = RotationSource.Engine;
            RotationalSpeed = engineData.Revolutions;
            EngineNumber = engineData.EngineNo + 1;
            PropellerPitch = engineData.Pitch;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public EngineRevolutions(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Internal constructor from NMEA string
        /// </summary>
        public EngineRevolutions(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            string source = ReadString(field);
            int? engineNo = ReadInt(field);
            double? rpm = ReadValue(field);
            double? pitch = ReadValue(field);
            string status = ReadString(field);

            if (source == "S")
            {
                RotationSource = RotationSource.Shaft;
            }
            else if (source == "E")
            {
                RotationSource = RotationSource.Engine;
            }
            else
            {
                RotationSource = RotationSource.Unknown;
            }

            if (engineNo.HasValue)
            {
                EngineNumber = engineNo.Value;
            }
            else
            {
                EngineNumber = 1;
            }

            if (rpm.HasValue)
            {
                RotationalSpeed = RotationalSpeed.FromRevolutionsPerMinute(rpm.Value);
            }
            else
            {
                RotationalSpeed = RotationalSpeed.Zero;
            }

            if (pitch.HasValue)
            {
                PropellerPitch = Ratio.FromPercent(pitch.Value);
            }

            if (rpm.HasValue && status == "A")
            {
                Valid = true;
            }
            else
            {
                Valid = false;
            }
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Cross track distance, meters
        /// </summary>
        public RotationalSpeed RotationalSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// Revolution counter source
        /// </summary>
        public RotationSource RotationSource
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of engine
        /// </summary>
        public int EngineNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// Pitch of propeller. Negative if running astern.
        /// </summary>
        public Ratio? PropellerPitch
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
                string source = string.Empty;
                if (RotationSource == RotationSource.Engine)
                {
                    source = "E";
                }
                else if (RotationSource == RotationSource.Shaft)
                {
                    source = "S";
                }

                string pitch = PropellerPitch.HasValue ? PropellerPitch.Value.Percent.ToString("F0", CultureInfo.InvariantCulture) : string.Empty;
                return FormattableString.Invariant($"{source},{EngineNumber},{RotationalSpeed.RevolutionsPerMinute:F0},{pitch},A");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Engine {EngineNumber} RPM: {RotationalSpeed.RevolutionsPerMinute}";
            }

            return "No valid RPM message (or engine off)";
        }
    }
}

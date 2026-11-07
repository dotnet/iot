// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    public class VesselHeading : Nmea2000PackedMessage
    {
        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0x1F112;

        public override uint Identifier => HexId;

        public Angle Heading
        {
            get;
            set;
        }

        public Angle? Deviation
        {
            get;
            set;
        }

        public Angle? Variation
        {
            get;
            set;
        }

        public bool IsMagnetic
        {
            get;
            set;
        }

        public int Sid
        {
            get;
            set;
        }

        public override string ToReadableContent()
        {
            return $"Vessel {(IsMagnetic ? "Magnetic" : "True")} Heading: {Heading} Variation: {Variation}";
        }

        public VesselHeading(Angle heading, Angle? deviation, Angle? variation, bool magnetic)
        {
            Heading = heading;
            Deviation = deviation;
            Variation = variation;
            IsMagnetic = magnetic;
            Valid = true;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public VesselHeading(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public VesselHeading(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            using IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadByteFromHexString(data, 0, out byte sid))
            {
                Sid = sid;
            }

            if (ReadUshortFromHexString(data, 2, out ushort heading))
            {
                Heading = Angle.FromRadians(heading * 0.0001);
            }

            if (ReadShortFromHexString(data, 6, out short dev))
            {
                Deviation = Angle.FromRadians(dev * 0.0001);
            }

            if (ReadShortFromHexString(data, 10, out short variation))
            {
                Variation = Angle.FromRadians(variation * 0.0001);
            }

            if (ReadByteFromHexString(data, 14, out byte kind))
            {
                IsMagnetic = kind == 0xFD;
            }

            Valid = true;
        }

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

        public override string ToNmeaParameterList()
        {
            string data = "FF"; // SID
            short a = (short)(Heading.Radians * 10000);
            string angle = WriteShortToHex(a);
            short? v = null;
            if (Variation.HasValue)
            {
                v = (short)(Variation.Value.Radians * 10000);
            }

            string variation = WriteShortToHex(v);

            v = null;
            if (Deviation.HasValue)
            {
                v = (short)(Deviation.Value.Radians * 10000);
            }

            string deviation = WriteShortToHex(v);
            return base.ToNmeaParameterList() + data + angle + deviation + variation + (IsMagnetic ? "FD" : "FC");
        }
    }
}

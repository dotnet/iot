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

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    public class SeatalkNgPilotWindStatus : Nmea2000PackedMessage
    {
        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0x0FF41;

        public ManufacturerCode Manufacturer
        {
            get;
            set;
        }

        public IndustryCode Industry
        {
            get;
            set;
        }

        public Angle? DesiredWindAngle
        {
            get;
            set;
        }

        public Angle? AverageWindAngle
        {
            get;
            set;
        }

        public override uint Identifier => HexId;

        public override string ToReadableContent()
        {
            if (DesiredWindAngle.HasValue)
            {
                return $"SeatalkNg Pilot Wind Settings: {DesiredWindAngle.Value.Normalize(false)} Desired, {AverageWindAngle} Actual";
            }

            return $"SeatalkNg Pilot Wind Settings: No Desired Value, {AverageWindAngle} Actual";
        }

        public SeatalkNgPilotWindStatus(Angle? desiredWindAngle, Angle? averageWindAngle)
        {
            Manufacturer = ManufacturerCode.Raymarine;
            Industry = IndustryCode.Marine;
            DesiredWindAngle = desiredWindAngle;
            AverageWindAngle = averageWindAngle;
            Valid = true;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public SeatalkNgPilotWindStatus(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public SeatalkNgPilotWindStatus(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadManufacturerAndIndustryFromHexString(data, 0, out var manufacturer, out var industry))
            {
                Manufacturer = manufacturer;
                Industry = industry;
            }

            DesiredWindAngle = null;
            AverageWindAngle = null;

            short v = 0;
            if (ReadShortFromHexString(data, 4, out v))
            {
                DesiredWindAngle = Angle.FromRadians(v * 0.0001);
            }

            ushort angle = 0;
            if (ReadUshortFromHexString(data, 10, out angle))
            {
                AverageWindAngle = Angle.FromRadians(angle * 0.0001);
            }

            Valid = true;
        }

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

        public override string ToNmeaParameterList()
        {
            string manufacturer = WriteManufacturerAndIndustryToHex(Manufacturer, Industry);

            ushort? desiredAngle = null;
            if (DesiredWindAngle.HasValue)
            {
                desiredAngle = (ushort)Math.Round(DesiredWindAngle.Value.Normalize(true).Radians / 0.0001);
            }

            string trueAngle = WriteUshortToHex(desiredAngle);

            string magAngle = DoubleTo16BitField(AverageWindAngle.HasValue ? AverageWindAngle.Value.Normalize(true).Radians : null,
                0.0001).ToString("X4", CultureInfo.InvariantCulture);
            magAngle = magAngle.Substring(2, 2) + magAngle.Substring(0, 2);
            return base.ToNmeaParameterList() + manufacturer + trueAngle + magAngle + "FFFF";
        }
    }
}

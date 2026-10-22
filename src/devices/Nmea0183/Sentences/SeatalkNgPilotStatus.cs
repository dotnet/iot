// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    public class SeatalkNgPilotStatus : Nmea2000PackedMessage
    {
        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0x0FF63;

        private uint _manufacturerAndIndustry;

        public AutopilotStatus PilotStatus
        {
            get;
            private set;
        }

        public override uint Identifier => HexId;

        public override string ToReadableContent()
        {
            return $"SeatalkNg Pilot status: {PilotStatus}";
        }

        public SeatalkNgPilotStatus(AutopilotStatus status)
        {
            _manufacturerAndIndustry = ManufacturerRaymarine;
            PilotStatus = status;
            Valid = true;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public SeatalkNgPilotStatus(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public SeatalkNgPilotStatus(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadUshortFromHexString(data, 0, out ushort manf))
            {
                _manufacturerAndIndustry = manf;
            }

            if (ReadUshortFromHexString(data, 4, out ushort status))
            {
                PilotStatus = AutopilotStatusFromNumber(status);
            }
            else
            {
                PilotStatus = AutopilotStatus.Undefined;
            }

            Valid = true;
        }

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

        public static AutopilotStatus AutopilotStatusFromNumber(int status)
        {
            return status switch
            {
                0 => AutopilotStatus.Standby,
                64 => AutopilotStatus.Auto,
                256 => AutopilotStatus.Wind,
                384 => AutopilotStatus.Track,
                385 => AutopilotStatus.Track,
                _ => AutopilotStatus.Undefined,
            };
        }

        public override string ToNmeaParameterList()
        {
            string manufacturer = _manufacturerAndIndustry.ToString("X4", CultureInfo.InvariantCulture);
            int statusByte = PilotStatus switch
            {
                AutopilotStatus.Standby => 0,
                AutopilotStatus.Offline => 0,
                AutopilotStatus.Auto => 64,
                AutopilotStatus.Wind => 256,
                AutopilotStatus.Track => 384,
                _ => 0,
            };

            string statusString = statusByte.ToString("X4", CultureInfo.InvariantCulture);
            statusString = statusString.Substring(2, 2) + statusString.Substring(0, 2);

            // Byte 7 must be 0x07, or a Raymarine Chart plotter won't recognize it
            return base.ToNmeaParameterList() + manufacturer + statusString + "000007FF";
        }
    }
}

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
    public class SeatalkNgPilotConfigurationValue : Nmea2000PackedMessage
    {
        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0x1EF00;

        private uint _manufacturerAndIndustry;

        /// <summary>
        /// Supported values:
        /// 108 (Pilot configuration)
        /// </summary>
        public uint ProprietaryId
        {
            get;
            set;
        }

        /// <summary>
        /// Supported values:
        /// 38 Auto Turn, boolean
        /// </summary>
        public uint Command
        {
            get;
            set;
        }

        public object? Value
        {
            get;
            set;
        }

        public override uint Identifier => HexId;

        public override string ToReadableContent()
        {
            return $"SeatalkNg Proprietary stuff: {ProprietaryId} {Command} {Value}";
        }

        public SeatalkNgPilotConfigurationValue()
        {
            _manufacturerAndIndustry = ManufacturerRaymarine;
            Valid = true;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public SeatalkNgPilotConfigurationValue(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public SeatalkNgPilotConfigurationValue(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadUshortFromHexString(data, 0, out ushort manf))
            {
                _manufacturerAndIndustry = manf;
            }

            if (ReadByteFromHexString(data, 4, out byte proprietaryId))
            {
                ProprietaryId = proprietaryId;
            }
            else
            {
                ProprietaryId = 0;
            }

            if (ReadByteFromHexString(data, 6, out byte command))
            {
                Command = command;
            }
            else
            {
                Command = 0;
            }

            // Can't currently read the value
            Valid = true;
        }

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => false;

        public override string ToNmeaParameterList()
        {
            string manufacturer = _manufacturerAndIndustry.ToString("X4", CultureInfo.InvariantCulture);

            string data = ProprietaryId.ToString("X2", CultureInfo.InvariantCulture);
            data += Command.ToString("X2", CultureInfo.InvariantCulture);
            if (Value is bool b)
            {
                data += "00" + (b ? "01" : "00") + "000000";
            }
            else
            {
                // Don't know how to report these
                data += "0000000000";
            }

            // Byte 7 must be 0x07, or a Raymarine Chart plotter won't recognize it
            return base.ToNmeaParameterList() + manufacturer + data;
        }
    }
}

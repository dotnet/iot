// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Represents the state of an array of on/off switches.
    /// </summary>
    public sealed class CzoneModuleAnnounce : Nmea2000PackedMessage
    {
        /// <summary>
        /// The serial number to announce.
        /// </summary>
        public uint SerialNumber { get; }

        private readonly byte _dipSwitch;

        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0xFF0A;

        /// <summary>
        /// The Dip switch value for this board. Default for the first board is 0x80.
        /// </summary>
        public byte DipSwitch => _dipSwitch;

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Creates an empty instance of this class. Can be updated later.
        /// </summary>
        /// <param name="serialNumber">Serial number, see comments on <see cref="Nmea2000VirtualButtons.Init"/></param>
        /// <param name="dipSwitch">Dip switch value for this board (0-252)</param>
        public CzoneModuleAnnounce(uint serialNumber, byte dipSwitch)
        {
            SerialNumber = serialNumber;
            _dipSwitch = dipSwitch;
            Valid = true;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public CzoneModuleAnnounce(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public CzoneModuleAnnounce(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadByteFromHexString(data, 14, out byte b))
            {
                _dipSwitch = b;
            }

            if (ReadUintFromHexString(data, 4, out uint v))
            {
                SerialNumber = v & 0xFFFFF; // 20 bits
            }

            Valid = true;
        }

        /// <inheritdoc/>
        public override string ToNmeaParameterList()
        {
            string manufacturer = WriteManufacturerAndIndustryToHex(ManufacturerCode.BepMarine2, IndustryCode.Marine);
            string instance = WriteByteToHex(_dipSwitch);
            // The serial number is a 20-bit value, but we need to format it as a 6-character hex string, then rearrange the bytes for the NMEA sentence.
            // for now, we don't need the last 4 bits, so it stays 0
            var serial = SerialNumber.ToString("X6", CultureInfo.InvariantCulture);
            serial = serial.Substring(4, 2) + serial.Substring(2, 2) + serial.Substring(0, 2);

            return base.ToNmeaParameterList() + manufacturer + serial + "0000" + instance;
        }

        /// <inheritdoc/>
        public override string ToReadableContent()
        {
            return $"CZone Module Announce: {Identifier} for Dip {_dipSwitch}";
        }

        /// <inheritdoc/>
        public override uint Identifier => HexId;
    }
}

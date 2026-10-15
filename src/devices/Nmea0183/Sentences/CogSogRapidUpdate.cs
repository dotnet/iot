// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using UnitsNet;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// COG &amp; SOG, Rapid Update (PGN 129026)
    /// </summary>
    public class CogSogRapidUpdate : Nmea2000PackedMessage
    {
        // PGN 129026 == 0x1F802
        public const int HexId = 0x1F802;

        /// <summary>
        /// PGN 129026 == 0x1F802
        /// </summary>
        public override uint Identifier => HexId;

        /// <summary>
        /// Sequence ID (optional)
        /// </summary>
        public byte SequenceId { get; set; }

        /// <summary>
        /// True if the COG reference is true, false if it uses a magnetic reference.
        /// Since COG usually comes from a GNSS receiver, this is very rarely used with a magnetic reference.
        /// </summary>
        public bool IsTrueAngle { get; set; }

        /// <summary>
        /// Course over ground (nullable). Interpreted/stored as radians with resolution 0.0001 rad.
        /// </summary>
        public Angle? CourseOverGround { get; set; }

        /// <summary>
        /// Speed over ground (nullable). Interpreted/stored in m/s with resolution 0.01 m/s.
        /// </summary>
        public Speed? SpeedOverGround { get; set; }

        public override bool ReplacesOlderInstance => true;

        public CogSogRapidUpdate()
            : base()
        {
            Valid = true;
        }

        public CogSogRapidUpdate(Angle? courseOverGround, Speed? sog)
            : base()
        {
            CourseOverGround = courseOverGround;
            SpeedOverGround = sog;
            IsTrueAngle = true;
            Valid = true;
        }

        public CogSogRapidUpdate(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, sentence.Fields, time)
        {
        }

        public CogSogRapidUpdate(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            // Parse common header fields (PGN, timestamp, source)
            ParseCommonFields(field, isAddressedMessage: false);

            // Data blob as one field
            string data = ReadString(field);

            // Offsets are in nibbles (hex characters)
            // SequenceId: 1 byte => offset 0
            // COG: 2 bytes => offset 4
            // SOG: 2 bytes => offset 8
            bool gotAny = false;

            if (!string.IsNullOrEmpty(data))
            {
                if (ReadByteFromHexString(data, 1, out byte b))
                {
                    if ((b & 0x3) == 1)
                    {
                        IsTrueAngle = false;
                    }
                    else
                    {
                        IsTrueAngle = true;
                    }
                }

                if (ReadByteFromHexString(data, 0, out byte sid))
                {
                    SequenceId = sid;
                    gotAny = true;
                }

                if (ReadShortFromHexString(data, 4, out short cogRaw))
                {
                    // Scale: 0.0001 rad
                    CourseOverGround = Angle.FromRadians(cogRaw * 0.0001);
                    gotAny = true;
                }

                if (ReadUshortFromHexString(data, 8, out ushort sogRaw))
                {
                    // Scale: 0.01 m/s
                    SpeedOverGround = Speed.FromMetersPerSecond(sogRaw * 0.01);
                    gotAny = true;
                }
            }

            Valid = gotAny;
        }

        /// <summary>
        /// Serialize to the PCDIN data blob (no length field).
        /// Data layout: SID(1), COG(2), SOG(2) — concatenated hex in little-endian order.
        /// </summary>
        public override string ToNmeaParameterList()
        {
            string header = base.ToNmeaParameterList();

            // SequenceId (1 byte)
            string sidHex = WriteByteToHex(SequenceId);

            string cogReference;
            if (IsTrueAngle)
            {
                // Only the last bit is interesting here (0 = True, 1 = Magnetic)
                cogReference = WriteByteToHex(0xFC);
            }
            else
            {
                cogReference = WriteByteToHex(0xFD);
            }

            // COG (2 bytes signed) using 0.0001 rad resolution
            short? cogRaw = null;
            if (CourseOverGround.HasValue)
            {
                double radians = CourseOverGround.Value.Radians;
                int raw = (int)Math.Round(radians / 0.0001);
                cogRaw = unchecked((short)raw);
            }

            string cogHex = WriteShortToHex(cogRaw);

            // SOG (2 bytes unsigned) using 0.01 m/s resolution
            ushort? sogRaw = null;
            if (SpeedOverGround.HasValue)
            {
                double mps = SpeedOverGround.Value.MetersPerSecond;
                int raw = (int)Math.Round(mps / 0.01);
                if (raw < 0)
                {
                    raw = 0;
                }

                sogRaw = (ushort)raw;
            }

            string sogHex = WriteUshortToHex(sogRaw);

            return $"{header}{sidHex}{cogReference}{cogHex}{sogHex}FFFF";
        }

        public override string ToReadableContent()
        {
            string sid = SequenceId.ToString(CultureInfo.InvariantCulture);
            string cog = CourseOverGround.HasValue ? $"{CourseOverGround.Value.Degrees:F3}°" : "n/a";
            string sog = SpeedOverGround.HasValue ? $"{SpeedOverGround.Value.Knots:F2} kt" : "n/a";
            return $"COG&SOG Rapid: SID={sid}, COG={cog}, SOG={sog}";
        }
    }
}

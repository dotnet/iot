// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// System Time (PGN 126992)
    /// </summary>
    public class SystemTime : Nmea2000PackedMessage
    {
        public const int HexId = 0x1F010;

        public override uint Identifier => HexId;

        public byte SequenceId { get; set; }

        /// <summary>
        /// Source used to derive the time (GPS, radio signal, local clock, etc)
        /// </summary>
        public SystemTimeSource TimeSource { get; set; }

        public override bool ReplacesOlderInstance => true;

        public SystemTime()
            : base()
        {
            Valid = true;
        }

        public SystemTime(DateTimeOffset dateTimeUtc, SystemTimeSource timeSource = SystemTimeSource.Gps)
            : base()
        {
            DateTime = dateTimeUtc;
            TimeSource = timeSource;
            Valid = true;
        }

        public SystemTime(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, sentence.Fields, time)
        {
        }

        public SystemTime(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();
            ParseCommonFields(field, isAddressedMessage: false);

            string data = ReadString(field);
            if (string.IsNullOrEmpty(data) || data.Length < 14) // 7 bytes minimum payload
            {
                Valid = false;
                return;
            }

            bool ok = true;

            ok &= ReadByteFromHexString(data, 0, out byte sid);
            SequenceId = sid;

            ok &= ReadByteFromHexString(data, 2, out byte sourceByte);
            TimeSource = (SystemTimeSource)(sourceByte & 0x0F);

            ushort? days = TryReadUShort(data, 4);
            uint? timeRaw = TryReadUInt(data, 8);

            if (days.HasValue && timeRaw.HasValue)
            {
                DateTime = DateTimeOffset.UnixEpoch.AddDays(days.Value).AddSeconds(timeRaw.Value * 0.0001);
            }
            else
            {
                ok = false;
            }

            Valid = ok;
        }

        public override string ToNmeaParameterList()
        {
            string header = base.ToNmeaParameterList();
            StringBuilder data = new StringBuilder();

            data.Append(WriteByteToHex(SequenceId));

            // Reserved/source byte: lower 4 bits = source, upper 4 bits reserved (all 1)
            byte sourceByte = (byte)(((byte)TimeSource & 0x0F) | 0xF0);
            data.Append(WriteByteToHex(sourceByte));

            if (DateTime.Ticks != 0)
            {
                DateTimeOffset dt = DateTime.ToUniversalTime();
                ushort days = (ushort)(dt.Date - DateTimeOffset.UnixEpoch.Date).TotalDays;
                uint timeRaw = (uint)Math.Round(dt.TimeOfDay.TotalSeconds / 0.0001);

                data.Append(WriteUshortToHex(days));
                data.Append(WriteUInt32ToHex(timeRaw));
            }
            else
            {
                data.Append("FFFF");
                data.Append("FFFFFFFF");
            }

            return $"{header}{data}";
        }

        public override string ToReadableContent()
        {
            string dt = DateTime.ToString("yyyy-MM-dd HH:mm:ss");
            return $"System Time: {dt} UTC, Source={TimeSource}";
        }
    }
}

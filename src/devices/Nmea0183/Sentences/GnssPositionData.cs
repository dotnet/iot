// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnitsNet;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// GNSS Position Data (PGN 129029)
    /// </summary>
    public class GnssPositionData : Nmea2000PackedMessage
    {
        public const int HexId = 0x1F805;

        public override uint Identifier => HexId;

        public byte SequenceId { get; set; }
        public DateTimeOffset? PositionDateTimeUtc { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Length? Altitude { get; set; }

        public GnssType GnssType { get; set; }
        public GnssMethod GnssMethod { get; set; }
        public GnssIntegrity GnssIntegrity { get; set; }

        public byte NumberOfSatellites { get; set; }
        public double? Hdop { get; set; }
        public double? Pdop { get; set; }
        public Length? GeoidalSeparation { get; set; }

        public IList<GnssReferenceStation> ReferenceStations { get; } = new List<GnssReferenceStation>();

        public override bool ReplacesOlderInstance => true;

        public GnssPositionData()
            : base()
        {
            Valid = true;
        }

        public GnssPositionData(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, sentence.Fields, time)
        {
        }

        public GnssPositionData(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();
            ParseCommonFields(field, isAddressedMessage: false);

            string data = ReadString(field);
            if (string.IsNullOrEmpty(data) || data.Length < 86) // 43 bytes minimum payload
            {
                Valid = false;
                return;
            }

            bool ok = true;

            ReadByteFromHexString(data, 0, out byte sid);
            SequenceId = sid;

            ushort? days = TryReadUShort(data, 2);
            uint? timeRaw = TryReadUInt(data, 6);

            if (days.HasValue && timeRaw.HasValue)
            {
                PositionDateTimeUtc = DateTimeOffset.UnixEpoch.AddDays(days.Value).AddSeconds(timeRaw.Value * 0.0001);
            }

            long? latRaw = TryReadInt64(data, 14);
            long? lonRaw = TryReadInt64(data, 30);
            long? altRaw = TryReadInt64(data, 46);

            Latitude = latRaw.HasValue ? latRaw.Value * 1e-16 : (double?)null;
            Longitude = lonRaw.HasValue ? lonRaw.Value * 1e-16 : (double?)null;
            Altitude = altRaw.HasValue ? Length.FromMeters(altRaw.Value * 1e-6) : (Length?)null;

            ok &= ReadByteFromHexString(data, 62, out byte typeMethod);
            GnssType = (GnssType)(typeMethod & 0x0F);
            GnssMethod = (GnssMethod)((typeMethod >> 4) & 0x0F);

            ok &= ReadByteFromHexString(data, 64, out byte integrityByte);
            GnssIntegrity = (GnssIntegrity)(integrityByte & 0x03);

            ok &= ReadByteFromHexString(data, 66, out byte numSats);
            NumberOfSatellites = numSats;

            ushort? hdopRaw = TryReadUShort(data, 68);
            ushort? pdopRaw = TryReadUShort(data, 72);
            int? geoidalRaw = TryReadInt32(data, 76);

            Hdop = hdopRaw.HasValue ? hdopRaw.Value * 0.01 : (double?)null;
            Pdop = pdopRaw.HasValue ? pdopRaw.Value * 0.01 : (double?)null;
            GeoidalSeparation = geoidalRaw.HasValue ? Length.FromMeters(geoidalRaw.Value * 0.01) : (Length?)null;

            ok &= ReadByteFromHexString(data, 84, out byte refCount);

            int offset = 86;
            ReferenceStations.Clear();

            for (int i = 0; i < refCount; i++)
            {
                if (offset + 8 > data.Length)
                {
                    break;
                }

                ushort? typeAndId = TryReadUShort(data, offset);
                ushort? ageRaw = TryReadUShort(data, offset + 4);

                if (!typeAndId.HasValue || !ageRaw.HasValue)
                {
                    break;
                }

                byte refType = (byte)(typeAndId.Value & 0x0F);
                ushort refId = (ushort)(typeAndId.Value >> 4);

                ReferenceStations.Add(new GnssReferenceStation(refType, refId, TimeSpan.FromSeconds(ageRaw.Value * 0.01)));
                offset += 8;
            }

            Valid = ok;
        }

        public override string ToNmeaParameterList()
        {
            string header = base.ToNmeaParameterList();
            StringBuilder data = new StringBuilder();

            data.Append(WriteByteToHex(SequenceId));

            if (PositionDateTimeUtc.HasValue)
            {
                DateTimeOffset dt = PositionDateTimeUtc.Value.ToUniversalTime();
                int days = (int)(dt.Date - DateTimeOffset.UnixEpoch.Date).TotalDays;
                uint timeRaw = (uint)Math.Round(dt.TimeOfDay.TotalSeconds / 0.0001);

                data.Append(WriteUshortToHex((ushort)days));
                data.Append(WriteUInt32ToHex(timeRaw));
            }
            else
            {
                data.Append("FFFF");
                data.Append("FFFFFFFF");
            }

            data.Append(WriteInt64Scaled(Latitude, 1e-16));
            data.Append(WriteInt64Scaled(Longitude, 1e-16));
            data.Append(WriteInt64Scaled(Altitude?.Meters, 1e-6));

            byte typeMethod = (byte)(((byte)GnssMethod << 4) | ((byte)GnssType & 0x0F));
            data.Append(WriteByteToHex(typeMethod));

            byte integrity = (byte)(((byte)GnssIntegrity & 0x03) | 0xFC);
            data.Append(WriteByteToHex(integrity));

            data.Append(WriteByteToHex(NumberOfSatellites));

            data.Append(Hdop.HasValue ? WriteUshortToHex((ushort)Math.Round(Hdop.Value / 0.01)) : "FFFF");
            data.Append(Pdop.HasValue ? WriteUshortToHex((ushort)Math.Round(Pdop.Value / 0.01)) : "FFFF");

            if (GeoidalSeparation.HasValue)
            {
                int geoRaw = (int)Math.Round(GeoidalSeparation.Value.Meters / 0.01);
                data.Append(WriteInt32ToHex(geoRaw));
            }
            else
            {
                data.Append("FFFFFF7F");
            }

            byte refCount = (byte)Math.Min(ReferenceStations.Count, byte.MaxValue);
            data.Append(WriteByteToHex(refCount));

            for (int i = 0; i < refCount; i++)
            {
                GnssReferenceStation station = ReferenceStations[i];
                ushort typeAndId = (ushort)(((station.StationId & 0x0FFF) << 4) | (station.ReferenceStationType & 0x0F));
                ushort age = (ushort)Math.Round(station.AgeOfCorrections.TotalSeconds / 0.01);

                data.Append(WriteUshortToHex(typeAndId));
                data.Append(WriteUshortToHex(age));
            }

            return $"{header}{data}";
        }

        public override string ToReadableContent()
        {
            return $"GNSS Position: Lat={Latitude:F7}, Lon={Longitude:F7}, Sats={NumberOfSatellites}, Type={GnssType}, Method={GnssMethod}";
        }
    }
}

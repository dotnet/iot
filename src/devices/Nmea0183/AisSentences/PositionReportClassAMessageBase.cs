// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal abstract record PositionReportClassAMessageBase : AisMessage
    {
        public NavigationStatus NavigationStatus { get; set; }
        public int? RateOfTurn { get; set; }
        public double SpeedOverGround { get; set; }
        public PositionAccuracy PositionAccuracy { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double CourseOverGround { get; set; }
        public uint? TrueHeading { get; set; }
        public uint Timestamp { get; set; }
        public ManeuverIndicator ManeuverIndicator { get; set; }
        public uint Spare { get; set; }
        public Raim Raim { get; set; }
        public uint RadioStatus { get; set; }

        protected PositionReportClassAMessageBase(AisMessageType messageType)
            : base(messageType)
        {
        }

        protected PositionReportClassAMessageBase(AisMessageType messageType, Payload payload)
            : base(messageType, payload)
        {
            Repeat = payload.ReadUInt(6, 2);
            Mmsi = payload.ReadUInt(8, 30);
            NavigationStatus = payload.ReadEnum<NavigationStatus>(38, 4);
            RateOfTurn = payload.ReadRateOfTurn(42, 8);
            if (RateOfTurn == 0x80)
            {
                // Not defined
                RateOfTurn = null;
            }

            SpeedOverGround = payload.ReadSpeedOverGround(50, 10);
            PositionAccuracy = payload.ReadEnum<PositionAccuracy>(60, 1);
            Longitude = payload.ReadLongitude(61, 28);
            Latitude = payload.ReadLatitude(89, 27);
            CourseOverGround = payload.ReadCourseOverGround(116, 12);
            TrueHeading = payload.ReadTrueHeading(128, 9);
            Timestamp = payload.ReadUInt(137, 6);
            ManeuverIndicator = payload.ReadEnum<ManeuverIndicator>(143, 2);
            Spare = payload.ReadUInt(145, 3);
            Raim = payload.ReadEnum<Raim>(148, 1);
            RadioStatus = payload.ReadUInt(149, 19);
        }

        public override AisTransceiverClass TransceiverType => AisTransceiverClass.A;

        public override void Encode(Payload payload)
        {
            base.Encode(payload);
            payload.WriteEnum<NavigationStatus>(NavigationStatus, 4);
            if (RateOfTurn == null)
            {
                payload.WriteRateOfTurn(0x80, 8); // 0x80 is "unknown"
            }
            else
            {
                payload.WriteRateOfTurn((int)RateOfTurn, 8);
            }

            payload.WriteSpeedOverGround(SpeedOverGround, 10);
            payload.WriteEnum<PositionAccuracy>(PositionAccuracy, 1);
            payload.WriteLongitude(Longitude, 28);
            payload.WriteLatitude(Latitude, 27);
            payload.WriteCourseOverGround(CourseOverGround, 12);
            if (TrueHeading == null)
            {
                payload.WriteRateOfTurn(511, 9); // Value indicates "not available"
            }
            else
            {
                payload.WriteTrueHeading((uint)TrueHeading, 9);
            }

            payload.WriteUInt(Timestamp, 6);
            payload.WriteEnum<ManeuverIndicator>(ManeuverIndicator, 2);
            payload.WriteUInt(Spare, 3);
            payload.WriteEnum<Raim>(Raim, 1);
            payload.WriteUInt(RadioStatus, 19);
        }
    }
}

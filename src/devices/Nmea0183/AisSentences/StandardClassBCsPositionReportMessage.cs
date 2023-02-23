// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record StandardClassBCsPositionReportMessage : AisMessage
    {
        public uint Reserved { get; set; }
        public double SpeedOverGround { get; set; }
        public PositionAccuracy PositionAccuracy { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double CourseOverGround { get; set; }
        public uint? TrueHeading { get; set; }
        public uint Timestamp { get; set; }
        public uint RegionalReserved { get; set; }
        public bool IsCsUnit { get; set; }
        public bool HasDisplay { get; set; }
        public bool HasDscCapability { get; set; }
        public bool Band { get; set; }
        public bool CanAcceptMessage22 { get; set; }
        public bool Assigned { get; set; }
        public Raim Raim { get; set; }
        public uint RadioStatus { get; set; }

        public StandardClassBCsPositionReportMessage()
            : base(AisMessageType.StandardClassBCsPositionReport)
        {
        }

        public StandardClassBCsPositionReportMessage(Payload payload)
            : base(AisMessageType.StandardClassBCsPositionReport, payload)
        {
            Reserved = payload.ReadUInt(38, 8);
            SpeedOverGround = payload.ReadSpeedOverGround(46, 10);
            PositionAccuracy = payload.ReadEnum<PositionAccuracy>(56, 1);
            Longitude = payload.ReadLongitude(57, 28);
            Latitude = payload.ReadLatitude(85, 27);
            CourseOverGround = payload.ReadCourseOverGround(112, 12);
            TrueHeading = payload.ReadTrueHeading(124, 9);
            Timestamp = payload.ReadUInt(133, 6);
            RegionalReserved = payload.ReadUInt(139, 2);
            IsCsUnit = payload.ReadBoolean(141, 1);
            HasDisplay = payload.ReadBoolean(142, 1);
            HasDscCapability = payload.ReadBoolean(143, 1);
            Band = payload.ReadBoolean(144, 1);
            CanAcceptMessage22 = payload.ReadBoolean(145, 1);
            Assigned = payload.ReadBoolean(146, 1);
            Raim = payload.ReadEnum<Raim>(147, 1);
            RadioStatus = payload.ReadUInt(148, 20);
        }

        public override AisTransceiverClass TransceiverType => AisTransceiverClass.B;
    }
}

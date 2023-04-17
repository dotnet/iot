// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record ExtendedClassBCsPositionReportMessage : AisMessage
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
        public string Name { get; set; }
        public ShipType ShipType { get; set; }
        public uint DimensionToBow { get; set; }
        public uint DimensionToStern { get; set; }
        public uint DimensionToPort { get; set; }
        public uint DimensionToStarboard { get; set; }
        public PositionFixType PositionFixType { get; set; }
        public Raim Raim { get; set; }
        public bool DataTerminalReady { get; set; }
        public bool Assigned { get; set; }
        public uint Spare { get; set; }

        public ExtendedClassBCsPositionReportMessage()
            : base(AisMessageType.ExtendedClassBCsPositionReport)
        {
            Name = string.Empty;
        }

        public ExtendedClassBCsPositionReportMessage(Payload payload)
            : base(AisMessageType.ExtendedClassBCsPositionReport, payload)
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
            Name = payload.ReadString(143, 120);
            ShipType = payload.ReadEnum<ShipType>(263, 8);
            DimensionToBow = payload.ReadUInt(271, 9);
            DimensionToStern = payload.ReadUInt(280, 9);
            DimensionToPort = payload.ReadUInt(289, 6);
            DimensionToStarboard = payload.ReadUInt(295, 6);
            PositionFixType = payload.ReadEnum<PositionFixType>(301, 4);
            Raim = payload.ReadEnum<Raim>(305, 1);
            DataTerminalReady = payload.ReadDataTerminalReady(306, 1);
            Assigned = payload.ReadBoolean(307, 1);
            Spare = payload.ReadUInt(308, 4);
        }

        public override AisTransceiverClass TransceiverType => AisTransceiverClass.B;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record StandardSarAircraftPositionReportMessage : AisMessage
    {
        public uint Altitude { get; set; }
        public uint SpeedOverGround { get; set; }
        public PositionAccuracy PositionAccuracy { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double CourseOverGround { get; set; }
        public uint Timestamp { get; set; }
        public uint Reserved { get; set; }
        public bool DataTerminalReady { get; set; }
        public uint Spare { get; set; }
        public bool Assigned { get; set; }
        public Raim Raim { get; set; }
        public uint RadioStatus { get; set; }

        public StandardSarAircraftPositionReportMessage()
            : base(AisMessageType.StandardSarAircraftPositionReport)
        {
        }

        public StandardSarAircraftPositionReportMessage(Payload payload)
            : base(AisMessageType.StandardSarAircraftPositionReport, payload)
        {
            Altitude = payload.ReadUInt(38, 12);
            SpeedOverGround = payload.ReadUInt(50, 10);
            PositionAccuracy = payload.ReadEnum<PositionAccuracy>(60, 1);
            Longitude = payload.ReadLongitude(61, 28);
            Latitude = payload.ReadLatitude(89, 27);
            CourseOverGround = payload.ReadCourseOverGround(116, 12);
            Timestamp = payload.ReadUInt(128, 6);
            Reserved = payload.ReadUInt(134, 8);
            DataTerminalReady = payload.ReadDataTerminalReady(142, 1);
            Spare = payload.ReadUInt(143, 3);
            Assigned = payload.ReadBoolean(146, 1);
            Raim = payload.ReadEnum<Raim>(147, 1);
            RadioStatus = payload.ReadUInt(148, 20);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record UtcAndDateResponseMessage : AisMessage
    {
        public uint Year { get; set; }
        public uint Month { get; set; }
        public uint Day { get; set; }
        public uint Hour { get; set; }
        public uint Minute { get; set; }
        public uint Second { get; set; }
        public PositionAccuracy PositionAccuracy { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public PositionFixType PositionFixType { get; set; }
        public uint Spare { get; set; }
        public Raim Raim { get; set; }
        public uint RadioStatus { get; set; }

        public UtcAndDateResponseMessage()
            : base(AisMessageType.UtcAndDateResponse)
        {
        }

        public UtcAndDateResponseMessage(Payload payload)
            : base(AisMessageType.UtcAndDateResponse, payload)
        {
            Year = payload.ReadUInt(38, 14);
            Month = payload.ReadUInt(52, 4);
            Day = payload.ReadUInt(56, 5);
            Hour = payload.ReadUInt(61, 5);
            Minute = payload.ReadUInt(66, 6);
            Second = payload.ReadNullableUInt(72, 6) ?? 0;
            PositionAccuracy = payload.ReadEnum<PositionAccuracy>(78, 1);
            Longitude = payload.ReadLongitude(79, 28);
            Latitude = payload.ReadLatitude(107, 27);
            PositionFixType = payload.ReadEnum<PositionFixType>(134, 4);
            Spare = payload.ReadUInt(138, 10);
            Raim = payload.ReadEnum<Raim>(148, 1);
            RadioStatus = payload.ReadUInt(149, 19);
        }
    }
}

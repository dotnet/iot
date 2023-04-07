// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using System;
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record AidToNavigationReportMessage : AisMessage
    {
        public NavigationalAidType NavigationalAidType { get; set; }
        public string Name { get; set; }
        public PositionAccuracy PositionAccuracy { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public uint DimensionToBow { get; set; }
        public uint DimensionToStern { get; set; }
        public uint DimensionToPort { get; set; }
        public uint DimensionToStarboard { get; set; }
        public PositionFixType PositionFixType { get; set; }
        public uint Timestamp { get; set; }
        public bool OffPosition { get; set; }
        public uint RegionalReserved { get; set; }
        public Raim Raim { get; set; }
        public bool VirtualAid { get; set; }
        public bool Assigned { get; set; }
        public uint Spare { get; set; }
        public string NameExtension { get; set; }

        public AidToNavigationReportMessage()
            : base(AisMessageType.AidToNavigationReport)
        {
            Name = string.Empty;
            NameExtension = string.Empty;
        }

        internal AidToNavigationReportMessage(Payload payload)
            : base(AisMessageType.AidToNavigationReport, payload)
        {
            NavigationalAidType = payload.ReadEnum<NavigationalAidType>(38, 5);
            Name = payload.ReadString(43, 120);
            PositionAccuracy = payload.ReadEnum<PositionAccuracy>(163, 1);
            Longitude = payload.ReadLongitude(164, 28);
            Latitude = payload.ReadLatitude(192, 27);
            DimensionToBow = payload.ReadUInt(219, 9);
            DimensionToStern = payload.ReadUInt(228, 9);
            DimensionToPort = payload.ReadUInt(237, 6);
            DimensionToStarboard = payload.ReadUInt(243, 6);
            PositionFixType = payload.ReadEnum<PositionFixType>(249, 4);
            Timestamp = payload.ReadUInt(253, 6);
            OffPosition = payload.ReadBoolean(259, 1);
            RegionalReserved = payload.ReadUInt(260, 8);
            Raim = payload.ReadEnum<Raim>(268, 1);
            VirtualAid = payload.ReadBoolean(269, 1);
            Assigned = payload.ReadBoolean(270, 1);
            Spare = payload.ReadUInt(271, 1);
            NameExtension = payload.ReadString(272, 88);
        }
    }
}

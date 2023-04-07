// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record StaticDataReportPartBMessage : StaticDataReportMessage
    {
        public ShipType ShipType { get; set; }
        public string VendorId { get; set; }
        public uint UnitModelCode { get; set; }
        public uint SerialNumber { get; set; }
        public string CallSign { get; set; }
        public uint DimensionToBow { get; set; }
        public uint DimensionToStern { get; set; }
        public uint DimensionToPort { get; set; }
        public uint DimensionToStarboard { get; set; }
        public uint MothershipMmsi { get; set; }
        public PositionFixType PositionFixType { get; set; }
        public uint Spare { get; set; }

        public StaticDataReportPartBMessage()
            : base(1)
        {
            VendorId = string.Empty;
            CallSign = string.Empty;
        }

        public StaticDataReportPartBMessage(StaticDataReportMessage message, Payload payload)
            : base(message)
        {
            ShipType = payload.ReadEnum<ShipType>(40, 8);
            VendorId = payload.ReadString(48, 18);
            UnitModelCode = payload.ReadUInt(66, 4);
            SerialNumber = payload.ReadUInt(70, 20);
            CallSign = payload.ReadString(90, 42);

            // TODO: handle MMSI auxiliary craft
            // MothershipMmsi = payload.ReadUInt(132, 30);
            DimensionToBow = payload.ReadUInt(132, 9);
            DimensionToStern = payload.ReadUInt(141, 9);
            DimensionToPort = payload.ReadUInt(150, 6);
            DimensionToStarboard = payload.ReadUInt(156, 6);
            PositionFixType = payload.ReadEnum<PositionFixType>(162, 4);
            Spare = payload.ReadUInt(166, 2);
        }
    }
}

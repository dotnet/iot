// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record StaticAndVoyageRelatedDataMessage : AisMessage
    {
        public uint AisVersion { get; set; }
        public uint ImoNumber { get; set; }
        public string CallSign { get; set; }
        public string ShipName { get; set; }
        public ShipType ShipType { get; set; }
        public uint DimensionToBow { get; set; }
        public uint DimensionToStern { get; set; }
        public uint DimensionToPort { get; set; }
        public uint DimensionToStarboard { get; set; }
        public PositionFixType PositionFixType { get; set; }
        public uint EtaMonth { get; set; }
        public uint EtaDay { get; set; }
        public uint EtaHour { get; set; }
        public uint EtaMinute { get; set; }
        public double Draught { get; set; }
        public string Destination { get; set; }
        public bool DataTerminalReady { get; set; }
        public uint Spare { get; set; }

        public StaticAndVoyageRelatedDataMessage()
            : base(AisMessageType.StaticAndVoyageRelatedData)
        {
            CallSign = string.Empty;
            ShipName = string.Empty;
            Destination = string.Empty;
        }

        public StaticAndVoyageRelatedDataMessage(Payload payload)
            : base(AisMessageType.StaticAndVoyageRelatedData, payload)
        {
            Repeat = payload.ReadUInt(6, 2);
            Mmsi = payload.ReadUInt(8, 30);
            AisVersion = payload.ReadUInt(38, 2);
            ImoNumber = payload.ReadUInt(40, 30);
            CallSign = payload.ReadString(70, 42).Trim(); // Unlike the name, the callsign cannot contain blanks, and so also not start with one
            ShipName = payload.ReadString(112, 120);
            ShipType = payload.ReadEnum<ShipType>(232, 8);
            DimensionToBow = payload.ReadUInt(240, 9);
            DimensionToStern = payload.ReadUInt(249, 9);
            DimensionToPort = payload.ReadUInt(258, 6);
            DimensionToStarboard = payload.ReadUInt(264, 6);
            PositionFixType = payload.ReadEnum<PositionFixType>(270, 4);
            EtaMonth = payload.ReadUInt(274, 4);
            EtaDay = payload.ReadUInt(278, 5);
            EtaHour = payload.ReadUInt(283, 5);
            EtaMinute = payload.ReadUInt(288, 6);
            Draught = payload.ReadDraught(294, 8);
            Destination = payload.ReadString(302, 120);
            DataTerminalReady = payload.ReadDataTerminalReady(422, 1);
            Spare = payload.ReadUInt(423, 1);
        }

        public override AisTransceiverClass TransceiverType => AisTransceiverClass.A;

        public bool IsEtaValid()
        {
            return EtaMonth >= 1 && EtaMonth <= 12 &&
                   EtaDay >= 1 && EtaDay <= 31 &&
                   EtaHour >= 0 && EtaHour < 24 &&
                   EtaMinute >= 0 && EtaMinute < 60;
        }

        public override void Encode(Payload payload)
        {
            base.Encode(payload);
            payload.WriteUInt(AisVersion, 2);
            payload.WriteUInt(ImoNumber, 30);
            payload.WriteString(CallSign, 42, true);
            payload.WriteString(ShipName, 120, true);
            payload.WriteEnum(ShipType, 8);
            payload.WriteUInt(DimensionToBow, 9);
            payload.WriteUInt(DimensionToStern, 9);
            payload.WriteUInt(DimensionToPort, 6);
            payload.WriteUInt(DimensionToStarboard, 6);
            payload.WriteEnum(PositionFixType, 4);
            payload.WriteUInt(EtaMonth, 4);
            payload.WriteUInt(EtaDay, 5);
            payload.WriteUInt(EtaHour, 5);
            payload.WriteUInt(EtaMinute, 6);
            payload.WriteDraught(Draught, 8);
            payload.WriteString(Destination, 120, true);
            payload.WriteUInt(DataTerminalReady ? 0 : 1u, 1);
            payload.WriteUInt(Spare, 1);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record BinaryAcknowledgeMessage : AisMessage
    {
        public uint Spare { get; set; }
        public uint SequenceNumber1 { get; set; }
        public uint Mmsi1 { get; set; }
        public uint SequenceNumber2 { get; set; }
        public uint? Mmsi2 { get; set; }
        public uint SequenceNumber3 { get; set; }
        public uint? Mmsi3 { get; set; }
        public uint SequenceNumber4 { get; set; }
        public uint? Mmsi4 { get; set; }

        public BinaryAcknowledgeMessage()
            : base(AisMessageType.BinaryAcknowledge)
        {
        }

        internal BinaryAcknowledgeMessage(Payload payload)
            : base(AisMessageType.BinaryAcknowledge, payload)
        {
            Spare = payload.ReadUInt(38, 2);
            Mmsi1 = payload.ReadUInt(40, 30);
            SequenceNumber1 = payload.ReadUInt(70, 2);
            Mmsi2 = payload.ReadMmsi(72, 30);
            SequenceNumber2 = payload.ReadUInt(102, 2);
            Mmsi3 = payload.ReadMmsi(104, 30);
            SequenceNumber3 = payload.ReadUInt(134, 2);
            Mmsi4 = payload.ReadMmsi(136, 30);
            SequenceNumber4 = payload.ReadUInt(166, 2);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record BinaryAddressedMessage : AisMessage
    {
        public uint SequenceNumber { get; set; }
        public uint DestinationMmsi { get; set; }
        public bool RetransmitFlag { get; set; }
        public uint Spare { get; set; }
        public uint DesignatedAreaCode { get; set; }
        public uint FunctionalId { get; set; }
        public string Data { get; set; }

        public BinaryAddressedMessage()
            : base(AisMessageType.BinaryAddressedMessage)
        {
            Data = string.Empty;
        }

        internal BinaryAddressedMessage(Payload payload)
            : base(AisMessageType.BinaryAddressedMessage, payload)
        {
            SequenceNumber = payload.ReadUInt(38, 2);
            DestinationMmsi = payload.ReadUInt(40, 30);
            RetransmitFlag = payload.ReadBoolean(70, 1);
            Spare = payload.ReadUInt(71, 1);
            DesignatedAreaCode = payload.ReadUInt(72, 10);
            FunctionalId = payload.ReadUInt(82, 6);
            Data = payload.RawValue.Substring(88); // It's binary, so keep it binary
        }
    }
}

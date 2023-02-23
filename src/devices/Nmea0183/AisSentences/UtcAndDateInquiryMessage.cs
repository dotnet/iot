// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record UtcAndDateInquiryMessage : AisMessage
    {
        public uint Spare1 { get; set; }
        public uint DestinationMmsi { get; set; }
        public uint Spare2 { get; set; }

        public UtcAndDateInquiryMessage()
            : base(AisMessageType.UtcAndDateInquiry)
        {
        }

        public UtcAndDateInquiryMessage(Payload payload)
            : base(AisMessageType.UtcAndDateInquiry, payload)
        {
            Spare1 = payload.ReadUInt(38, 2);
            DestinationMmsi = payload.ReadUInt(40, 30);
            Spare2 = payload.ReadUInt(70, 2);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record InterrogationMessage : AisMessage
    {
        public uint InterrogatedMmsi { get; set; }
        public AisMessageType FirstMessageType { get; set; }
        public uint FirstSlotOffset { get; set; }

        public AisMessageType? SecondMessageType { get; set; }
        public uint? SecondSlotOffset { get; set; }

        public uint? SecondStationInterrogationMmsi { get; set; }
        public AisMessageType? SecondStationFirstMessageType { get; set; }
        public uint? SecondStationFirstSlotOffset { get; set; }

        public InterrogationMessage()
            : base(AisMessageType.Interrogation)
        {
        }

        public InterrogationMessage(Payload payload)
            : base(AisMessageType.Interrogation, payload)
        {
            // spare 38, 2
            InterrogatedMmsi = payload.ReadUInt(40, 30);
            FirstMessageType = payload.ReadEnum<AisMessageType>(70, 6);
            FirstSlotOffset = payload.ReadUInt(76, 12);

            var length = payload.Length;
            if (length > 88)
            {
                // spare 88, 2
                SecondMessageType = payload.ReadNullableMessageType(90, 6);
                if (SecondMessageType != null)
                {
                    SecondSlotOffset = payload.ReadNullableUInt(96, 12);
                }
                else
                {
                    SecondSlotOffset = null;
                }

                // spare 108, 2
            }

            if (length > 110)
            {
                SecondStationInterrogationMmsi = payload.ReadNullableUInt(110, 30);
                SecondStationFirstMessageType = payload.ReadNullableMessageType(140, 6);
                if (SecondStationFirstMessageType != null)
                {
                    SecondStationFirstSlotOffset = payload.ReadNullableUInt(146, 12);
                }
                else
                {
                    SecondStationFirstSlotOffset = null;
                }

                // spare 158, 2
            }
        }
    }
}

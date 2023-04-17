// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using System;
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record SafetyRelatedBroadcastMessage : AisMessage
    {
        public uint Spare { get; set; }

        public string Text { get; set; }

        public SafetyRelatedBroadcastMessage()
            : base(AisMessageType.SafetyRelatedBroadcastMessage)
        {
            Text = String.Empty;
        }

        internal SafetyRelatedBroadcastMessage(Payload payload)
            : base(AisMessageType.SafetyRelatedBroadcastMessage, payload)
        {
            Spare = payload.ReadUInt(38, 2);
            Text = payload.ReadString(40, 966);
        }

        public override void Encode(Payload payload)
        {
            base.Encode(payload);
            payload.WriteUInt(Spare, 2);
            payload.WriteString(Text, 966, false);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record BinaryBroadcastMessage : AisMessage
    {
        public uint Spare { get; set; }
        public uint DesignatedAreaCode { get; set; }
        public uint FunctionalId { get; set; }

        /// <summary>
        /// Binary payload, as string of "0" and "1"
        /// </summary>
        public string Data { get; set; }

        public BinaryBroadcastMessage()
            : base(AisMessageType.BinaryBroadcastMessage)
        {
            Data = string.Empty;
        }

        public BinaryBroadcastMessage(Payload payload)
            : base(AisMessageType.BinaryBroadcastMessage, payload)
        {
            Spare = payload.ReadUInt(38, 2);
            DesignatedAreaCode = payload.ReadUInt(40, 10);
            FunctionalId = payload.ReadUInt(50, 6);
            Data = payload.RawValue.Substring(56);
        }
    }
}

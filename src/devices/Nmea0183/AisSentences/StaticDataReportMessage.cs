// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record StaticDataReportMessage : AisMessage
    {
        public uint PartNumber { get; }

        protected StaticDataReportMessage()
            : base(AisMessageType.StaticDataReport)
        {
        }

        protected StaticDataReportMessage(uint partNumber)
            : base(AisMessageType.StaticDataReport)
        {
            PartNumber = partNumber;
        }

        protected StaticDataReportMessage(StaticDataReportMessage message)
            : base(message)
        {
            PartNumber = message.PartNumber;
        }

        private StaticDataReportMessage(Payload payload)
            : base(AisMessageType.StaticDataReport, payload)
        {
            PartNumber = payload.ReadUInt(38, 2);
        }

        public override AisTransceiverClass TransceiverType => AisTransceiverClass.B;

        public static AisMessage Create(Payload payload)
        {
            var message = new StaticDataReportMessage(payload);
            if (message.PartNumber == 0)
            {
                return new StaticDataReportPartAMessage(message, payload);
            }

            return new StaticDataReportPartBMessage(message, payload);
        }
    }
}

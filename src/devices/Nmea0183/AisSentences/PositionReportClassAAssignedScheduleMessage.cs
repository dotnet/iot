// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.AisSentences
{
    internal record PositionReportClassAAssignedScheduleMessage : PositionReportClassAMessageBase
    {
        public PositionReportClassAAssignedScheduleMessage()
            : base(AisMessageType.PositionReportClassAAssignedSchedule)
        {
        }

        public PositionReportClassAAssignedScheduleMessage(Payload payload)

            : base(AisMessageType.PositionReportClassAAssignedSchedule, payload)
        {
        }
    }
}

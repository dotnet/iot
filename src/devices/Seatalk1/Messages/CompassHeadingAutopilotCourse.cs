// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Seatalk1.Messages
{
    public record CompassHeadingAutopilotCourse : SeatalkMessage
    {
        public override byte CommandByte => 0x84;
        public override byte ExpectedLength => 0x9;

        public Angle CompassHeading { get; init; }

        public byte AutoPilotType { get; init; }

        public Angle AutoPilotCourse { get; init; }

        public Angle RudderPosition { get; init; }

        public AutopilotStatus AutopilotStatus { get; init; }

        public AutopilotAlarms Alarms { get; init; }

        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            VerifyPacket(data);

            uint u = ((uint)data[1]) >> 4;
            uint vw = data[2];
            // TODO: Verify last part. Actually, only one bit is required there, and the uppermost bit is also used for the turning direction, according to the docs
            long heading = (u & 0x3) * 90 + (vw & 0x3F) * 2 + BitCount(u & 0xC);
            Angle headingA = Angle.FromDegrees(heading);

            long desiredCourse = (vw >> 6) * 90 + data[3] / 2;

            AutopilotStatus status = GetAutopilotStatus(data[4]);

            if (status == AutopilotStatus.Undefined)
            {
                Logger.LogWarning($"Unknown autopilot status byte {data[4]}");
            }

            sbyte rudder = (sbyte)data[6];

            AutopilotAlarms alarms = AutopilotAlarms.None;
            byte almByte = data[5];
            if ((almByte & 0x4) == 0x4)
            {
                alarms |= AutopilotAlarms.OffCourse;
            }

            if ((almByte & 0x8) == 0x8)
            {
                alarms |= AutopilotAlarms.WindShift;
            }

            return this with
            {
                CompassHeading = headingA,
                AutoPilotType = data[8],
                AutoPilotCourse = Angle.FromDegrees(desiredCourse),
                AutopilotStatus = status,
                RudderPosition = Angle.FromDegrees(rudder),
                Alarms = alarms,
            };
        }

        public override byte[] CreateDatagram()
        {
            throw new NotImplementedException();
        }

        private AutopilotStatus GetAutopilotStatus(byte z)
        {
            switch (z)
            {
                case 0:
                    return AutopilotStatus.Standby;
                case 2:
                    return AutopilotStatus.Auto;
                case 4: // Wind mode may be on its own and together with the auto flag
                    return AutopilotStatus.InactiveWind;
                case 6:
                    return AutopilotStatus.Wind;
                case 8:
                    return AutopilotStatus.InactiveTrack;
                case 10:
                    return AutopilotStatus.Track;
                case 0x10:
                    return AutopilotStatus.Calibration;
                default:
                    return AutopilotStatus.Undefined;
            }
        }

        public override bool MatchesMessageType(IReadOnlyList<byte> data)
        {
            // Add some additional tests to make sure the message is not messed up
            return base.MatchesMessageType(data) && GetAutopilotStatus(data[4]) != AutopilotStatus.Undefined && (data[5] & 0xF0) == 0 && (data[8] & 0xF0) == 0;
        }
    }
}

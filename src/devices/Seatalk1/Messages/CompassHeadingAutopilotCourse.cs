// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            return new CompassHeadingAutopilotCourse()
            {
                CompassHeading = headingA,
                AutoPilotType = data[8],
                AutoPilotCourse = Angle.FromDegrees(desiredCourse),
                AutopilotStatus = status,
                RudderPosition = Angle.FromDegrees(rudder),
                Alarms = alarms,
            };
        }

        private AutopilotStatus GetAutopilotStatus(byte z)
        {
            return (z & 0xf) switch
            {
                0 => AutopilotStatus.Standby,
                2 => AutopilotStatus.Auto,
                4 => AutopilotStatus.Wind,
                8 => AutopilotStatus.Track,
                _ => AutopilotStatus.Undefined,
            };
        }
    }
}

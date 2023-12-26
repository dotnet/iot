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

        public TurnDirection TurnDirection { get; init; }

        public byte AutoPilotType { get; set; }

        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            VerifyPacket(data);

            uint u = ((uint)data[1]) >> 4;
            uint vw = data[2];
            // TODO: Verify last part. Actually, only one bit is required there, and the uppermost bit is also used for the turning direction, according to the docs
            long heading = (u & 0x3) * 90 + (vw & 0x3F) * 2 + BitCount(u & 0xC);
            Angle headingA = Angle.FromDegrees(heading);

            return new CompassHeadingAutopilotCourse()
            {
                CompassHeading = headingA,
                TurnDirection = (u & 0x80) == 0 ? TurnDirection.Port : TurnDirection.Starboard,
                AutoPilotType = data[8],
            };
        }
    }
}

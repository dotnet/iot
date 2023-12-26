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
    public record CompassHeadingAndRudderPosition : SeatalkMessage
    {
        public override byte CommandByte => 0x9c;
        public override byte ExpectedLength => 4;

        public Angle CompassHeading
        {
            get;
            init;
        }

        public Angle RudderPosition
        {
            get;
            init;
        }

        /// <summary>
        /// 84  U6  VW  XY 0Z 0M RR SS TT  Compass heading  Autopilot course
        /// </summary>
        /// <param name="data">Input data</param>
        /// <returns>A message of this type</returns>
        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            VerifyPacket(data);

            uint u = ((uint)data[1]) >> 4;
            uint vw = data[2];
            // TODO: Verify last part. Actually, only one bit is required there, and the uppermost bit is also used for the turning direction, according to the docs
            long heading = (u & 0x3) * 90 + (vw & 0x3F) * 2 + BitCount(u & 0xC);
            Angle headingA = Angle.FromDegrees(heading);
            sbyte rudder = (sbyte)data[3];

            return new CompassHeadingAndRudderPosition()
            {
                CompassHeading = headingA,
                RudderPosition = Angle.FromDegrees(rudder),
            };
        }
    }
}

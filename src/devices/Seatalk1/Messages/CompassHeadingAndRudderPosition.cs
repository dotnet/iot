// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Compass heading and rudder position message from autopilot
    /// </summary>
    public record CompassHeadingAndRudderPosition : SeatalkMessage
    {
        /// <inheritdoc />
        public override byte CommandByte => 0x9c;

        /// <inheritdoc />
        public override byte ExpectedLength => 4;

        /// <summary>
        /// Compass heading from the autopilot's internal sensor
        /// </summary>
        public Angle CompassHeading
        {
            get;
            init;
        }

        /// <summary>
        /// Rudder position. The autopilot may consistently report 0 here if no such sensor is fitted.
        /// </summary>
        public Angle RudderPosition
        {
            get;
            init;
        }

        /// <summary>
        /// Direction the boat is currently turning
        /// </summary>
        public TurnDirection TurnDirection
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
            long heading = (u & 0x3) * 90 + (vw & 0x3F) * 2 + ((u & 0x4) == 0x4 ? 1 : 0);
            Angle headingA = Angle.FromDegrees(heading);

            Messages.TurnDirection td = (u & 0x8) == 0x8 ? TurnDirection.Starboard : TurnDirection.Port;
            sbyte rudder = (sbyte)data[3];

            return new CompassHeadingAndRudderPosition()
            {
                CompassHeading = headingA,
                RudderPosition = Angle.FromDegrees(rudder),
                TurnDirection = td,
            };
        }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            byte[] data = new byte[4];
            data[0] = CommandByte;
            int heading = (int)Math.Round(CompassHeading.Normalize(true).Degrees);
            int u = 0;
            if (heading >= 270)
            {
                u = 3;
                heading -= 270;
            }
            else if (heading >= 180)
            {
                u = 2;
                heading -= 180;
            }
            else if (heading >= 90)
            {
                u = 1;
                heading -= 90;
            }

            if (TurnDirection == TurnDirection.Starboard)
            {
                u |= 0x8;
            }

            if (heading % 2 == 1)
            {
                u |= 0x4;
            }

            sbyte rudder = Convert.ToSByte(RudderPosition.Degrees);

            data[1] = (byte)(u << 4 | (ExpectedLength - 3));
            data[2] = (byte)((heading >> 1) & 0xff);
            data[3] = (byte)rudder;

            return data;
        }
    }
}

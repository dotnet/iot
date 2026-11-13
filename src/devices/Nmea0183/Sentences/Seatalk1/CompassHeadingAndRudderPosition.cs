// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Compass heading and rudder position message from autopilot
    /// </summary>
    public class CompassHeadingAndRudderPosition : SeatalkMessage
    {
        /// <summary>
        /// Standard constructor
        /// </summary>
        public CompassHeadingAndRudderPosition()
        {
            CompassHeading = Angle.FromDegrees(0);
            RudderPosition = Angle.FromDegrees(0);
            TurnDirection = TurnDirection.TurnToPort;
        }

        /// <summary>
        /// Constructs an instance from sentence data.
        /// </summary>
        public CompassHeadingAndRudderPosition(TalkerSentence sentence, DateTimeOffset time)
            : base(sentence.Fields, time)
        {
            VerifyPacket(sentence.Fields);

            uint u = (uint)(GetField(1) >> 4);
            uint vw = GetField(2);
            long heading = (u & 0x3) * 90 + (vw & 0x3F) * 2 + ((u & 0x4) == 0x4 ? 1 : 0);
            CompassHeading = Angle.FromDegrees(heading);
            TurnDirection = (u & 0x8) == 0x8 ? TurnDirection.TurnToStarboard : TurnDirection.TurnToPort;
            sbyte rudder = unchecked((sbyte)GetField(3));
            RudderPosition = Angle.FromDegrees(rudder);
            Valid = true;
        }

        /// <inheritdoc />
        public override bool ReplacesOlderInstance => true;

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

            if (TurnDirection == TurnDirection.TurnToStarboard)
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

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return $"Heading: {CompassHeading.Degrees:F0}°, Rudder: {RudderPosition.Degrees:F0}°, Turn: {TurnDirection}";
        }
    }
}

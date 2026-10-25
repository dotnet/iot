// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Compass heading and current autopilot course and parameters.
    /// </summary>
    public class CompassHeadingAutopilotCourse : SeatalkMessage
    {
        /// <summary>
        /// Standard constructor
        /// </summary>
        public CompassHeadingAutopilotCourse()
        {
            TurnDirection = TurnDirection.TurnToPort;
        }

        /// <summary>
        /// Constructs a new instance from sentence data.
        /// </summary>
        public CompassHeadingAutopilotCourse(TalkerSentence sentence, DateTimeOffset time)
            : base(sentence.Fields, time)
        {
            VerifyPacket(sentence.Fields);

            uint u = (uint)(GetField(1) >> 4);
            uint vw = GetField(2);
            uint lastBits = u switch
            {
                0xC => 2,
                0x4 => 1,
                _ => 0,
            };

            long heading = (u & 0x3) * 90 + (vw & 0x3F) * 2 + lastBits;
            CompassHeading = Angle.FromDegrees(heading);
            TurnDirection = (u & 0x8) == 0x8 ? TurnDirection.TurnToStarboard : TurnDirection.TurnToPort;

            long desiredCourse = ((vw >> 6) * 90) + (GetField(3) / 2);
            AutoPilotCourse = Angle.FromDegrees(desiredCourse);

            AutopilotStatus status = GetAutopilotStatus(GetField(4));
            AutopilotStatus = status;

            sbyte rudder = unchecked((sbyte)GetField(6));
            RudderPosition = Angle.FromDegrees(rudder);

            AutopilotAlarms alarms = AutopilotAlarms.None;
            byte almByte = GetField(5);
            if ((almByte & 0x4) == 0x4)
            {
                alarms |= AutopilotAlarms.OffCourse;
            }

            if ((almByte & 0x8) == 0x8)
            {
                alarms |= AutopilotAlarms.WindShift;
            }

            Alarms = alarms;
            AutoPilotType = GetField(8);
            if (status == AutopilotStatus.Undefined)
            {
                Valid = false;
            }
            else
            {
                Valid = true;
            }
        }

        /// <inheritdoc />
        public override bool ReplacesOlderInstance => true;

        /// <inheritdoc />
        public override byte CommandByte => 0x84;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x9;

        /// <summary>
        /// Heading of the internal sensor of the autopilot.
        /// </summary>
        public Angle CompassHeading { get; init; }

        /// <summary>
        /// Autopilot type, not really relevant
        /// </summary>
        public byte AutoPilotType { get; init; }

        /// <summary>
        /// Autopilot target course. Often not available when not in auto mode
        /// </summary>
        public Angle AutoPilotCourse { get; init; }

        /// <summary>
        /// Rudder position. Positive when turning to starboard
        /// </summary>
        public Angle RudderPosition { get; init; }

        /// <summary>
        /// Current autopilot status
        /// </summary>
        public AutopilotStatus AutopilotStatus { get; init; } = AutopilotStatus.Standby;

        /// <summary>
        /// Active alarms
        /// </summary>
        public AutopilotAlarms Alarms { get; init; }

        /// <summary>
        /// Turn direction. Not really reliable when no Rudder sensor is fitted.
        /// </summary>
        public TurnDirection TurnDirection { get; init; }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            byte[] data = new byte[ExpectedLength];
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
            data[2] = (byte)((heading >> 1) & 0x3f);

            byte almByte = 0;
            if ((Alarms & AutopilotAlarms.OffCourse) == AutopilotAlarms.OffCourse)
            {
                almByte |= 4;
            }

            if ((Alarms & AutopilotAlarms.WindShift) == AutopilotAlarms.WindShift)
            {
                almByte |= 8;
            }

            int vw = 0;
            double desiredCourse = AutoPilotCourse.Normalize(true).Degrees;
            if (desiredCourse >= 270)
            {
                vw = 0xC0;
                desiredCourse -= 270;
            }
            else if (desiredCourse >= 180)
            {
                vw = 0x80;
                desiredCourse -= 180;
            }
            else if (desiredCourse >= 90)
            {
                vw = 0x40;
                desiredCourse -= 90;
            }

            int xy = (int)Math.Round(desiredCourse * 2);

            data[2] |= (byte)vw;
            data[3] |= (byte)xy;
            data[4] = GetAutopilotStatusByte(AutopilotStatus);
            data[5] = almByte;
            data[6] = (byte)rudder;
            data[7] = 0;
            data[8] = AutoPilotType;

            return data;
        }

        private AutopilotStatus GetAutopilotStatus(byte z)
        {
            switch (z)
            {
                case 0:
                    return AutopilotStatus.Standby;
                case 2:
                    return AutopilotStatus.Auto;
                case 4:
                    return AutopilotStatus.InactiveWind;
                case 6:
                    return AutopilotStatus.Wind;
                case 8:
                    return AutopilotStatus.InactiveTrack;
                case 0x0a:
                    return AutopilotStatus.Track;
                case 0x10:
                    return AutopilotStatus.Calibration;
                default:
                    return AutopilotStatus.Undefined;
            }
        }

        private byte GetAutopilotStatusByte(AutopilotStatus status)
        {
            return status switch
            {
                AutopilotStatus.Standby => 0,
                AutopilotStatus.Auto => 2,
                AutopilotStatus.Track => 10,
                AutopilotStatus.Wind => 6,
                AutopilotStatus.Calibration => 16,
                AutopilotStatus.InactiveTrack => 8,
                AutopilotStatus.InactiveWind => 4,
                _ => 0,
            };
        }

        /// <inheritdoc />
        public override bool MatchesMessageType(IReadOnlyList<byte> data)
        {
            return base.MatchesMessageType(data) && GetAutopilotStatus(data[4]) != AutopilotStatus.Undefined && (data[5] & 0xF0) == 0 && (data[8] & 0xF0) == 0;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return $"Heading: {CompassHeading.Degrees:F0}°, AP course: {AutoPilotCourse.Degrees:F0}°, Status: {AutopilotStatus}, Alarms: {Alarms}";
        }
    }
}

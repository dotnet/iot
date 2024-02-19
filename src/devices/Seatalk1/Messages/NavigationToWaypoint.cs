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
    /// <summary>
    /// Navigation-to-waypoint message from a navigation computer
    /// </summary>
    public record NavigationToWaypoint : SeatalkMessage
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NavigationToWaypoint()
        {
        }

        /// <summary>
        /// Construct a new instance
        /// </summary>
        /// <param name="crossTrackError">Cross track error. Positive if right of desired track</param>
        /// <param name="bearingToDestination">Bearing to destination</param>
        /// <param name="bearingIsTrue">The bearing is true, false if it is magnetic</param>
        /// <param name="distanceToDestination">Distance to destination</param>
        public NavigationToWaypoint(Length? crossTrackError, Angle? bearingToDestination, bool bearingIsTrue, Length? distanceToDestination)
        {
            CrossTrackError = crossTrackError;
            BearingToDestination = bearingToDestination;
            BearingIsTrue = bearingIsTrue;
            DistanceToDestination = distanceToDestination;
        }

        /// <inheritdoc />
        public override byte CommandByte => 0x85;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x9;

        /// <summary>
        /// Cross track error. Positive if right of track
        /// </summary>
        public Length? CrossTrackError
        {
            get;
            init;
        }

        /// <summary>
        /// Bearing to destination waypoint.
        /// </summary>
        public Angle? BearingToDestination
        {
            get;
            init;
        }

        /// <summary>
        /// The bearing value is true (means: It is relative to true north)
        /// </summary>
        public bool BearingIsTrue
        {
            get;
            init;
        }

        /// <summary>
        /// Distance to destination
        /// </summary>
        public Length? DistanceToDestination
        {
            get;
            init;
        }

        /// <inheritdoc />
        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            byte flags = data[6];
            int vvv = ((data[1] & 0xF0) >> 4) | (data[2] << 4);
            Length? crossTrackDistance = Length.FromNauticalMiles(vvv / 100.0);
            if ((data[6] & 0x40) == 0x40)
            {
                crossTrackDistance = -crossTrackDistance;
            }

            if ((flags & 1) == 0)
            {
                crossTrackDistance = null;
            }

            bool bearingIsTrue = (data[3] & 0x8) == 0x8;
            int u = data[3] & 0x3;

            int remainder = (data[4] & 0xF) << 4;
            remainder |= (data[3] & 0xF0) >> 4;

            Angle? bearingToDestination = Angle.FromDegrees((u * 90) + (remainder / 2.0));

            if ((flags & 2) == 0)
            {
                bearingToDestination = null;
            }

            int dist = data[5] << 4;
            dist |= (data[4] >> 4);

            Length? distance;
            if ((flags & 4) == 0)
            {
                distance = null;
            }
            else if ((data[6] & 0x10) == 0x10)
            {
                distance = Length.FromNauticalMiles(dist / 100.0);
            }
            else
            {
                distance = Length.FromNauticalMiles(dist / 10.0);
            }

            return new NavigationToWaypoint()
            {
                BearingIsTrue = bearingIsTrue,
                BearingToDestination = bearingToDestination,
                CrossTrackError = crossTrackDistance,
                DistanceToDestination = distance,
            };
        }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            // 85  X6  XX  VU ZW ZZ YF 00 yf
            byte flags = 0;
            byte[] data = new byte[9];
            data[0] = CommandByte;
            data[1] = (byte)(ExpectedLength - 3);
            if (CrossTrackError.HasValue)
            {
                double nmTimes100 = CrossTrackError.Value.NauticalMiles * 100.0;
                int v = (int)Math.Round(Math.Abs(nmTimes100));
                if (v > 0xfff)
                {
                    v = 0xfff;
                }

                data[1] = (byte)((v & 0x00f) << 4 | 0x6); // This is the attr byte
                data[2] = (byte)(v >> 4);
                if (nmTimes100 < 0) // Xtrack error negative -> Steer right to correct
                {
                    data[6] |= 0x40; // this is indicated on y bit 2
                }

                flags |= 1;
                if (Math.Abs(CrossTrackError.Value.NauticalMiles) >= 0.3)
                {
                    flags |= 8;
                }
            }

            if (BearingToDestination.HasValue)
            {
                double angle = BearingToDestination.Value.Degrees;
                int u = 0;
                if (angle >= 270)
                {
                    u = 3;
                    angle -= 270;
                }
                else if (angle >= 180)
                {
                    u = 2;
                    angle -= 180;
                }
                else if (angle >= 90)
                {
                    u = 1;
                    angle -= 90;
                }

                int lowerPart = (int)Math.Round(angle * 2);

                data[3] |= (byte)u;
                if (BearingIsTrue)
                {
                    data[3] |= 0x8;
                }

                data[3] |= (byte)(lowerPart << 4); // V, lower nibble of remainder
                data[4] |= (byte)((lowerPart >> 4) & 0xF); // upper nibble of remainder (upper nibble of this byte is used in DistanceToDestination)
                flags |= 2;
            }

            if (DistanceToDestination.HasValue)
            {
                double distanceNm = DistanceToDestination.Value.NauticalMiles;
                if (distanceNm <= 9.9)
                {
                    int decoding = (int)Math.Round(distanceNm * 100.0);
                    data[4] |= (byte)((decoding & 0x00F) << 4);
                    data[5] = (byte)(((decoding & 0xFF0) >> 4));
                    data[6] |= 0x10; // Set scale bit Y
                }
                else
                {
                    int decoding = (int)Math.Round(distanceNm * 10.0);
                    data[4] |= (byte)((decoding & 0x00F) << 4);
                    data[5] = (byte)(((decoding & 0xFF0) >> 4));
                    // Bit in Y is not set
                }

                flags |= 4;
            }

            data[6] |= flags;
            data[7] = 0;
            data[8] = (byte)(~data[6]);

            return data;
        }

        /// <inheritdoc />
        public override bool MatchesMessageType(IReadOnlyList<byte> data)
        {
            return base.MatchesMessageType(data) && data[6] == (byte)(~data[8]);
        }
    }
}

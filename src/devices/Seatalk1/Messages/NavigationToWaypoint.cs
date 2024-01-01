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
    public record NavigationToWaypoint : SeatalkMessage
    {
        public override byte CommandByte => 0x85;
        public override byte ExpectedLength => 0x9;

        public Length? CrossTrackError
        {
            get;
            init;
        }

        public Angle? BearingToDestination
        {
            get;
            init;
        }

        public bool BearingIsTrue
        {
            get;
            init;
        }

        public Length? DistanceToDestination
        {
            get;
            init;
        }

        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            return new NavigationToWaypoint();
        }

        public override byte[] CreateDatagram()
        {
            // 85  X6  XX  VU ZW ZZ YF 00 yf
            byte flags = 0;
            byte[] data = new byte[9];
            data[0] = CommandByte;
            data[1] = ExpectedLength;
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
    }
}

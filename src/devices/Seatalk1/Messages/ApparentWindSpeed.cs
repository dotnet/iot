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
    public record ApparentWindSpeed : SeatalkMessage
    {
        private readonly Speed _apparentSpeed;

        public ApparentWindSpeed()
        {
            ApparentSpeed = Speed.FromKnots(0); // Default unit is knots
        }

        public ApparentWindSpeed(Speed apparentSpeed)
        {
            ApparentSpeed = apparentSpeed;
        }

        public override byte CommandByte => 0x11;
        public override byte ExpectedLength => 0x4;

        /// <summary>
        /// Apparent wind angle, relative to bow.
        /// Positive for right of bow (wind from starboard)
        /// </summary>
        public Speed ApparentSpeed
        {
            get
            {
                return _apparentSpeed;
            }
            init
            {
                if (value < Speed.Zero)
                {
                    throw new ArgumentException("Speed cannot be negative");
                }

                _apparentSpeed = value;
            }
        }

        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            Speed spd = Speed.Zero;
            if ((data[2] & 0x80) == 0x80) // Speed is given in m/s
            {
                spd = Speed.FromMetersPerSecond((data[2] & 0x7F) + (data[3] / 10.0));
            }
            else // Speed is in knots
            {
                spd = Speed.FromKnots((data[2] & 0x7F) + (data[3] / 10.0));
            }

            return this with
            {
                ApparentSpeed = spd
            };
        }

        public override byte[] CreateDatagram()
        {
            double rounded = Math.Round(ApparentSpeed.Knots, 1);
            byte byte1 = (byte)rounded;
            double remainder = Math.Round(Math.Abs(rounded - Math.Round(rounded)), 1);
            byte byte2 = (byte)(remainder * 10.0);
            return new byte[]
            {
                CommandByte, (byte)(ExpectedLength - 3), byte1, byte2
            };
        }
    }
}

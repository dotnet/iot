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
    /// Apparent wind angle, as provided by the wind instrument in the masthead of a sailboat.
    /// </summary>
    public record ApparentWindAngle : SeatalkMessage
    {
        private readonly Angle _apparentAngle;

        /// <summary>
        /// Create a default instance
        /// </summary>
        public ApparentWindAngle()
        {
            ApparentAngle = Angle.Zero;
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="apparentAngle">Apparent wind angle (positive for wind from starboard)</param>
        public ApparentWindAngle(Angle apparentAngle)
        {
            ApparentAngle = apparentAngle;
        }

        /// <inheritdoc />
        public override byte CommandByte => 0x10;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x4;

        /// <summary>
        /// Apparent wind angle, relative to bow. 0 means wind comes directly from ahead.
        /// Positive for right of bow (wind from starboard)
        /// </summary>
        public Angle ApparentAngle
        {
            get
            {
                return _apparentAngle;
            }
            init
            {
                value = value.Normalize(false);
                _apparentAngle = value;
            }
        }

        /// <inheritdoc />
        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            uint angle10u = Convert.ToUInt32(data[2] << 8 | data[3]);
            if ((angle10u & 0x8000) != 0)
            {
                angle10u |= 0xFFFF0000;
            }

            int angle10 = unchecked((int)angle10u);
            Angle angle = Angle.FromDegrees(angle10 / 2.0);
            return new ApparentWindAngle()
            {
                ApparentAngle = angle,
            };
        }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            short angle10 = (short)Math.Round(ApparentAngle.Degrees * 2);
            return new byte[]
            {
                CommandByte, (byte)(ExpectedLength - 3), (byte)(angle10 >> 8), (byte)(angle10 & 0xFF)
            };
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// ST-2000 forwards this message when it receives the NMEA wind sequences.
    /// The exact meaning is unknown as of yet.
    /// Format:
    /// 11-80-0X, where X is a number between 0 and 4, note that the <see cref="ApparentWindSpeed"/> encodes as 11-01-XX-0Y
    /// </summary>
    public record AutopilotWindStatus : SeatalkMessage
    {
        /// <summary>
        /// Constructs a new instance of this class
        /// </summary>
        public AutopilotWindStatus()
        {
            Status = 0;
        }

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="status">The status to report</param>
        public AutopilotWindStatus(int status)
        {
            Status = status;
        }

        /// <summary>
        /// The status byte (meaning unknown)
        /// </summary>
        public int Status
        {
            get;
        }

        /// <inheritdoc />
        public override byte CommandByte => 0x11;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x03;

        /// <inheritdoc />
        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            int status = data[2];
            return new AutopilotWindStatus(status);
        }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            int status = Status & 0xFF;
            return new byte[] { CommandByte, 0, (byte)status, };
        }
    }
}

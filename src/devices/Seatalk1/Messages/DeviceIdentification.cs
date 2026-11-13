// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Query/Reply to device identification broadcast
    /// </summary>
    public record class DeviceIdentification : SeatalkMessage
    {
        /// <inheritdoc />
        public override byte CommandByte => 0xA4;

        /// <inheritdoc />
        public override byte ExpectedLength => 5;

        /// <summary>
        /// The device type of someone replying to this message.
        /// </summary>
        public int DeviceType
        {
            get;
            set;
        }

        /// <summary>
        /// True if this is an actual reply from a device
        /// </summary>
        public bool IsReply => DeviceType != 0;

        /// <inheritdoc />
        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            return new DeviceIdentification()
            {
                DeviceType = data[2]
            };
        }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            // This is the query command
            return new byte[] { CommandByte, 2, (byte)DeviceType, 0, 0 };
        }
    }
}

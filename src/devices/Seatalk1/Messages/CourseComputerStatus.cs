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
    /// Course computer status. Various autopilots may use this message for different purposes.
    /// </summary>
    public record CourseComputerStatus : SeatalkMessage
    {
        /// <inheritdoc />
        public override byte CommandByte => 0x83;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x0a; // this message has 10 bytes, despite only 3 used.

        /// <summary>
        /// Active warnings
        /// </summary>
        public CourseComputerWarnings Warnings
        {
            get;
            init;
        }

        /// <inheritdoc />
        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            Logger.LogInformation($"Course computer warning msg: {string.Join("-", data.Select(x => x.ToString("X2")))}");
            return this with
            {
                Warnings = (CourseComputerWarnings)data[2],
            };
        }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            return new byte[]
            {
                CommandByte, (byte)(ExpectedLength - 3), (byte)Warnings, 0, 0, 0, 0, 0, 0, 0
            };
        }

        /// <inheritdoc />
        public override bool MatchesMessageType(IReadOnlyList<byte> data)
        {
            return base.MatchesMessageType(data) && data[3] == 0;
        }
    }
}

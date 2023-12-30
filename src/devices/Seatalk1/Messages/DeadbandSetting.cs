// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    public record DeadbandSetting : SeatalkMessage
    {
        public override byte CommandByte => 0x87;
        public override byte ExpectedLength => 0x03;

        public DeadbandMode Mode { get; init; }

        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            VerifyPacket(data);
            DeadbandMode mode = DeadbandMode.None;
            if (data[2] == 1)
            {
                mode = DeadbandMode.Automatic;
            }
            else if (data[2] == 2)
            {
                mode = DeadbandMode.Minimal;
            }

            return new DeadbandSetting()
            {
                Mode = mode,
            };
        }

        public override byte[] CreateDatagram()
        {
            throw new NotSupportedException("Send the respective keycodes instead (0x09 or 0x0a)");
        }

        public override bool MatchesMessageType(IReadOnlyList<byte> data)
        {
            return base.MatchesMessageType(data) && data[1] == 0x0;
        }
    }
}

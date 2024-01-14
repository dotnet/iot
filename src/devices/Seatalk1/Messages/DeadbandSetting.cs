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
    /// Sent by the auto pilot when the automatic deadband mode is changed.
    /// If set to "Automatic" (the default) the Autopilot tries to compensate for the effect of waves and reduces the
    /// rudder movement to a minimum. If set to "Minimum", the Autopilot tries to minimize the off-track error, at the
    /// expense of additional movements and thus higher power consumption. The latter is particularly useful when navigating
    /// on narrow waterways.
    /// </summary>
    /// <remarks>
    /// To change the setting, send the respective keycodes (0x09 or 0x0a) instead of a message with the desired new value.
    /// </remarks>
    public record DeadbandSetting : SeatalkMessage
    {
        /// <inheritdoc />
        public override byte CommandByte => 0x87;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x03;

        /// <summary>
        /// The new mode
        /// </summary>
        public DeadbandMode Mode { get; init; }

        /// <inheritdoc />
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

        /// <summary>
        /// Creates a Deadband status change message.
        /// </summary>
        /// <returns>The datagram</returns>
        public override byte[] CreateDatagram()
        {
            byte mode = Mode switch
            {
                DeadbandMode.Automatic => 1,
                DeadbandMode.Minimal => 2,
                _ => 0,
            };

            return new byte[]
            {
                CommandByte, (byte)(ExpectedLength - 3), mode
            };
        }

        /// <inheritdoc />
        public override bool MatchesMessageType(IReadOnlyList<byte> data)
        {
            return base.MatchesMessageType(data) && data[1] == 0x0;
        }
    }
}

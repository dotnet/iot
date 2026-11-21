// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Iot.Device.Nmea0183;

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
    public class DeadbandSetting : SeatalkMessage
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DeadbandSetting()
        {
        }

        /// <summary>
        /// Initializes a new instance from sentence data.
        /// </summary>
        public DeadbandSetting(TalkerSentence sentence, DateTimeOffset time)
            : base(sentence.Fields, time)
        {
            VerifyPacket(sentence.Fields);

            Mode = GetField(2) switch
            {
                1 => DeadbandMode.Automatic,
                2 => DeadbandMode.Minimal,
                _ => DeadbandMode.None,
            };
            Valid = true;
        }

        /// <inheritdoc />
        public override bool ReplacesOlderInstance => true;

        /// <inheritdoc />
        public override byte CommandByte => 0x87;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x03;

        /// <summary>
        /// The new mode
        /// </summary>
        public DeadbandMode Mode { get; init; }

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

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return $"Deadband mode: {Mode}";
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Query/Reply to device identification broadcast
    /// </summary>
    public class DeviceIdentification : SeatalkMessage
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DeviceIdentification()
        {
        }

        /// <summary>
        /// Initializes a new instance from sentence data.
        /// </summary>
        public DeviceIdentification(TalkerSentence sentence, DateTimeOffset time)
            : base(sentence.Fields, time)
        {
            VerifyPacket(sentence.Fields);
            DeviceType = GetField(2);
            Valid = true;
        }

        /// <inheritdoc />
        public override bool ReplacesOlderInstance => false;

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
        public override byte[] CreateDatagram()
        {
            // This is the query command
            return new byte[] { CommandByte, 2, (byte)DeviceType, 0, 0 };
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return IsReply ? $"Device identification reply: type 0x{DeviceType:X2}" : "Device identification query";
        }
    }
}

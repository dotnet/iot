// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Iot.Device.Nmea0183;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Course computer status. Various autopilots may use this message for different purposes.
    /// </summary>
    public class CourseComputerStatus : SeatalkMessage
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public CourseComputerStatus()
        {
        }

        /// <summary>
        /// Initializes a new instance from sentence data.
        /// </summary>
        public CourseComputerStatus(TalkerSentence sentence, DateTimeOffset time)
            : base(sentence.Fields, time)
        {
            VerifyPacket(sentence.Fields);
            Warnings = (CourseComputerWarnings)GetField(2);
            Valid = true;
        }

        /// <inheritdoc />
        public override bool ReplacesOlderInstance => true;

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

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return $"Course computer warnings: {Warnings}";
        }
    }
}

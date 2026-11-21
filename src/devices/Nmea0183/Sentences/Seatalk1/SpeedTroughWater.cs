// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183;
using UnitsNet;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Speed trough water (data from log instrument)
    /// </summary>
    public class SpeedTroughWater : SeatalkMessage
    {
        internal SpeedTroughWater()
        {
            Speed = Speed.FromKnots(0); // Because the decoding converts to knots
        }

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="speed">The current speed</param>
        public SpeedTroughWater(Speed speed)
        {
            Speed = speed;
            Forwarded = false;
        }

        /// <summary>
        /// Constructs a new instance from sentence data.
        /// </summary>
        public SpeedTroughWater(TalkerSentence sentence, DateTimeOffset time)
            : base(sentence.Fields, time)
        {
            VerifyPacket(sentence.Fields);
            double speedvalue = (GetField(2) << 8) | GetField(3);
            speedvalue /= 10.0;
            Speed = Speed.FromKnots(speedvalue);
            Forwarded = (GetField(1) & 0x80) != 0;
            Valid = true;
        }

        /// <inheritdoc />
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// The current speed trough water
        /// </summary>
        public Speed Speed
        {
            get;
        }

        /// <summary>
        /// True if the message was forwarded from somewhere
        /// </summary>
        public bool Forwarded
        {
            get;
            init;
        }

        /// <inheritdoc />
        public override byte CommandByte => 0x20;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x4;

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            double v = Speed.Knots * 10.0;
            int v1 = (int)v;
            byte b1 = (byte)(v1 >> 8);
            byte b2 = (byte)(v1 & 0xff);
            return new byte[] { CommandByte, (byte)(Forwarded ? 0x81 : 0x1), b1, b2 };
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return $"Speed through water: {Speed.Knots:F1} kn (Forwarded: {Forwarded})";
        }
    }
}

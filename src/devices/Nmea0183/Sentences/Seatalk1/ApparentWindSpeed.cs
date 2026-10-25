// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183;
using UnitsNet;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// This message provides the apparent wind speed as measured by the wind instrument on the masthead of a sailboat.
    /// </summary>
    public class ApparentWindSpeed : SeatalkMessage
    {
        private readonly Speed _apparentSpeed;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public ApparentWindSpeed()
        {
            ApparentSpeed = Speed.FromKnots(0); // Default unit is knots
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="apparentSpeed">Apparent wind speed (default unit is knots)</param>
        public ApparentWindSpeed(Speed apparentSpeed)
        {
            ApparentSpeed = apparentSpeed;
        }

        /// <summary>
        /// Construct from a Seatalk sentence.
        /// </summary>
        public ApparentWindSpeed(TalkerSentence sentence, DateTimeOffset time)
            : base(sentence.Fields, time)
        {
            VerifyPacket(sentence.Fields);

            Speed spd;
            byte b2 = GetField(2);
            byte b3 = GetField(3);
            if ((b2 & 0x80) == 0x80)
            {
                spd = Speed.FromMetersPerSecond((b2 & 0x7F) + (b3 / 10.0));
            }
            else
            {
                spd = Speed.FromKnots((b2 & 0x7F) + (b3 / 10.0));
            }

            ApparentSpeed = spd;
            Valid = true;
        }

        /// <inheritdoc />
        public override bool ReplacesOlderInstance => true;

        /// <inheritdoc />
        public override byte CommandByte => 0x11;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x4;

        /// <summary>
        /// Apparent wind angle, relative to bow.
        /// Positive for right of bow (wind from starboard)
        /// </summary>
        public Speed ApparentSpeed
        {
            get
            {
                return _apparentSpeed;
            }
            init
            {
                if (value < Speed.Zero)
                {
                    throw new ArgumentException("Speed cannot be negative");
                }

                _apparentSpeed = value;
            }
        }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            double rounded = Math.Round(ApparentSpeed.Knots, 1);
            byte byte1 = (byte)rounded;
            double remainder = Math.Round(Math.Abs(rounded - Math.Round(rounded)), 1);
            byte byte2 = (byte)(remainder * 10.0);
            return new byte[]
            {
                CommandByte, (byte)(ExpectedLength - 3), byte1, byte2
            };
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return $"Apparent wind speed: {ApparentSpeed.Knots:F1} kn";
        }
    }
}

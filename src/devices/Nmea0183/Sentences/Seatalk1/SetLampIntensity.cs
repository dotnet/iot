// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Iot.Device.Nmea0183;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Set lamp intensity message. This message can be used to instruct all devices on the bus to adjust their background light.
    /// This message is only sent once from a controlling device.
    /// </summary>
    public class SetLampIntensity : SeatalkMessage
    {
        private readonly DisplayBacklightLevel _intensity;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SetLampIntensity()
        {
            Intensity = DisplayBacklightLevel.Off;
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="intensity">New brightness setting</param>
        public SetLampIntensity(DisplayBacklightLevel intensity)
        {
            Intensity = intensity;
        }

        /// <summary>
        /// Constructs a new instance from sentence data.
        /// </summary>
        public SetLampIntensity(TalkerSentence sentence, DateTimeOffset time)
            : base(sentence.Fields, time)
        {
            VerifyPacket(sentence.Fields);
            Intensity = (DisplayBacklightLevel)(GetField(2) & 0xF);
            Valid = true;
        }

        /// <inheritdoc />
        public override bool ReplacesOlderInstance => true;

        /// <inheritdoc />
        public override byte CommandByte => 0x30;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x3;

        /// <summary>
        /// The new intensity.
        /// </summary>
        public DisplayBacklightLevel Intensity
        {
            get
            {
                return _intensity;
            }
            init
            {
                if (!Enum.IsDefined(typeof(DisplayBacklightLevel), value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The display backlight value setting is unknown");
                }

                _intensity = value;
            }
        }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            return new byte[]
            {
                CommandByte, (byte)(ExpectedLength - 3), (byte)Intensity
            };
        }

        /// <inheritdoc />
        public override bool MatchesMessageType(IReadOnlyList<byte> data)
        {
            return base.MatchesMessageType(data) && data[2] is 0 or 0x4 or 0x8 or 0xC;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return $"Lamp intensity: {Intensity}";
        }
    }
}

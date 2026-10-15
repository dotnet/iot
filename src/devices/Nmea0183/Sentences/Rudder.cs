// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

#pragma warning disable CS1591

namespace Iot.Device.Nmea0183.Sentences
{
    public class Rudder : Nmea2000PackedMessage
    {
        private byte _directionOrder;
        public const int HexId = 0x1F10D;
        public override bool ReplacesOlderInstance => true;

        public Angle? ActualAngle
        {
            get;
            set;
        }

        public Angle? DesiredAngle
        {
            get;
            set;
        }

        public byte Instance
        {
            get;
            set;
        }

        /// <summary>
        /// Current direction the rudder shall move
        /// 0 = No order
        /// 1 = To starboard
        /// 2 = To port
        /// 7 = Unknown/Not set
        /// </summary>
        public TurnDirection DirectionOrder
        {
            get
            {
                return _directionOrder switch
                {
                    0 => TurnDirection.NoCommand,
                    1 => TurnDirection.TurnToStarboard,
                    2 => TurnDirection.TurnToPort,
                    _ => TurnDirection.NoCommand,
                };
            }
            set
            {
                _directionOrder = value switch
                {
                    TurnDirection.TurnToPort => 2,
                    TurnDirection.TurnToStarboard => 1,
                    _ => 0,
                };
            }
        }

        public Rudder(Angle? actualAngle, Angle? desiredAngle, TurnDirection directionOrder, byte instance)
        {
            Instance = instance;
            ActualAngle = actualAngle;
            DesiredAngle = desiredAngle;
            DirectionOrder = directionOrder;
            Valid = true;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public Rudder(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public Rudder(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadByteFromHexString(data, 0, out byte b))
            {
                Instance = b;
            }

            if (ReadByteFromHexString(data, 2, out b))
            {
                _directionOrder = (byte)(b & 0x7);
            }
            else
            {
                _directionOrder = 7;
            }

            if (ReadShortFromHexString(data, 4, out short v))
            {
                DesiredAngle = Angle.FromRadians(0.0001 * v);
            }

            if (ReadShortFromHexString(data, 8, out v))
            {
                ActualAngle = Angle.FromRadians(0.0001 * v);
            }

            Valid = true;
        }

        public override string ToNmeaParameterList()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(WriteByteToHex(Instance));
            sb.Append(WriteByteToHex((byte)(_directionOrder | 0xF8)));
            short? angle = null;
            if (DesiredAngle.HasValue)
            {
                angle = (short)Math.Round(DesiredAngle.Value.Radians / 0.0001);
            }

            sb.Append(WriteShortToHex(angle));

            angle = null;
            if (ActualAngle.HasValue)
            {
                angle = (short)Math.Round(ActualAngle.Value.Radians / 0.0001);
            }

            sb.Append(WriteShortToHex(angle));
            sb.Append(WriteUshortToHex(null)); // Reserved

            return base.ToNmeaParameterList() + sb.ToString();
        }

        public override string ToReadableContent()
        {
            return $"Rudder Angle: {ActualAngle}, Desired Angle {DesiredAngle}";
        }

        public override uint Identifier => HexId;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// This message wraps a Seatalk1 binary message into an NMEA0183 message
    /// This is supported by some OpenCPN plugins, such as "Raymarine Autopilot"
    /// The message is always sent with the identifier $STALK
    /// </summary>
    public class SeatalkNmeaMessage : NmeaSentence
    {
        /// <summary>
        /// This sentence ID "ALK"
        /// </summary>
        public static SentenceId Id => new SentenceId("ALK");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new MWV sentence
        /// </summary>
        public SeatalkNmeaMessage(byte[] datagram)
            : base(Nmea0183.TalkerId.Seatalk, Id, DateTimeOffset.UtcNow)
        {
            Datagram = datagram;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public SeatalkNmeaMessage(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Standard decoding constructor
        /// </summary>
        public SeatalkNmeaMessage(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            List<byte> datagram = new List<byte>(20);
            foreach (var field in fields)
            {
                if (byte.TryParse(field, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                {
                    datagram.Add(b);
                }
                else
                {
                    throw new FormatException($"Invalid value in $TALK sequence: {field} is not a valid hex number");
                }
            }

            Datagram = datagram.ToArray();
            Valid = true;
        }

        /// <summary>
        /// Directly create a message from a datagram
        /// </summary>
        /// <param name="datagram">Datagram, byte array</param>
        /// <param name="time">The current time</param>
        public SeatalkNmeaMessage(byte[] datagram, DateTimeOffset time)
            : base(TalkerId.Seatalk,  Id, time)
        {
            Datagram = datagram;
            Valid = true;
        }

        /// <summary>
        /// This message type can embed various messages, some of them are not repeating (e.g. Keypresses)
        /// </summary>
        public override bool ReplacesOlderInstance => false;

        /// <inheritdoc />
        public override string ToNmeaParameterList()
        {
            return string.Join(",", Datagram.Select(x => x.ToString("X2", CultureInfo.InvariantCulture)));
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return $"$STALK," + string.Join(",", Datagram.Select(x => x.ToString("X2", CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// The Seatalk1 raw Datagram (up to 18 bytes)
        /// </summary>
        public byte[] Datagram { get; private set; }
    }
}

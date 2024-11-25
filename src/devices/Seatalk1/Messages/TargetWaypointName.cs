// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// This message reports the last 4 characters of the next waypoint
    /// </summary>
    public record TargetWaypointName : SeatalkMessage
    {
        internal TargetWaypointName()
        {
            Name = "0000";
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="name">The name of the waypoint. Only the last 4 chars will be transmitted.</param>
        public TargetWaypointName(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public override byte CommandByte => 0x82;

        /// <inheritdoc />
        public override byte ExpectedLength => 8;

        /// <summary>
        /// The name of the waypoint. Only the last 4 characters are transmitted. If the string is shorter than 4, it is padded with zeros.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            // 82  05  XX  xx YY yy ZZ zz   Target waypoint name, with XX+yy == 0xff etc.
            int c1, c2, c3, c4;
            c1 = data[2] & 0x3F;
            c2 = ((data[4] & 0xF) * 4) + ((data[2] & 0xC0) / 64);
            c3 = ((data[6] & 0x3) * 16) + ((data[4] & 0xF0) / 16);
            c4 = (data[6] & 0xFC) / 4;
            c1 += 0x30;
            c2 += 0x30;
            c3 += 0x30;
            c4 += 0x30;
            string name = $"{(char)c1}{(char)c2}{(char)c3}{(char)c4}";
            return new TargetWaypointName(name);
        }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            string chars;
            if (Name.Length > 4)
            {
                chars = Name.Substring(Name.Length - 4, 4);
            }
            else
            {
                chars = Name.Substring(0, Name.Length);
                chars = chars + new String('0', 4 - Name.Length);
            }

            var result = new byte[]
            {
                CommandByte, (byte)(ExpectedLength - 3), 0x0, 0xff, 0x0, 0xff, 0x0, 0xff
            };

            int c1 = ConvertLetter(chars[0]);
            int c2 = ConvertLetter(chars[1]);
            int c3 = ConvertLetter(chars[2]);
            int c4 = ConvertLetter(chars[3]);

            result[2] = (byte)((c1) + (c2 << 6));
            result[4] = (byte)((c2 / 4) + (c3 << 4));
            result[6] = (byte)((c3 >> 4) + (c4 << 2));

            result[3] = (byte)(0xff - result[2]);
            result[5] = (byte)(0xff - result[4]);
            result[7] = (byte)(0xff - result[6]);

            return result;
        }

        private int ConvertLetter(char c)
        {
            c = Char.ToUpper(c, CultureInfo.InvariantCulture);
            // We have 6 bit per char, and the ascii table is offset by 0x30 (which is the digit "0")
            // The last letter that could be represented is the small "o", but I'm not sure anything above "Z" (0x5a) is valid.
            if (c < 0x30 || (c - 0x30) > 0x3F)
            {
                return 0; // We cannot use this character, replace with "0"
            }

            return c - 0x30;
        }

        /// <inheritdoc />
        public override bool MatchesMessageType(IReadOnlyList<byte> data)
        {
            // The data bytes 2-7 must pairwise sum to 0xff, otherwise the message is corrupted.
            return base.MatchesMessageType(data) && (data[2] + data[3]) == 0xFF && (data[4] + data[5]) == 0xFF && (data[6] + data[7]) == 0xFF;
        }
    }
}

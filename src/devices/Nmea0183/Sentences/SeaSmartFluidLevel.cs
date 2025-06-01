// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// SeaSmart message for fluid levels of a tank (Wrapped NMEA2000 message)
    /// For format, see also https://github.com/ttlappalainen/NMEA2000
    /// </summary>
    public class SeaSmartFluidLevel : ProprietaryMessage
    {
        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0x01F211;

        /// <summary>
        /// Constructs a new sentence
        /// </summary>
        public SeaSmartFluidLevel(FluidData data)
        {
            Level = data.Level;
            Type = data.Type;
            TankNumber = data.TankNumber;
            Valid = true;
            TankVolume = data.Volume;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public SeaSmartFluidLevel(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public SeaSmartFluidLevel(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            string subMessage = ReadString(field);
            if (!int.TryParse(subMessage, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result) || result != Identifier)
            {
                Valid = false;
                return;
            }

            string timeStamp = ReadString(field);

            if (Int32.TryParse(timeStamp, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int time1))
            {
                MessageTimeStamp = time1;
            }

            ReadString(field); // Ignore next field

            string data = ReadString(field);

            if (ReadFromHexString(data, 0, 2, false, out int combined))
            {
                TankNumber = combined & 0x0f;
                Type = (FluidType)((combined >> 4) & 0x0f);
            }

            if (ReadFromHexString(data, 2, 4, false, out int level))
            {
                Level = Ratio.FromPercent(level);
            }

            if (ReadFromHexString(data, 6, 8, false, out int volume))
            {
                volume = BinaryPrimitives.ReadInt32BigEndian(new byte[]
                {
                    (byte)(volume & 0xff),
                    (byte)(volume >> 8),
                    (byte)(volume >> 16),
                    (byte)(volume >> 24),
                });
                TankVolume = Volume.FromLiters(volume / 10.0d);
            }

            Valid = true;
        }

        /// <summary>
        /// The NMEA2000 Sentence identifier for this message
        /// </summary>
        public override int Identifier => HexId;

        /// <summary>
        /// The timestamp for the NMEA 2000 message
        /// </summary>
        public int MessageTimeStamp
        {
            get;
            private set;
        }

        /// <summary>
        /// Tank level in percent (100% = Full, 0% = empty)
        /// </summary>
        public Ratio? Level
        {
            get;
            private set;
        }

        /// <summary>
        /// Instance number of the tank
        /// </summary>
        public int TankNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// The type of fluid in the tank
        /// </summary>
        public FluidType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// The volume of the tank if full.
        /// </summary>
        public Volume? TankVolume
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns false for this message (because PCDIN messages are identified based on their inner message)
        /// </summary>
        public override bool ReplacesOlderInstance => false;

        /// <inheritdoc />
        public override string ToNmeaParameterList()
        {
            if (Valid)
            {
                string timeStampText = MessageTimeStamp.ToString("X8", CultureInfo.InvariantCulture);
                // $PCDIN,01F211,000C7E1B,02,000000FFFF407FFF*XX
                //                           1-2---3-------4-
                // 1) Fluid type and tank number (4 bit each)
                // 2) Level in %
                // 3) Capacity in 0.1 liters
                // 4) Reserved
                int combination = (((int)Type << 4) & 0xF0) | ((int)TankNumber & 0x0F);
                string ftypeString = combination.ToString("X2", CultureInfo.InvariantCulture);
                int l = (int)Math.Round(Level.HasValue ? Level.Value.Percent : 0);
                string level = l.ToString("X4", CultureInfo.InvariantCulture);
                int vol = (int)Math.Round(TankVolume.HasValue ? TankVolume.Value.Liters * 10 : 0);
                string capacity = vol.ToString("X8", CultureInfo.InvariantCulture);
                string capacitySwapped = capacity.Substring(6, 2) + capacity.Substring(4, 2) +
                                         capacity.Substring(2, 2) + capacity.Substring(0, 2);
                return "01F211," + timeStampText + ",02," + ftypeString + level + capacitySwapped + "FF";

            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Tank for {Type} number {TankNumber}: Remaining capacity {Level}";
            }

            return "No valid data";
        }
    }
}

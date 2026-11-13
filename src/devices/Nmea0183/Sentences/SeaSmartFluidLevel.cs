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
    public class SeaSmartFluidLevel : Nmea2000PackedMessage
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

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadByteFromHexString(data, 0, out byte combined))
            {
                TankNumber = combined & 0x0f;
                Type = (FluidType)((combined >> 4) & 0x0f);
            }

            if (ReadShortFromHexString(data, 2, out short s))
            {
                Level = Ratio.FromPercent(s * 0.004);
            }

            if (ReadUintFromHexString(data, 6, out uint volume))
            {
                TankVolume = Volume.FromLiters(volume / 10.0d);
            }

            Valid = true;
        }

        /// <summary>
        /// The NMEA2000 Sentence identifier for this message
        /// </summary>
        public override uint Identifier => HexId;

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

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

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
                short l = (short)Math.Round(Level.HasValue ? Level.Value.Percent / 0.004 : 0);
                string level = WriteShortToHex(l);
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

        /// <summary>
        /// Returns this message as a <see cref="FluidData"/> instance
        /// </summary>
        public FluidData AsFluidData()
        {
            return new FluidData(Type, Level.GetValueOrDefault(), TankVolume.GetValueOrDefault(), TankNumber,
                Type is FluidType.Fuel or FluidType.FuelGasoline or FluidType.Oil or FluidType.Water);
        }
    }
}

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
    /// An extended engine data message, using a PCDIN sequence (supported by some NMEA0183 to NMEA2000 bridges)
    /// This message mostly provides the RPM value and can be sent with a high frequency.
    /// </summary>
    public class SeaSmartEngineFast : ProprietaryMessage
    {
        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0x01F200;

        /// <summary>
        /// Constructs a new sentence
        /// </summary>
        public SeaSmartEngineFast(RotationalSpeed speed, int engineNumber, Ratio pitch)
            : base()
        {
            RotationalSpeed = speed;
            EngineNumber = engineNumber;
            PropellerPitch = pitch;
            Valid = true;
            MessageTimeStamp = Environment.TickCount;
        }

        /// <summary>
        /// Constructs a new sentence
        /// </summary>
        public SeaSmartEngineFast(EngineData data)
        {
            RotationalSpeed = data.Revolutions;
            EngineNumber = data.EngineNo;
            PropellerPitch = data.Pitch;
            MessageTimeStamp = data.MessageTimeStamp;
            Valid = true;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public SeaSmartEngineFast(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public SeaSmartEngineFast(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
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

            if (ReadFromHexString(data, 0, 2, false, out int engineNo))
            {
                EngineNumber = engineNo;
            }

            if (ReadFromHexString(data, 2, 4, false, out int rpm))
            {
                RotationalSpeed = RotationalSpeed.FromRevolutionsPerSecond(rpm);
            }

            if (ReadFromHexString(data, 10, 2, false, out int pitch))
            {
                PropellerPitch = Ratio.FromPercent(pitch);
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
        /// Engine revolutions per time, typically RPM (revolutions per minute) is used
        /// as unit for engine speed.
        /// </summary>
        public RotationalSpeed RotationalSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of the engine, 0-based.
        /// </summary>
        public int EngineNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// Pitch of the propeller. Propellers with changeable pitch are very rare for pleasure boats, with the exception of folding propellers
        /// for sailboats, but these fold only when the engine is not in use and there's no sensor to detect the state.
        /// </summary>
        public Ratio PropellerPitch
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns false for this message, as PCDIN messages can mean different things
        /// </summary>
        public override bool ReplacesOlderInstance => false;

        /// <inheritdoc />
        public override string ToNmeaParameterList()
        {
            if (Valid)
            {
                // Example data set: (bad example from the docs, since the engine is just not running here)
                // $PCDIN,01F200,000C7A4F,02,000000FFFF7FFFFF*21
                // Unfortunately, the resolution of the RPM value is quite low, as 1 RPS ~ 64RPM
                int rpm = (int)Math.Round(RotationalSpeed.RevolutionsPerSecond);
                string engineNoText = EngineNumber.ToString("X2", CultureInfo.InvariantCulture);
                string rpmText = rpm.ToString("X4", CultureInfo.InvariantCulture);
                int pitchPercent = (int)PropellerPitch.Percent;
                string pitchText = pitchPercent.ToString("X2", CultureInfo.InvariantCulture);
                string timeStampText = MessageTimeStamp.ToString("X8", CultureInfo.InvariantCulture);

                return "01F200," + timeStampText + ",02," + engineNoText + rpmText + "FFFF" + pitchText + "FFFF";
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Engine {EngineNumber} RPM: {RotationalSpeed.RevolutionsPerMinute}";
            }

            return "No valid data (or engine off)";
        }
    }
}

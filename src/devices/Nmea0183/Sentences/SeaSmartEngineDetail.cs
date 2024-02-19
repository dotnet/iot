// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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
    /// An extended engine data message, using a PCDIN sequence (supported by some NMEA0183 to NMEA2000 bridges)
    /// PCDIN message 01F201 Engine status data (temperatures, oil pressure, operating time)
    /// </summary>
    public class SeaSmartEngineDetail : ProprietaryMessage
    {
        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0x01F201;

        /// <summary>
        /// Constructs a new sentence
        /// </summary>
        public SeaSmartEngineDetail(EngineStatus status, TimeSpan operatingTime, Temperature temperature, int engineNumber)
            : base()
        {
            Status = status;
            OperatingTime = operatingTime;
            Temperature = temperature;
            EngineNumber = engineNumber;
            MessageTimeStamp = Environment.TickCount;
            Valid = true;
        }

        /// <summary>
        /// Constructs a new sentence
        /// </summary>
        public SeaSmartEngineDetail(EngineData data)
        {
            Status = data.Status;
            OperatingTime = data.OperatingTime;
            Temperature = data.EngineTemperature;
            EngineNumber = data.EngineNo;
            MessageTimeStamp = data.MessageTimeStamp;
            Valid = true;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public SeaSmartEngineDetail(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public SeaSmartEngineDetail(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
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

            if (ReadFromHexString(data, 10, 4, true, out int temp))
            {
                if (temp > 0 && temp < 0xFFFF)
                {
                    Temperature = UnitsNet.Temperature.FromKelvins(temp / 100.0).ToUnit(TemperatureUnit.DegreeCelsius);
                }
                else
                {
                    Temperature = null;
                }
            }

            if (ReadFromHexString(data, 22, 8, true, out int operatingTime))
            {
                OperatingTime = TimeSpan.FromSeconds(operatingTime);
            }

            if (ReadFromHexString(data, 40, 4, false, out int status))
            {
                Status = (EngineStatus)status;
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
        /// Engine status: True for running, false for not running/error.
        /// </summary>
        public EngineStatus Status
        {
            get;
            private set;
        }

        /// <summary>
        /// Total engine operating time (typically displayed in hours)
        /// </summary>
        public TimeSpan OperatingTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Engine temperature
        /// </summary>
        public Temperature? Temperature
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of the engine
        /// </summary>
        public int EngineNumber
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
                string engineNoText = EngineNumber.ToString("X2", CultureInfo.InvariantCulture);
                // $PCDIN,01F201,000C7E1B,02,000000FFFF407F0005000000000000FFFF000000000000007F7F*24
                //                           1-2---3---4---5---6---7-------8---9---1011--12--1314
                // 1) Engine no. 0: Cntr/Single
                // 2) Oil pressure
                // 3) Oil temp
                // 4) Engine Temp
                // 5) Alternator voltage
                // 6) Fuel rate
                // 7) Engine operating time (seconds)
                // 8) Coolant pressure
                // 9) Fuel pressure
                // 10) Reserved
                // 11) Status
                // 12) Status
                // 13) Load percent
                // 14) Torque percent
                int operatingTimeSeconds = (int)OperatingTime.TotalSeconds;
                string operatingTimeString = operatingTimeSeconds.ToString("X8", CultureInfo.InvariantCulture);
                // For whatever reason, this expects this as little endian (all the other way round)
                string swappedString = operatingTimeString.Substring(6, 2) + operatingTimeString.Substring(4, 2) +
                                       operatingTimeString.Substring(2, 2) + operatingTimeString.Substring(0, 2);

                // Lower 16 bits of status
                uint status1 = ((uint)Status & 0xFFFF);
                string status1String = status1.ToString("X4", CultureInfo.InvariantCulture);

                uint status2 = ((uint)Status & 0xFFFF0000) >> 16;
                string status2String = status2.ToString("X4", CultureInfo.InvariantCulture);

                int engineTempKelvin;
                if (Temperature.HasValue)
                {
                    engineTempKelvin = (int)Math.Round(Temperature.Value.Kelvins * 100.0, 1);
                }
                else
                {
                    engineTempKelvin = 0xFFFF;
                }

                string engineTempString = engineTempKelvin.ToString("X4", CultureInfo.InvariantCulture);
                // Seems to require a little endian conversion as well
                engineTempString = engineTempString.Substring(2, 2) + engineTempString.Substring(0, 2);
                return "01F201," + timeStampText + ",02," + engineNoText + "0000FFFF" + engineTempString + "00050000" + swappedString + "FFFF000000" + status1String + status2String + "7F7F";

            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Engine {EngineNumber} Status: {Status} Temperature {Temperature.GetValueOrDefault(UnitsNet.Temperature.Zero).DegreesCelsius} °C";
            }

            return "No valid data (or engine off)";
        }
    }
}

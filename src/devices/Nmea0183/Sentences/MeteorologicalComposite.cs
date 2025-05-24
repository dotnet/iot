// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Provides meteorological information.
    /// This class does not parse or send the wind information, which is also part of this message. Use <see cref="WindSpeedAndAngle"/> (MWV sentence)
    /// instead.
    /// </summary>
    public class MeteorologicalComposite : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("MDA");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new MDA sentence
        /// </summary>
        /// <param name="pressure">Barometric pressure</param>
        /// <param name="airTemperature">Temperature of air</param>
        /// <param name="waterTemperature">Water temperature</param>
        /// <param name="relativeHumidity">Relative humidity, percent</param>
        /// <param name="dewPoint">Dew point</param>
        public MeteorologicalComposite(Pressure? pressure, Temperature? airTemperature, Temperature? waterTemperature,
            RelativeHumidity? relativeHumidity, Temperature? dewPoint)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            BarometricPressure = pressure;
            AirTemperature = airTemperature;
            WaterTemperature = waterTemperature;
            RelativeHumidity = relativeHumidity;
            DewPoint = dewPoint;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public MeteorologicalComposite(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public MeteorologicalComposite(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? baroInchesMercury = ReadValue(field);
            string referenceI = ReadString(field) ?? string.Empty;
            double? baroBars = ReadValue(field);
            string referenceB = ReadString(field) ?? string.Empty;
            double? airTemp = ReadValue(field);
            string referenceAir = ReadString(field) ?? string.Empty;
            double? waterTemp = ReadValue(field);
            string referenceWater = ReadString(field) ?? string.Empty;
            double? relHumidity = ReadValue(field);
            ReadValue(field); // Absolute humidity (ignored, since meaning unclear)
            double? dewPoint = ReadValue(field);
            string referenceDewPoint = ReadString(field) ?? string.Empty;

            if (baroBars.HasValue && referenceB == "B")
            {
                BarometricPressure = Pressure.FromHectopascals(baroBars.Value * 1000);
            }
            else if (baroInchesMercury.HasValue && referenceI == "I")
            {
                BarometricPressure = Pressure.FromInchesOfMercury(baroInchesMercury.Value);
            }

            if (airTemp.HasValue && referenceAir == "C")
            {
                AirTemperature = Temperature.FromDegreesCelsius(airTemp.Value);
            }

            if (waterTemp.HasValue && referenceWater == "C")
            {
                WaterTemperature = Temperature.FromDegreesCelsius(waterTemp.Value);
            }

            if (relHumidity.HasValue)
            {
                RelativeHumidity = UnitsNet.RelativeHumidity.FromPercent(relHumidity.Value);
            }

            if (dewPoint.HasValue && referenceDewPoint == "C")
            {
                DewPoint = Temperature.FromDegreesCelsius(dewPoint.Value);
            }

            // Note: The remaining fields are about wind speed/direction. These are not parsed here, use MWV sentence instead
            Valid = true;
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Barometric pressure (corrected for station altitude)
        /// </summary>
        public Pressure? BarometricPressure { get; }

        /// <summary>
        /// Air temperature
        /// </summary>
        public Temperature? AirTemperature { get; }

        /// <summary>
        /// Water temperature
        /// </summary>
        public Temperature? WaterTemperature { get; }

        /// <summary>
        /// Relative humidity
        /// </summary>
        public RelativeHumidity? RelativeHumidity { get; }

        /// <summary>
        /// Dew point
        /// </summary>
        public Temperature? DewPoint { get; }

        /// <summary>
        /// Presents this message as output
        /// </summary>
        public override string ToNmeaParameterList()
        {
            if (Valid)
            {
                string inchesOfMercury = BarometricPressure.HasValue
                    ? BarometricPressure.Value.InchesOfMercury.ToString("F2", CultureInfo.InvariantCulture)
                    : string.Empty;
                string bars = BarometricPressure.HasValue
                    ? (BarometricPressure.Value.Hectopascals / 1000).ToString("F3", CultureInfo.InvariantCulture)
                    : string.Empty;
                string airTemp = AirTemperature.HasValue
                    ? AirTemperature.Value.DegreesCelsius.ToString("F1", CultureInfo.InvariantCulture)
                    : string.Empty;
                string waterTemp = WaterTemperature.HasValue
                    ? WaterTemperature.Value.DegreesCelsius.ToString("F1", CultureInfo.InvariantCulture)
                    : string.Empty;
                string humidity = RelativeHumidity.HasValue
                    ? RelativeHumidity.Value.Percent.ToString("F1", CultureInfo.InvariantCulture)
                    : string.Empty;
                string dewPoint = DewPoint.HasValue
                    ? DewPoint.Value.DegreesCelsius.ToString("F1", CultureInfo.InvariantCulture)
                    : string.Empty;
                return FormattableString.Invariant($"{inchesOfMercury},I,{bars},B,{airTemp},C,{waterTemp},C,{humidity},,{dewPoint},C,,T,,M,,N,,M");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Pressure: {BarometricPressure.GetValueOrDefault(Pressure.Zero).ToUnit(PressureUnit.Hectopascal)}, Air Temp: {AirTemperature.GetValueOrDefault(Temperature.Zero)}, " +
                       $"Water temp: {WaterTemperature.GetValueOrDefault(Temperature.Zero)}, Dew Point: {DewPoint.GetValueOrDefault(Temperature.Zero)}";
            }

            return "No valid data";
        }
    }
}

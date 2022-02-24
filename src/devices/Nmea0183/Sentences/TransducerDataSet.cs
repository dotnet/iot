// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// A single data set from an XDR sentence
    /// </summary>
    public sealed class TransducerDataSet
    {
        /// <summary>
        /// Creates an empty instance
        /// </summary>
        public TransducerDataSet()
        {
            Unit = string.Empty;
            Value = 0;
            DataType = string.Empty;
            DataName = string.Empty;
        }

        /// <summary>
        /// Creates an instance filled with data.
        /// </summary>
        public TransducerDataSet(string dataType, double value, string unit, string dataName)
        {
            DataType = dataType;
            Value = value;
            Unit = unit;
            DataName = dataName;
        }

        /// <summary>
        /// The unit of the measurement
        /// Known values:
        /// - B (bar)
        /// - P (Pascal, Percentage)
        /// - D (degrees)
        /// - G (Acceleration, g-Force)
        /// - C (degrees Celsius)
        /// - M (Mass, Liters)
        /// - V (Voltage)
        /// - A (Ampere)
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// The value of the measurement
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// The data type of the measurement
        /// Known values:
        /// - A (Angle)
        /// - P (atmospheric Pressure)
        /// - C (Temperature)
        /// - H (Humidity)
        /// - V (Volume)
        /// - U (Voltage)
        /// - I (Current)
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Name of the data set
        /// Known values (may differ by equipment vendor):
        /// - ENV_WATER_T
        /// - ENV_OUTAIR_T, TempAir
        /// - ENV_ATMOS_P, barometer
        /// - ENV_INSIDE_H
        /// - PTCH, PITCH
        /// - ROLL
        /// - YAW
        /// - ENGT - Engine temperature
        /// - FUEL
        /// - FRESHWATER
        /// - WASTEWATER
        /// - BLACKWATER
        /// - LIVEWELL
        /// - OIL
        /// - BATVOLT, BATCURR
        ///
        /// The above values may be followed by a #y, where y is the instance number (i.e. the tank number)
        /// </summary>
        public string DataName { get; set; }

        /// <summary>
        /// Content formatted as NMEA message part
        /// </summary>
        public override string ToString()
        {
            string formattedValue;
            if (Unit == "B" || Unit == "P")
            {
                // Pressure needs 6 digits
                formattedValue = Value.ToString("G6", CultureInfo.InvariantCulture);
            }
            else if (Unit == "H")
            {
                formattedValue = Value.ToString("F1", CultureInfo.InvariantCulture);
            }
            else
            {
                formattedValue = Value.ToString("F2", CultureInfo.InvariantCulture); // 2 significant digits
            }

            if (formattedValue.Contains("E"))
            {
                // But if that would result in scientific notation (probably fishy, but not here to decide), use fixed point instead
                formattedValue = Value.ToString("F1", CultureInfo.InvariantCulture);
            }

            return FormattableString.Invariant($"{DataType},{formattedValue},{Unit},{DataName}");
        }

        /// <summary>
        /// Content as readable text
        /// </summary>
        public string ToReadableContent()
        {
            return FormattableString.Invariant($"{DataName}: {Value:F2} {Unit}");
        }

        /// <summary>
        /// Returns the value of the current instance as angle.
        /// </summary>
        /// <returns>An angle, null if this instance is not an angle</returns>
        public Angle? AsAngle()
        {
            if (Unit == "D")
            {
                return Angle.FromDegrees(Value);
            }

            return null;
        }

        /// <summary>
        /// Returns the value of the current instance as temperature.
        /// </summary>
        /// <returns>A temperature, null if this instance is not a temperature</returns>
        public Temperature? AsTemperature()
        {
            if (Unit == "C")
            {
                return Temperature.FromDegreesCelsius(Value);
            }

            return null;
        }
    }
}

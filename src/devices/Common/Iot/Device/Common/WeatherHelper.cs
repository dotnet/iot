// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Units;
using System;

namespace Iot.Device.Common
{
    /// <summary>
    /// Helpers for weather. Formulas taken from http://www.reahvac.com/tools/humidity-formulas/.
    /// </summary>
    public class WeatherHelper
    {
        /// <summary>
        /// The heat index (or apparent temperature) is used to measure the amount of discomfort
        /// during the summer months when heat and humidity often combine to make it feel hotter
        /// than it actually is. The heat index is usually used for afternoon high temperatures.
        /// Use the summer simmer index instead for overnight low temperatures.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH) expressed as a percentage</param>
        /// <returns>The heat index, also known as the apparent temperature</returns>
        public static Temperature HeatIndex(Temperature airTemperature, double relativeHumidity)
        {
            double tf = airTemperature.Fahrenheit;
            double rh = relativeHumidity;
            double tf2 = Math.Pow(tf, 2);
            double rh2 = Math.Pow(rh, 2);
            return Temperature.FromFahrenheit((-42.379)
                + (2.04901523 * tf)
                + (10.14333127 * rh)
                - (0.22475541 * tf * rh)
                - (6.83783 * Math.Pow(10, -3) * tf2)
                - (5.481717 * Math.Pow(10, -2) * rh2)
                + (1.22874 * Math.Pow(10, -3) * tf2 * (rh))
                + (8.5282 * Math.Pow(10, -4) * tf * rh2)
                - (1.99 * Math.Pow(10, -6) * tf2 * rh2));
        }

        /// <summary>
        /// The summer simmer index is used to measure the amount of discomfort during the summer months
        /// when heat and humidity often combine to make it feel hotter than it actually is. The summer
        /// simmer index is usually used for overnight low temperatures. Use the heat index instead for
        /// afternoon high temperatures.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH) expressed as a percentage</param>
        /// <returns>The summer simmer index</returns>
        public static Temperature SummerSimmerIndex(Temperature airTemperature, double relativeHumidity)
        {
            double tf = airTemperature.Fahrenheit;
            double rh = relativeHumidity;
            return Temperature.FromFahrenheit(1.98 * (tf - ((0.55 - (0.0055 * rh)) * (tf - 58.0))) - 56.83);
        }

        /// <summary>
        /// Calculates the saturated vapor pressure.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <returns>The saturated vapor pressure</returns>
        public static Pressure SaturatedVaporPressure(Temperature airTemperature)
        {
            double tc = airTemperature.Celsius;
            return Pressure.FromHectopascal(6.11 * Math.Pow(10, ((7.5 * tc) / (237.7 + tc))));
        }

        /// <summary>
        /// Calculates the actual vapor pressure.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH) expressed as a percentage</param>
        /// <returns>The actual vapor pressure</returns>
        public static Pressure ActualVaporPressure(Temperature airTemperature, double relativeHumidity)
        {
            return Pressure.FromHectopascal((relativeHumidity * SaturatedVaporPressure(airTemperature).Hectopascal) / 100);
        }

        /// <summary>
        /// Calculates the dew point.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH) expressed as a percentage</param>
        /// <returns>The dew point</returns>
        public static Temperature DewPoint(Temperature airTemperature, double relativeHumidity)
        {
            var avp = ActualVaporPressure(airTemperature, relativeHumidity).Hectopascal;
            var lavp = Math.Log(avp);
            return Temperature.FromCelsius(((-430.22 + (237.7 * lavp)) / (19.08 - lavp)));
        }

        /// <summary>
        /// Calculates the absolute humidity in g/m³
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH) expressed as a percentage</param>
        /// <returns>The absolute humidity in g/m³</returns>
        public static double AbsoluteHumidity(Temperature airTemperature, double relativeHumidity)
        {
            var avp = ActualVaporPressure(airTemperature, relativeHumidity).Pascal;
            return avp / (airTemperature.Kelvin * 461.5) * 1000;
        }
    }
}

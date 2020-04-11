// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Units;

namespace Iot.Device.Common
{
    /// <summary>
    /// Helpers for weather
    /// </summary>
    public static class WeatherHelper
    {
        #region TemperatureAndRelativeHumidity
        // Formulas taken from https://www.wpc.ncep.noaa.gov/html/heatindex_equation.shtml
        // US government website, therefore public domain.

        /// <summary>
        /// The heat index (or apparent temperature) is used to measure the amount of discomfort
        /// during the summer months when heat and humidity often combine to make it feel hotter
        /// than it actually is. The heat index is usually used for afternoon high temperatures.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH) expressed as a percentage</param>
        /// <returns>The heat index, also known as the apparent temperature</returns>
        public static Temperature CalculateHeatIndex(Temperature airTemperature, double relativeHumidity)
        {
            double tf = airTemperature.Fahrenheit;
            double rh = relativeHumidity;
            double tf2 = Math.Pow(tf, 2);
            double rh2 = Math.Pow(rh, 2);

            double steadman = 0.5 * (tf + 61 + ((tf - 68) * 1.2) + (rh * 0.094));

            if (steadman + tf < 160) // if the average is lower than 80F, use Steadman, otherwise use Rothfusz.
            {
                return Temperature.FromFahrenheit(steadman);
            }

            double rothfuszRegression = (-42.379)
                + (2.04901523 * tf)
                + (10.14333127 * rh)
                - (0.22475541 * tf * rh)
                - (6.83783 * 0.001 * tf2)
                - (5.481717 * 0.01 * rh2)
                + (1.22874 * 0.001 * tf2 * rh)
                + (8.5282 * 0.0001 * tf * rh2)
                - (1.99 * 0.000001 * tf2 * rh2);

            if (rh < 13 && tf >= 80 && tf <= 112)
            {
                rothfuszRegression += ((13 - rh) / 4) * Math.Sqrt((17 - Math.Abs(tf - 95)) / 17);
            }
            else if (rh > 85 && tf >= 80 && tf <= 87)
            {
                rothfuszRegression += ((rh - 85) / 10) * ((87 - tf) / 5);
            }

            return Temperature.FromFahrenheit(rothfuszRegression);
        }

        /// <summary>
        /// Calculates the saturated vapor pressure.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <returns>The saturated vapor pressure</returns>
        public static Pressure CalculateSaturatedVaporPressure(Temperature airTemperature)
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
        public static Pressure CalculateActualVaporPressure(Temperature airTemperature, double relativeHumidity)
        {
            return Pressure.FromHectopascal((relativeHumidity * CalculateSaturatedVaporPressure(airTemperature).Hectopascal) / 100);
        }

        /// <summary>
        /// Calculates the dew point.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH) expressed as a percentage</param>
        /// <returns>The dew point</returns>
        public static Temperature CalculateDewPoint(Temperature airTemperature, double relativeHumidity)
        {
            var avp = CalculateActualVaporPressure(airTemperature, relativeHumidity).Hectopascal;
            var lavp = Math.Log(avp);
            return Temperature.FromCelsius((-430.22 + (237.7 * lavp)) / (19.08 - lavp));
        }

        /// <summary>
        /// Calculates the absolute humidity in g/m³
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH) expressed as a percentage</param>
        /// <returns>The absolute humidity in g/m³</returns>
        public static double CalculateAbsoluteHumidity(Temperature airTemperature, double relativeHumidity)
        {
            var avp = CalculateActualVaporPressure(airTemperature, relativeHumidity).Pascal;
            return avp / (airTemperature.Kelvin * 461.5) * 1000;
        }
        #endregion TemperatureAndRelativeHumidity

        #region Pressure
        // Formula  from https://de.wikipedia.org/wiki/Barometrische_H%C3%B6henformel#Internationale_H%C3%B6henformel, solved
        // for different parameters

        /// <summary>
        /// Calculates the altitude in meters from the given pressure, sea-level pressure and air temperature
        /// </summary>
        /// <param name="pressure">The pressure at the point for which altitude is being calculated</param>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <param name="airTemperature">The dry air temperature at the point for which altitude is being calculated</param>
        /// <returns>The altitude in meters</returns>
        public static double CalculateAltitude(Pressure pressure, Pressure seaLevelPressure, Temperature airTemperature)
        {
            return ((Math.Pow(seaLevelPressure.Pascal / pressure.Pascal, 1 / 5.255) - 1) * airTemperature.Kelvin) / 0.0065;
        }

        /// <summary>
        /// Calculates the altitude in meters from the given pressure and air temperature. Assumes mean sea-level pressure.
        /// </summary>
        /// <param name="pressure">The pressure at the point for which altitude is being calculated</param>
        /// <param name="airTemperature">The dry air temperature at the point for which altitude is being calculated</param>
        /// <returns>The altitude in meters</returns>
        public static double CalculateAltitude(Pressure pressure, Temperature airTemperature)
        {
            return CalculateAltitude(pressure, Pressure.MeanSeaLevel, airTemperature);
        }

        /// <summary>
        /// Calculates the altitude in meters from the given pressure and sea-level pressure. Assumes temperature of 15C.
        /// </summary>
        /// <param name="pressure">The pressure at the point for which altitude is being calculated</param>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <returns>The altitude in meters</returns>
        public static double CalculateAltitude(Pressure pressure, Pressure seaLevelPressure)
        {
            return CalculateAltitude(pressure, seaLevelPressure, Temperature.FromCelsius(15));
        }

        /// <summary>
        /// Calculates the altitude in meters from the given pressure. Assumes mean sea-level pressure and temperature of 15C.
        /// </summary>
        /// <param name="pressure">The pressure at the point for which altitude is being calculated</param>
        /// <returns>The altitude in meters</returns>
        public static double CalculateAltitude(Pressure pressure)
        {
            return CalculateAltitude(pressure, Pressure.MeanSeaLevel, Temperature.FromCelsius(15));
        }

        /// <summary>
        /// Calculates the approximate sea-level pressure from given absolute pressure, altitude and air temperature.
        /// </summary>
        /// <param name="pressure">The air pressure at the point of measurement</param>
        /// <param name="altitude">The altitude in meters at the point of the measurement</param>
        /// <param name="airTemperature">The air temperature</param>
        /// <returns>The estimated absolute sea-level pressure</returns>
        /// <remarks><see cref="CalculatePressure"/> solved for sea level pressure</remarks>
        public static Pressure CalculateSeaLevelPressure(Pressure pressure, double altitude, Temperature airTemperature)
        {
            return Pressure.FromPascal(Math.Pow((((0.0065 * altitude) / airTemperature.Kelvin) + 1), 5.255) * pressure.Pascal);
        }

        /// <summary>
        /// Calculates the approximate absolute pressure from given sea-level pressure, altitude and air temperature.
        /// </summary>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <param name="altitude">The altitude in meters at the point for which pressure is being calculated</param>
        /// <param name="airTemperature">The air temperature at the point for which pressure is being calculated</param>
        /// <returns>The estimated absolute pressure at the given altitude</returns>
        public static Pressure CalculatePressure(Pressure seaLevelPressure, double altitude, Temperature airTemperature)
        {
            return Pressure.FromPascal(seaLevelPressure.Pascal / Math.Pow((((0.0065 * altitude) / airTemperature.Kelvin) + 1), 5.255));
        }

        /// <summary>
        /// Calculates the temperature gradient for the given pressure difference
        /// </summary>
        /// <param name="pressure">The air pressure at the point for which temperature is being calculated</param>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <param name="altitude">The altitude in meters at the point for which temperature is being calculated</param>
        /// <returns>The standard temperature at the given altitude, when the given pressure difference is known</returns>
        /// <remarks><see cref="CalculatePressure"/> solved for temperature</remarks>
        public static Temperature CalculateTemperature(Pressure pressure, Pressure seaLevelPressure, double altitude)
        {
            return Temperature.FromKelvin((0.0065 * altitude) / (Math.Pow(seaLevelPressure.Pascal / pressure.Pascal, 1 / 5.255) - 1));
        }

        /// <summary>
        /// Calculates the barometric pressure from a raw reading, using the reduction formula from the german met service.
        /// This is a more complex variant of <see cref="CalculateSeaLevelPressure"/>. It gives the value that a weather station gives
        /// for a particular area and is also used in meteorological charts.
        /// <example>
        /// You are at 650m over sea and measure a pressure of 948.7 hPa and a temperature of 24.0°C. The met service will show that
        /// you are within a high-pressure area of around 1020 hPa.
        /// </example>
        /// </summary>
        /// <param name="measuredPressure">Measured pressure at the observation point</param>
        /// <param name="measuredTemperature">Measured temperature at the observation point</param>
        /// <param name="measurementAltitude">Height over sea level of the observation point (to be really precise, geopotential heights have
        /// to be used above ~750m)</param>
        /// <returns>The barometric pressure at the point of observation</returns>
        public static Pressure CalculateBarometricPressure(Pressure measuredPressure, Temperature measuredTemperature,
            double measurementAltitude)
        {
            double vaporPressure;
            if (measuredTemperature.Celsius >= 9.1)
            {
                vaporPressure = 18.2194 * (1.0463 - Math.Exp((-0.0666) * measuredTemperature.Celsius));
            }
            else
            {
                vaporPressure = 5.6402 * (-0.0916 + Math.Exp((-0.06) * measuredTemperature.Celsius));
            }

            double x = (9.80665 / (287.05 * ((measuredTemperature.Kelvin) + 0.12 * vaporPressure +
                                             (0.0065 * measurementAltitude) / 2))) * measurementAltitude;
            double barometricPressure = measuredPressure.Hectopascal * Math.Exp(x);
            return Pressure.FromHectopascal(barometricPressure);
        }

        #endregion
    }
}

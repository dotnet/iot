// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Units;
using System;

namespace Iot.Device.Common
{
    /// <summary>
    /// Helpers for weather
    /// </summary>
    public class WeatherHelper
    {
        #region TemperatureAndRelativeHumidity
        // Formulas taken from http://www.reahvac.com/tools/humidity-formulas/
        // Extra notes for heat index taken from https://www.wpc.ncep.noaa.gov/html/heatindex_equation.shtml
        
        /// <summary>
        /// The heat index (or apparent temperature) is used to measure the amount of discomfort
        /// during the summer months when heat and humidity often combine to make it feel hotter
        /// than it actually is. The heat index is usually used for afternoon high temperatures.
        /// Use the summer simmer index instead for overnight low temperatures.
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

            if (steadman < 80)
                return Temperature.FromFahrenheit(steadman);

            double rothfuszRegression = (-42.379)
                + (2.04901523 * tf)
                + (10.14333127 * rh)
                - (0.22475541 * tf * rh)
                - (6.83783 * 0.001 * tf2)
                - (5.481717 * 0.01 * rh2)
                + (1.22874 * 0.001 * tf2 * (rh))
                + (8.5282 * 0.0001 * tf * rh2)
                - (1.99 * 0.000001 * tf2 * rh2);

            if (rh < 13 && tf >= 80 && tf <= 112)
            { // adjustment
                rothfuszRegression += ((13 - rh) / 4) * Math.Sqrt((17 - Math.Abs(tf - 95)) / 17);
            }
            else if (rh > 85 && tf >= 80 && tf <= 87)
            { // adjustment
                rothfuszRegression += ((rh - 85) / 10) * ((87 - tf) / 5);
            }

            return Temperature.FromFahrenheit(rothfuszRegression);
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
        public static Temperature CalculateSummerSimmerIndex(Temperature airTemperature, double relativeHumidity)
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
            return Temperature.FromCelsius(((-430.22 + (237.7 * lavp)) / (19.08 - lavp)));
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
        // Formula taken from https://keisan.casio.com/has10/SpecExec.cgi?path=06000000.Science%252F02100100.Earth%2520science%252F12000300.Altitude%2520from%2520atmospheric%2520pressure%252Fdefault.xml&charset=utf-8
            
        /// <summary>
        /// Calculates the altitude in meters from the given pressure, sea-level pressure and air temperature
        /// </summary>
        /// <param name="pressure">The pressure at the point for which altitude is being calculated</param>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <param name="airTemperature">The dry air temperature at the point for which altitude is being calculated</param>
        /// <returns>The altitude in meters</returns>
        public static double CalculateAltitude(Pressure pressure, Pressure seaLevelPressure, Temperature airTemperature)
        {
            return ((Math.Pow(seaLevelPressure.Pascal / pressure.Pascal, 1 / 5.257) - 1) * airTemperature.Kelvin) / 0.0065;
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
        /// Calculates the sea-level pressure from given pressure, altitude and air temperature.
        /// </summary>
        /// <param name="pressure">The air pressure</param>
        /// <param name="altitude">The altitude in meters</param>
        /// <param name="airTemperature">The air temperature</param>
        /// <returns>The sea-level pressure</returns>
        public static Pressure CalculateSeaLevelPressure(Pressure pressure, double altitude, Temperature airTemperature)
        {
            return Pressure.FromPascal(Math.Pow((((0.0065 * altitude) / airTemperature.Kelvin) + 1), 5.257) * pressure.Pascal);
        }
        
        /// <summary>
        /// Calculates the pressure from given sea-level pressure, altitude and air temperature.
        /// </summary>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <param name="altitude">The altitude in meters at the point for which pressure is being calculated</param>
        /// <param name="airTemperature">The air temperature at the point for which pressure is being calculated</param>
        /// <returns>The air pressure</returns>
        public static Pressure CalculatePressure(Pressure seaLevelPressure, double altitude, Temperature airTemperature)
        {
            return Pressure.FromPascal(seaLevelPressure.Pascal / Math.Pow((((0.0065 * altitude) / airTemperature.Kelvin) + 1), 5.257));
        }
        
        /// <summary>
        /// Calculates the temperature from given pressure, sea-level pressure and altitude.
        /// </summary>
        /// <param name="pressure">The air pressure at the point for which temperature is being calculated</param>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <param name="altitude">The altitude in meters at the point for which temperature is being calculated</param>
        /// <returns>The temperature</returns>
        public static Temperature CalculateTemperature(Pressure pressure, Pressure seaLevelPressure, double altitude)
        {
            return Temperature.FromKelvin((0.0065 * altitude) / (Math.Pow(seaLevelPressure.Pascal / pressure.Pascal, 1 / 5.257) - 1));
        }
        #endregion
    }
}

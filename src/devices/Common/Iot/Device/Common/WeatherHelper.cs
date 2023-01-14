// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;

namespace Iot.Device.Common
{
    /// <summary>
    /// This class contains methods to calculate meteorological values from different
    /// sensor measurements. Multiple sensor inputs are used to generate additional information.
    /// </summary>
    public static class WeatherHelper
    {
        /// <summary>
        /// Gas constant of dry Air, J / (kg * K)
        /// </summary>
        internal const double SpecificGasConstantOfAir = 287.058;

        /// <summary>
        /// Gas constant of vapor, J / (kg * K)
        /// </summary>
        internal const double SpecificGasConstantOfVapor = 461.523;

        /// <summary>
        /// Default atmospheric temperature gradient = 0.0065K/m (or 0.65K per 100m)
        /// </summary>
        internal const double DefaultTemperatureGradient = 0.0065;

        /// <summary>
        /// The mean sea-level pressure (MSLP) is the average atmospheric pressure at mean sea level
        /// </summary>
        public static readonly Pressure MeanSeaLevel = Pressure.FromPascals(101325);

        #region TemperatureAndRelativeHumidity

        /// <summary>
        /// The heat index (or apparent temperature) is used to measure the amount of discomfort
        /// during the summer months when heat and humidity often combine to make it feel hotter
        /// than it actually is. The heat index is usually used for afternoon high temperatures.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH)</param>
        /// <returns>The heat index, also known as the apparent temperature</returns>
        /// <remarks>
        /// Formula from https://www.wpc.ncep.noaa.gov/html/heatindex_equation.shtml
        /// </remarks>
        public static Temperature CalculateHeatIndex(Temperature airTemperature, RelativeHumidity relativeHumidity)
        {
            double tf = airTemperature.DegreesFahrenheit;
            double rh = relativeHumidity.Percent;
            double tf2 = Math.Pow(tf, 2);
            double rh2 = Math.Pow(rh, 2);

            double steadman = 0.5 * (tf + 61 + ((tf - 68) * 1.2) + (rh * 0.094));

            if (steadman + tf < 160) // if the average is lower than 80F, use Steadman, otherwise use Rothfusz.
            {
                return Temperature.FromDegreesFahrenheit(steadman);
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

            return Temperature.FromDegreesFahrenheit(rothfuszRegression);
        }

        /// <summary>
        /// Calculates the saturated vapor pressure for a given air temperature over water.
        /// The formula used is valid for temperatures between -100°C and +100°C.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <returns>The saturated vapor pressure</returns>
        /// <remarks>
        /// From https://de.wikibooks.org/wiki/Tabellensammlung_Chemie/_Stoffdaten_Wasser, after D. Sonntag (1982)
        /// </remarks>
        public static Pressure CalculateSaturatedVaporPressureOverWater(Temperature airTemperature)
        {
            double tk = airTemperature.Kelvins;
            double e_w = Math.Exp((-6094.4642 / tk) + 21.1249952 - (2.7245552E-2 * tk) + (1.6853396E-5 * tk * tk) +
                                   (2.4575506 * Math.Log(tk)));
            return Pressure.FromPascals(e_w);
        }

        /// <summary>
        /// Calculates the saturated vapor pressure for a given air temperature over ice.
        /// The formula used is valid for temperatures between -100°C and +0°C.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <returns>The saturated vapor pressure</returns>
        /// <remarks>
        /// From https://de.wikibooks.org/wiki/Tabellensammlung_Chemie/_Stoffdaten_Wasser, after D. Sonntag (1982)
        /// </remarks>
        public static Pressure CalculateSaturatedVaporPressureOverIce(Temperature airTemperature)
        {
            double tk = airTemperature.Kelvins;
            double e_i = Math.Exp((-5504.4088 / tk) - 3.574628 - (1.7337458E-2 * tk) + (6.5204209E-6 * tk * tk) +
                                  (6.1295027 * Math.Log(tk)));
            return Pressure.FromPascals(e_i * 1.0041); // The table values are corrected by this
        }

        /// <summary>
        /// Calculates the actual vapor pressure.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH)</param>
        /// <returns>The actual vapor pressure</returns>
        public static Pressure CalculateActualVaporPressure(Temperature airTemperature, RelativeHumidity relativeHumidity) =>
            Pressure.FromHectopascals((relativeHumidity.Percent / 100.0 * CalculateSaturatedVaporPressureOverWater(airTemperature).Hectopascals));

        /// <summary>
        /// Calculates the dew point. The dew point is the temperature at which, given the other values remain constant - dew or fog would start
        /// building up.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH)</param>
        /// <returns>The dew point</returns>
        /// <remarks>
        /// Source https://en.wikipedia.org/wiki/Dew_point
        /// </remarks>
        public static Temperature CalculateDewPoint(Temperature airTemperature, RelativeHumidity relativeHumidity)
        {
            if (relativeHumidity <= RelativeHumidity.FromPercent(0.1))
            {
                relativeHumidity = RelativeHumidity.FromPercent(0.1);
            }

            double pa = CalculateActualVaporPressure(airTemperature, relativeHumidity).Hectopascals;
            double a = 6.1121; // hPa
            double c = 257.14; // °C
            double b = 18.678;
            double dewPoint = (c * Math.Log(pa / a)) / (b - Math.Log(pa / a));
            return Temperature.FromDegreesCelsius(dewPoint);
        }

        /// <summary>
        /// Calculates the absolute humidity in g/m³.
        /// </summary>
        /// <param name="airTemperature">The dry air temperature</param>
        /// <param name="relativeHumidity">The relative humidity (RH)</param>
        /// <returns>The absolute humidity in g/m³</returns>
        /// <remarks>
        /// Source https://de.wikipedia.org/wiki/Luftfeuchtigkeit#Absolute_Luftfeuchtigkeit
        /// </remarks>
        public static Density CalculateAbsoluteHumidity(Temperature airTemperature, RelativeHumidity relativeHumidity)
        {
            var avp = CalculateActualVaporPressure(airTemperature, relativeHumidity).Pascals;
            double gramsPerCubicMeter = avp / (airTemperature.Kelvins * 461.5) * 1000;
            return Density.FromGramsPerCubicMeter(gramsPerCubicMeter);
        }

        /// <summary>
        /// Calculates a corrected relative humidity. This is useful if you have a temperature/humidity sensor that is
        /// placed in a location where the temperature is different from the real ambient temperature (like it sits inside a hot case)
        /// and another temperature-only sensor that gives more reasonable ambient temperature readings.
        /// Do note that the relative humidity is dependent on the temperature, because it depends on how much water a volume of air
        /// can contain, which increases with temperature.
        /// </summary>
        /// <param name="airTemperatureFromHumiditySensor">Temperature measured by the humidity sensor</param>
        /// <param name="relativeHumidityMeasured">Humidity measured</param>
        /// <param name="airTemperatureFromBetterPlacedSensor">Temperature measured by better placed sensor</param>
        /// <returns>A corrected humidity. The value will be lower than the input value if the better placed sensor is cooler than
        /// the "bad" sensor.</returns>
        public static RelativeHumidity GetRelativeHumidityFromActualAirTemperature(Temperature airTemperatureFromHumiditySensor,
            RelativeHumidity relativeHumidityMeasured, Temperature airTemperatureFromBetterPlacedSensor)
        {
            Density absoluteHumidity =
                CalculateAbsoluteHumidity(airTemperatureFromHumiditySensor, relativeHumidityMeasured);
            double avp = absoluteHumidity.GramsPerCubicMeter * (airTemperatureFromBetterPlacedSensor.Kelvins * 461.5) / 1000;
            double ret = avp / CalculateSaturatedVaporPressureOverWater(airTemperatureFromBetterPlacedSensor).Hectopascals;
            return RelativeHumidity.FromPercent(ret);

        }

        #endregion TemperatureAndRelativeHumidity

        #region Pressure
        // Formula  from https://de.wikipedia.org/wiki/Barometrische_Höhenformel#Internationale_Höhenformel, solved
        // for different parameters

        /// <summary>
        /// Calculates the altitude in meters from the given pressure, sea-level pressure and air temperature.
        /// </summary>
        /// <param name="pressure">The pressure at the point for which altitude is being calculated</param>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <param name="airTemperature">The dry air temperature at the point for which altitude is being calculated</param>
        /// <returns>The altitude</returns>
        public static Length CalculateAltitude(Pressure pressure, Pressure seaLevelPressure, Temperature airTemperature)
        {
            double meters = ((Math.Pow(seaLevelPressure.Pascals / pressure.Pascals, 1 / 5.255) - 1) * airTemperature.Kelvins) / DefaultTemperatureGradient;
            return Length.FromMeters(meters);
        }

        /// <summary>
        /// Calculates the altitude in meters from the given pressure and air temperature. Assumes mean sea-level pressure.
        /// </summary>
        /// <param name="pressure">The pressure at the point for which altitude is being calculated</param>
        /// <param name="airTemperature">The dry air temperature at the point for which altitude is being calculated</param>
        /// <returns>The altitude</returns>
        public static Length CalculateAltitude(Pressure pressure, Temperature airTemperature) =>
             CalculateAltitude(pressure, MeanSeaLevel, airTemperature);

        /// <summary>
        /// Calculates the altitude in meters from the given pressure and sea-level pressure. Assumes temperature of 15C.
        /// </summary>
        /// <param name="pressure">The pressure at the point for which altitude is being calculated</param>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <returns>The altitude</returns>
        public static Length CalculateAltitude(Pressure pressure, Pressure seaLevelPressure) =>
            CalculateAltitude(pressure, seaLevelPressure, Temperature.FromDegreesCelsius(15));

        /// <summary>
        /// Calculates the altitude in meters from the given pressure. Assumes mean sea-level pressure and temperature of 15C.
        /// </summary>
        /// <param name="pressure">The pressure at the point for which altitude is being calculated</param>
        /// <returns>The altitude</returns>
        public static Length CalculateAltitude(Pressure pressure) =>
            CalculateAltitude(pressure, MeanSeaLevel, Temperature.FromDegreesCelsius(15));

        /// <summary>
        /// Calculates the approximate sea-level pressure from given absolute pressure, altitude and air temperature.
        /// </summary>
        /// <param name="pressure">The air pressure at the point of measurement</param>
        /// <param name="altitude">The altitude at the point of the measurement</param>
        /// <param name="airTemperature">The air temperature</param>
        /// <returns>The estimated absolute sea-level pressure</returns>
        /// <remarks><see cref="CalculatePressure"/> solved for sea level pressure</remarks>
        public static Pressure CalculateSeaLevelPressure(Pressure pressure, Length altitude, Temperature airTemperature) =>
            Pressure.FromPascals(Math.Pow((((DefaultTemperatureGradient * altitude.Meters) / airTemperature.Kelvins) + 1), 5.255) * pressure.Pascals);

        /// <summary>
        /// Calculates the approximate absolute pressure from given sea-level pressure, altitude and air temperature.
        /// </summary>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <param name="altitude">The altitude in meters at the point for which pressure is being calculated</param>
        /// <param name="airTemperature">The air temperature at the point for which pressure is being calculated</param>
        /// <returns>The estimated absolute pressure at the given altitude</returns>
        public static Pressure CalculatePressure(Pressure seaLevelPressure, Length altitude, Temperature airTemperature) =>
            Pressure.FromPascals(seaLevelPressure.Pascals / Math.Pow((((DefaultTemperatureGradient * altitude.Meters) / airTemperature.Kelvins) + 1), 5.255));

        /// <summary>
        /// Calculates the temperature gradient for the given pressure difference
        /// </summary>
        /// <param name="pressure">The air pressure at the point for which temperature is being calculated</param>
        /// <param name="seaLevelPressure">The sea-level pressure</param>
        /// <param name="altitude">The altitude in meters at the point for which temperature is being calculated</param>
        /// <returns>The standard temperature at the given altitude, when the given pressure difference is known</returns>
        /// <remarks><see cref="CalculatePressure"/> solved for temperature</remarks>
        public static Temperature CalculateTemperature(Pressure pressure, Pressure seaLevelPressure, Length altitude) =>
            Temperature.FromKelvins((DefaultTemperatureGradient * altitude.Meters) / (Math.Pow(seaLevelPressure.Pascals / pressure.Pascals, 1 / 5.255) - 1));

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
        /// to be used above ~750m). Do not use the height obtained by calling <see cref="CalculateAltitude(UnitsNet.Pressure)"/>
        /// or any of its overloads, since that would use redundant data.</param>
        /// <returns>The barometric pressure at the point of observation</returns>
        /// <remarks>
        /// From https://de.wikipedia.org/wiki/Barometrische_Höhenformel#Anwendungen
        /// </remarks>
        public static Pressure CalculateBarometricPressure(Pressure measuredPressure, Temperature measuredTemperature,
            Length measurementAltitude)
        {
            double vaporPressure;
            if (measuredTemperature.DegreesCelsius >= 9.1)
            {
                vaporPressure = 18.2194 * (1.0463 - Math.Exp((-0.0666) * measuredTemperature.DegreesCelsius));
            }
            else
            {
                vaporPressure = 5.6402 * (-0.0916 + Math.Exp((-0.06) * measuredTemperature.DegreesCelsius));
            }

            return CalculateBarometricPressure(measuredPressure, measuredTemperature, Pressure.FromHectopascals(vaporPressure),
                measurementAltitude);
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
        /// <param name="vaporPressure">Vapor pressure, meteorologic definition</param>
        /// <param name="measurementAltitude">Height over sea level of the observation point (to be really precise, geopotential heights have
        /// to be used above ~750m)</param>
        /// <returns>The barometric pressure at the point of observation</returns>
        /// <remarks>
        /// From https://de.wikipedia.org/wiki/Barometrische_Höhenformel#Anwendungen
        /// </remarks>
        public static Pressure CalculateBarometricPressure(Pressure measuredPressure, Temperature measuredTemperature, Pressure vaporPressure,
            Length measurementAltitude)
        {
            double x = (9.80665 / (287.05 * ((measuredTemperature.Kelvins) + 0.12 * vaporPressure.Hectopascals +
                                             (DefaultTemperatureGradient * measurementAltitude.Meters) / 2))) * measurementAltitude.Meters;
            double barometricPressure = measuredPressure.Hectopascals * Math.Exp(x);
            return Pressure.FromHectopascals(barometricPressure);
        }

        /// <summary>
        /// Calculates the barometric pressure from a raw reading, using the reduction formula from the german met service.
        /// This is a more complex variant of <see cref="CalculateSeaLevelPressure"/>. It gives the value that a weather station gives
        /// for a particular area and is also used in meteorological charts.
        /// Use this method if you also have the relative humidity.
        /// </summary>
        /// <param name="measuredPressure">Measured pressure at the observation point</param>
        /// <param name="measuredTemperature">Measured temperature at the observation point</param>
        /// <param name="measurementAltitude">Height over sea level of the observation point (to be really precise, geopotential heights have
        /// to be used above ~750m)</param>
        /// <param name="relativeHumidity">Relative humidity at point of measurement</param>
        /// <returns>The barometric pressure at the point of observation</returns>
        /// <remarks>
        /// From https://de.wikipedia.org/wiki/Barometrische_Höhenformel#Anwendungen
        /// </remarks>
        public static Pressure CalculateBarometricPressure(Pressure measuredPressure, Temperature measuredTemperature,
            Length measurementAltitude, RelativeHumidity relativeHumidity)
        {
            Pressure vaporPressure = CalculateActualVaporPressure(measuredTemperature, relativeHumidity);
            return CalculateBarometricPressure(measuredPressure, measuredTemperature, vaporPressure,
                measurementAltitude);
        }

        #endregion

        /// <summary>
        /// Simplified air density (not taking humidity into account)
        /// </summary>
        /// <param name="airPressure">Measured air pressure</param>
        /// <param name="temperature">Measured temperature</param>
        /// <returns>Approximate standard air density</returns>
        /// <remarks>From https://de.wikipedia.org/wiki/Luftdichte </remarks>
        public static Density CalculateAirDensity(Pressure airPressure, Temperature temperature)
        {
            var result = airPressure.Pascals / (SpecificGasConstantOfAir * temperature.Kelvins);
            return Density.FromKilogramsPerCubicMeter(result);
        }

        /// <summary>
        /// Calculates the air density
        /// </summary>
        /// <param name="airPressure">Measured air pressure</param>
        /// <param name="temperature">Measured temperature</param>
        /// <param name="humidity">Measured relative humidity</param>
        /// <returns>Approximate standard air density at sea level</returns>
        /// <remarks>From https://de.wikipedia.org/wiki/Luftdichte </remarks>
        public static Density CalculateAirDensity(Pressure airPressure, Temperature temperature, RelativeHumidity humidity)
        {
            double rs = SpecificGasConstantOfAir;
            double rd = SpecificGasConstantOfVapor;
            var pd = CalculateSaturatedVaporPressureOverWater(temperature);
            // It's still called "constant" even though it's not constant
            double gasConstant = rs / (1 - (humidity.Percent / 100) * (pd.Pascals / airPressure.Pascals) * (1 - (rs / rd)));
            var result = airPressure.Pascals / (gasConstant * temperature.Kelvins);
            return Density.FromKilogramsPerCubicMeter(result);
        }

        /// <summary>
        /// Calculates the wind chill temperature - this is the perceived temperature in (heavy) winds at cold temperatures.
        /// This is only useful at temperatures below about 20°C, above use <see cref="CalculateHeatIndex"/> instead.
        /// Not suitable for wind speeds &lt; 5 km/h.
        /// </summary>
        /// <param name="temperature">The measured air temperature</param>
        /// <param name="windSpeed">The wind speed (measured at 10m above ground)</param>
        /// <returns>The perceived temperature. Note that this is not a real temperature, and the skin will never really reach
        /// this temperature. This is more an indication on how fast the skin will reach the air temperature. If the skin
        /// reaches a temperature of about -5°C, frostbite might occur.</returns>
        /// <remarks>From https://de.wikipedia.org/wiki/Windchill </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The wind speed is less than zero</exception>
        public static Temperature CalculateWindchill(Temperature temperature, Speed windSpeed)
        {
            if (windSpeed < Speed.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(windSpeed), "The wind speed cannot be negative");
            }

            double va = temperature.DegreesCelsius;
            if (windSpeed < Speed.FromKilometersPerHour(1))
            {
                // otherwise, the result is unusable, because the second and third terms of the equation are 0, resulting in a constant offset from the input temperature
                windSpeed = Speed.FromKilometersPerHour(1);
            }

            double wct = 13.12 + 0.6215 * va + (0.3965 * va - 11.37) * Math.Pow(windSpeed.KilometersPerHour, 0.16);
            return Temperature.FromDegreesCelsius(wct);
        }

        /// <summary>
        /// Calculates the wind force on an object.
        /// </summary>
        /// <param name="densityOfAir">The denisty of the air, calculated using one of the overloads of <see cref="CalculateAirDensity(UnitsNet.Pressure,UnitsNet.Temperature)"/></param>
        /// <param name="windSpeed">The speed of the wind</param>
        /// <param name="pressureCoefficient">Pressure coefficient for the shape of the object. Use 1 for a rectangular object directly facing the wind</param>
        /// <returns>The Pressure the wind applies on the object</returns>
        /// <remarks>From https://de.wikipedia.org/wiki/Winddruck </remarks>
        public static Pressure CalculateWindForce(Density densityOfAir, Speed windSpeed, double pressureCoefficient = 1.0)
        {
            double v = windSpeed.MetersPerSecond;
            double rho = densityOfAir.KilogramsPerCubicMeter;

            double wd = pressureCoefficient * rho / 2 * (v * v);
            return Pressure.FromNewtonsPerSquareMeter(wd);
        }
    }
}

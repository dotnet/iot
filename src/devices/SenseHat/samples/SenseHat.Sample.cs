// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Common;
using Iot.Units;
using System;
using System.Drawing;
using System.Threading;

namespace Iot.Device.SenseHat.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // Sample for each device separately
            // LedMatrix.Run();
            // Joystick.Run();
            // AccelerometerAndGyroscope.Run();
            // Magnetometer.Run();
            // TemperatureAndHumidity.Run();
            // PressureAndTemperature.Run();
            
            //set this to the current sea level pressure in the area for correct altitude readings
            var defaultSeaLevelPressure = Pressure.MeanSeaLevel;

            using (var sh = new SenseHat())
            {
                int n = 0;
                int x = 3, y = 3;

                while (true)
                {
                    Console.Clear();

                    (int dx, int dy, bool holding) = JoystickState(sh);

                    if (holding)
                    {
                        n++;
                    }

                    x = (x + 8 + dx) % 8;
                    y = (y + 8 + dy) % 8;

                    sh.Fill(n % 2 == 0 ? Color.DarkBlue : Color.DarkRed);
                    sh.SetPixel(x, y, Color.Yellow);

                    var tempValue = sh.Temperature;
                    var temp2Value = sh.Temperature2;
                    var preValue = sh.Pressure;
                    var humValue = sh.Humidity;
                    var accValue = sh.Acceleration;
                    var angValue = sh.AngularRate;
                    var magValue = sh.MagneticInduction;
                    var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue);

                    Console.WriteLine($"Temperature Sensor 1: {tempValue.Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Temperature Sensor 2: {temp2Value.Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Pressure: {preValue:0.##}hPa");
                    Console.WriteLine($"Altitude: {altValue:0.##}m");
                    Console.WriteLine($"Acceleration: {sh.Acceleration}g");
                    Console.WriteLine($"Angular rate: {sh.AngularRate}DPS");
                    Console.WriteLine($"Magnetic induction: {sh.MagneticInduction}gauss");
                    Console.WriteLine($"Relative humidity: {humValue:0.#}%");
                    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Summer simmer index: {WeatherHelper.CalculateSummerSimmerIndex(tempValue, humValue).Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Saturated vapor pressure: {WeatherHelper.CalculateSaturatedVaporPressure(tempValue).Hectopascal:0.##}hPa");
                    Console.WriteLine($"Actual vapor pressure: {WeatherHelper.CalculateActualVaporPressure(tempValue, humValue).Hectopascal:0.##}hPa");
                    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Absolute humidity: {WeatherHelper.CalculateAbsoluteHumidity(tempValue, humValue):0.#}g/m\u0179");                    

                    Thread.Sleep(1000);
                }
            }
        }

        private static (int, int, bool) JoystickState(SenseHat sh)
        {
            sh.ReadJoystickState();

            int dx = 0;
            int dy = 0;

            if (sh.HoldingUp)
            {
                dy--; // y goes down
            }

            if (sh.HoldingDown)
            {
                dy++;
            }

            if (sh.HoldingLeft)
            {
                dx--;
            }

            if (sh.HoldingRight)
            {
                dx++;
            }

            return (dx, dy, sh.HoldingButton);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

                    Console.WriteLine($"Temperature: Sensor1: {sh.Temperature.Celsius} °C   Sensor2: {sh.Temperature2.Celsius} °C");
                    Console.WriteLine($"Humidity: {sh.Humidity} %rH");
                    Console.WriteLine($"Pressure: {sh.Pressure} hPa");
                    Console.WriteLine($"Acceleration: {sh.Acceleration} g");
                    Console.WriteLine($"Angular rate: {sh.AngularRate} DPS");
                    Console.WriteLine($"Magnetic induction: {sh.MagneticInduction} gauss");

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

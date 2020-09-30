// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Amg88xx.Samples
{
    /// <summary>
    /// Samples for Amg88xx
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            const int I2cBus = 1;
            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(I2cBus, Amg88xx.AlternativeDeviceAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
            Amg88xx amg88xx = new Amg88xx(i2cDevice);

            amg88xx.ClearStatus();

            string avgMode = amg88xx.GetMovingAverageMode() ? "on" : "off";
            Console.WriteLine($"Average mode: {avgMode}");

            while (true)
            {
                Console.WriteLine($"Thermistor: {amg88xx.GetSensorTemperature()}");

                Status status = amg88xx.GetStatus();
                Console.WriteLine($"Status: {status}");

                var image = new Temperature[Amg88xx.Columns, Amg88xx.Rows];
                // var image = new int[Amg88xx.Columns, Amg88xx.Rows];
                image = amg88xx.GetThermalImage();
                for (int r = 0; r < Amg88xx.Rows; r++)
                {
                    for (int c = 0; c < Amg88xx.Columns; c++)
                    {
                        Console.Write($"{((int)image[c, r].DegreesCelsius).ToString().PadRight(10)}");
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();
                Thread.Sleep(1000);
            }
        }
    }
}

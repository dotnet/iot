// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Mhz19b.Samples
{
    /// <summary>
    /// Sample for MH-Z19B sensor
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            Mhz19b sensor = new Mhz19b("/dev/serial0");
            while (true)
            {
                (Ratio concentration, bool validity) reading = sensor.GetCo2Reading();
                if (reading.validity)
                {
                    Console.WriteLine($"{reading.concentration}");
                }
                else
                {
                    Console.WriteLine("Concentration counldn't be read");
                }

                Thread.Sleep(1000);
            }
        }
    }
}

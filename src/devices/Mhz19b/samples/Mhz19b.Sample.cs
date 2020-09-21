// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mhz19b.Samples
{
    /// <summary>
    /// Samples for Mhz19b
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
                var reading = sensor.GetCo2Reading();
                if (reading.Item2)
                {
                    Console.WriteLine($"{reading.Item1}");
                }
                else
                {
                    Console.WriteLine("Concentration counldn't be read");
                }
            }
        }
    }
}

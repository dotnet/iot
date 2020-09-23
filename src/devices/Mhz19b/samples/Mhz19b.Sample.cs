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

            // Switch ABM on (default).
            // sensor.SetAutomaticBaselineCorrection(AbmState.On);

            // Set sensor detection range to 2000ppm (default).
            // sensor.SetSensorDetectionRange(DetectionRange.Range2000);

            // Perform calibration
            // Step #1: perform zero point calibration
            // Step #2: perform span point calibration at 2000ppm
            // CAUTION: enable the following lines only if you know exactly what you do.
            //          Consider also that zero point and span point calibration are performed
            //          at different concentrations. The sensor requires up to 20 min to be
            //          saturated at the target level.
            // sensor.PerformZeroPointCalibration();
            // ---- Now change to target concentration for span point.
            // sensor.PerformSpanPointCalibration(VolumeConcentration.FromPartsPerMillion(200));

            // Continously read current concentration
            while (true)
            {
                (VolumeConcentration concentration, bool validity) reading = sensor.GetCo2Reading();
                if (reading.validity)
                {
                    Console.WriteLine($"{reading.concentration}");
                }
                else
                {
                    Console.WriteLine("Concentration couldn't be read");
                }

                Thread.Sleep(1000);
            }
        }
    }
}

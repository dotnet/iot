// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

using Iot.Device.MettlerToledo.Readings;

namespace Iot.Device.MettlerToledo.Samples
{
    /// <summary>
    /// Samples for MettlerToledo
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the MettlerToledo Sample!");

            // Create a scale object
            var scale = new MettlerToledoDevice("/dev/ttyUSB1");

            // Reset the scale
            scale.Reset();

            // Now fetch a reading, when the scale is stable. This means the scale is confident in the weight.
            MettlerToledoWeightReading reading = scale.GetStableWeight();

            // Print reading to console
            Console.WriteLine($"Received a weight of {reading.Weight.Grams} grams.");
        }
    }
}

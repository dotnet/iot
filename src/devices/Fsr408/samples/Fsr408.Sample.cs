// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.Fsr408.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Fsr408 Sample!");

            // Create Mcp3008 instance depending how you wired ADC pins to controller
            // in this example "bit-banging" wiring method used.
            // please refer https://github.com/dotnet/iot/tree/master/src/devices/Mcp3008/samples for more information
            Fsr408 fsr408 = new Fsr408(new Mcp3008.Mcp3008(18, 23, 24, 25));

            //Below sample demonstrate read the analog signal using ADC Mcp3008 
            using (fsr408)
            {   //Fsr produces analog signal, we can read this with 2 way, using ADC or capacitor
                fsr408.PowerSupplied = 3300; // 3.3v, in milli volts, default is 5000 (5V)
                fsr408.Resistance = 10000;   // set the pull down resistor resistance, default is 10000 (10k ohm)
                
                while (true)
                {
                    int value = fsr408.Read();
                    int voltage = fsr408.CalculateVoltage(value);
                    int resistance = fsr408.CalculateFsrResistance(voltage);
                    int force = fsr408.CalculateForce(resistance);
                    Console.WriteLine($"Read value: {value}, voltage: {voltage}, resistance: {resistance}, approximate force in Newtons: {force}");
                    Thread.Sleep(500);
                }
            }
        }
    }
}

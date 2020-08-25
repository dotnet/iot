// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.UFire.Sample
{
    /// <summary>
    /// This sample is for Raspberry Pi Model 3B+
    /// </summary>
    public static class Program
    {
        private const int BusId = 1;

        private static void PrintHelp()
        {
            Console.WriteLine("Command:");
            Console.WriteLine("    B           Basic");
            Console.WriteLine("    O           Orp");
            Console.WriteLine("    P           PH");
            Console.WriteLine();
        }

        /// <summary>
        /// Example program main entry point
        /// </summary>
        /// <param name="args">Command line arguments see <see cref="PrintHelp"/></param>
        public static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(BusId, UFireIse.I2cAddress);
            I2cDevice device = I2cDevice.Create(settings);

            Console.WriteLine(
                    $"UFire_ISE is ready on I2C bus {device.ConnectionSettings.BusId} with address {device.ConnectionSettings.DeviceAddress}");

            Console.WriteLine();
            PrintHelp();

            while (true)
            {
                var command = Console.ReadLine().ToLower().Split(' ');
                if (string.IsNullOrEmpty(command[0]))
                {
                    return;
                }

                switch (command[0][0])
                {
                    case 'b':
                        Basic(device);
                        return;
                    case '0':
                        Orp(device);
                        return;
                    case 'p':
                        Ph(device);
                        return;
                }
            }
        }

        private static void Basic(I2cDevice device)
        {
            using (UFireIse uFire_ISE = new UFireIse(device))
            {
                Console.WriteLine("mV:" + uFire_ISE.Measure().Millivolts);
            }
        }

        private static void Orp(I2cDevice device)
        {
            using (UFireOrp uFire_orp = new UFireOrp(device))
            {
                Console.WriteLine("mV:" + uFire_orp.Measure().Millivolts);
                Console.WriteLine("Eh:" + uFire_orp.ReductionPotential);
            }
        }

        private static void Ph(I2cDevice device)
        {
            using (UFirePh uFire_pH = new UFirePh(device))
            {
                Console.WriteLine("mV:" + uFire_pH.Measure().Millivolts);

                if (uFire_pH.TryMeasurepH(out float pH))
                {
                    Console.WriteLine("pH:" + pH);
                    Console.WriteLine("pOH:" + uFire_pH.Poh);
                }
                else
                {
                    Console.WriteLine("Not possible to measure pH");
                }
            }
        }
    }

}

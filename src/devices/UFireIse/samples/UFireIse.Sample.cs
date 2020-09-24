// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using UnitsNet;

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
            Console.WriteLine("    O           Read Orp (Oxidation-reduction potential) value");
            Console.WriteLine("    P           Read pH (Power of Hydrogen) value");
            Console.WriteLine();
        }

        /// <summary>
        /// Example program main entry point
        /// </summary>
        /// <param name="args">Command line arguments see <see cref="PrintHelp"/></param>
        public static void Main(string[] args)
        {
            PrintHelp();

            I2cConnectionSettings settings = new I2cConnectionSettings(BusId, UFireIse.I2cAddress);
            using (I2cDevice device = I2cDevice.Create(settings))
            {
                Console.WriteLine(
                        $"UFire_ISE is ready on I2C bus {device.ConnectionSettings.BusId} with address {device.ConnectionSettings.DeviceAddress}");

                Console.WriteLine();

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
        }

        private static void Basic(I2cDevice device)
        {
            using (UFireIse uFireIse = new UFireIse(device))
            {
                Console.WriteLine("mV:" + uFireIse.Read().Millivolts);
            }
        }

        private static void Orp(I2cDevice device)
        {
            using (UFireOrp uFireOrp = new UFireOrp(device))
            {
                if (uFireOrp.TryMeasureOxidationReducationPotential(out ElectricPotential orp))
                {
                    Console.WriteLine("Eh:" + orp.Millivolts);
                }
                else
                {
                    Console.WriteLine("Not possible to measure pH");
                }
            }
        }

        private static void Ph(I2cDevice device)
        {
            using (UFirePh uFire_pH = new UFirePh(device))
            {
                Console.WriteLine("mV:" + uFire_pH.Read().Millivolts);

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

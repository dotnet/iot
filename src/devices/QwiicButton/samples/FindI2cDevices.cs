// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.QwiicButton.Samples
{
    /// <summary>
    /// Finds the connected I2C devices' addresses.
    /// </summary>
    internal class FindI2cDevices
    {
        public static void Run()
        {
            Console.WriteLine("Enter I2C bus ID: [Press Enter for default = 1]");
            if (!Int32.TryParse(Console.ReadLine(), out int i2cBusId))
            {
                Console.WriteLine("You must enter an integer - exiting...");
                return;
            }

            Console.WriteLine($"Scanning I2C bus {i2cBusId}...");
            int devicesFoundCount = 0;

            for (int address = 8; address < 120; address++)
            {
                try
                {
                    using (new QwiicButton(i2cBusId, (byte)address))
                    {
                    }

                    devicesFoundCount++;
                    Console.WriteLine($"Found I2C device on address 0x{BitConverter.ToString(new[] { (byte)address })} ({address})");
                }
                catch
                {
                    // No device on address - try next
                }
            }

            if (devicesFoundCount == 0)
            {
                Console.WriteLine("Could not detect any attached I2C devices!");
            }
        }
    }
}

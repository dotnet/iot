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
            Console.WriteLine("Enter I2C bus ID to scan: [Press Enter for default = 1]");
            var busIdAsString = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(busIdAsString))
            {
                busIdAsString = "1";
            }

            if (!Int32.TryParse(busIdAsString, out int i2cBusId))
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
                    using (var button = new QwiicButton(i2cBusId, (byte)address))
                    {
                        // Try to read device ID - fails if no device on address
                        button.GetDeviceId();
                    }

                    devicesFoundCount++;
                    Console.WriteLine($"Found I2C device on address 0x{address:X} (decimal: {address})");
                }
                catch
                {
                    // No device on address - try next
                }
            }

            Console.WriteLine("Scan finished");

            if (devicesFoundCount == 0)
            {
                Console.WriteLine("Could not detect any attached I2C devices!");
            }

            Console.WriteLine();
        }
    }
}

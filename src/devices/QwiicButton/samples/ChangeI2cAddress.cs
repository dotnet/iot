// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.QwiicButton.Samples
{
    /// <summary>
    /// Lets the user change the I2C address.
    /// </summary>
    internal class ChangeI2cAddress
    {
        public static void Run(QwiicButton button)
        {
            Console.WriteLine("Change I2C address sample started");
            Console.WriteLine("---------------------------------");

            var address = EnterNewAddress();
            if (address == null)
            {
                return;
            }

            button.ChangeI2cAddressAndDispose((byte)address.Value);
            Console.WriteLine($"Successfully changed the I2C address of the button to {address}!");
            Console.WriteLine("Notice that the current instance of the QwiicButton class was now actively disposed");
            Console.WriteLine("since the underlying I2cDevice instance was not reconfigured to use the new address.");
        }

        public static uint? EnterNewAddress()
        {
            Console.WriteLine("Enter a new I2C address for the Qwiic Button to use.");
            Console.WriteLine("Use hexadecimal notation, but don't use the 0x prefix.");
            Console.WriteLine("For instance, if you wanted to change the address to 0x5B,");
            Console.WriteLine("you would enter 5B and press enter.");

            try
            {
                var address = Convert.ToUInt32(Console.ReadLine(), 16);
                Console.WriteLine($"Using I2C address 0x{address:X} (decimal: {address})");
                return address;
            }
            catch
            {
                Console.WriteLine("Could not parse hexadecimal value - exiting...");
                return null;
            }
        }
    }
}

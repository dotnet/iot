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

            if (!button.SetI2cAddress((byte)address.Value))
            {
                return;
            }

            Console.WriteLine($"Successfully changed the I2C address of the button to {address}!");
        }

        public static uint? EnterNewAddress()
        {
            Console.WriteLine("Enter a new I2C address for the Qwiic Button to use.");
            Console.WriteLine("Don't use the 0x prefix. For instance, if you wanted to");
            Console.WriteLine("change the address to 0x5B, you would enter 5B and press enter.");

            try
            {
                var address = Convert.ToUInt32(Console.ReadLine(), 16);
                Console.WriteLine($"Using I2C address 0x{BitConverter.ToString(new[] { (byte)address })} (decimal: {address})");
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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.QwiicButton.Samples
{
    /// <summary>
    /// Checks whether the button is pressed and then prints its status in the console.
    /// </summary>
    internal class PrintButtonStatus
    {
        public static void Run(QwiicButton button)
        {
            Console.WriteLine("Print button status sample started - press ESC to stop");

            do
            {
                while (!Console.KeyAvailable)
                {
                    // Check if button is pressed, and tell us if it is!
                    if (button.IsPressedDown())
                    {
                        Console.WriteLine("The button is pressed!");
                        while (button.IsPressedDown())
                        {
                            Thread.Sleep(10); // Wait for user to stop pressing
                        }

                        Console.WriteLine("The button is not pressed anymore!");
                    }

                    Thread.Sleep(20); // Don't hammer too hard on the I2c bus
                }
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}

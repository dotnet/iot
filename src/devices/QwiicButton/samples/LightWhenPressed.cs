// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.QwiicButton.Samples
{
    /// <summary>
    /// Turns on the button's built-in LED when pressed and prints its status in the console.
    /// </summary>
    internal class LightWhenPressed
    {
        // The brightness to set the LED to when the button is pushed.
        // Can be any value between 0 (off) and 255 (max).
        private const byte Brightness = 128;

        public static void Run(QwiicButton button)
        {
            Console.WriteLine("Light when pressed sample started - press ESC to stop");
            button.LedOff();

            do
            {
                while (!Console.KeyAvailable)
                {
                    // Check if button is pressed and tell us if it is!
                    if (button.IsPressedDown())
                    {
                        Console.WriteLine("The button is pressed!");
                        button.LedOn(Brightness);

                        while (button.IsPressedDown())
                        {
                            Thread.Sleep(10); // Wait for user to stop pressing
                        }

                        Console.WriteLine("The button is not pressed anymore.");
                        button.LedOff();
                    }

                    Thread.Sleep(20); // Don't hammer too hard on the I2C bus
                }
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}

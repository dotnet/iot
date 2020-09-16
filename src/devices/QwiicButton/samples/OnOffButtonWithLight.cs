// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.QwiicButton.Samples
{
    /// <summary>
    /// ON/OFF button where the built-in LED is lit when the button is ON.
    /// </summary>
    internal class OnOffButtonWithLight
    {
        // The brightness to set the LED to when the button is pushed.
        // Can be any value between 0 (off) and 255 (max).
        private const byte Brightness = 128;

        public static void Run(QwiicButton button)
        {
            Console.WriteLine("ON/OFF button sample started - press ESC to stop");
            button.LedOff();
            bool isOn = false;

            do
            {
                while (!Console.KeyAvailable)
                {
                    // Check if button is pressed and tell us if it is!
                    if (button.IsPressed())
                    {
                        if (!isOn)
                        {
                            button.LedOn(Brightness);
                            isOn = true;
                            Console.WriteLine("The button is ON!");
                        }
                        else
                        {
                            button.LedOff();
                            isOn = false;
                            Console.WriteLine("The button is OFF!");
                        }

                        while (button.IsPressed())
                        {
                            Thread.Sleep(10); // Wait for user to stop pressing
                        }
                    }

                    Thread.Sleep(20); // Don't hammer too hard on the I2C bus
                }
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}

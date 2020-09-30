// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.QwiicButton.Samples
{
    /// <summary>
    /// Sets the button's built-in LED to light up and pulse when pressed and prints its status in the console.
    /// </summary>
    internal class PulseWhenPressed
    {
        // Define LED characteristics
        private const byte Brightness = 250; // The maximum brightness of the pulsing LED. Can be between 0 (min) and 255 (max).
        private const ushort CycleTime = 1000; // The total time for the pulse to take. Set to a bigger number for a slower pulse, or a smaller number for a faster pulse.
        private const ushort OffTime = 200; // The total time to stay off between pulses. Set to 0 to be pulsing continuously.

        public static void Run(QwiicButton button)
        {
            Console.WriteLine("Pulse when pressed sample started - press ESC to stop");
            button.LedOff();

            do
            {
                while (!Console.KeyAvailable)
                {
                    // Check if button is pressed and tell us if it is!
                    if (button.IsPressed())
                    {
                        Console.WriteLine("The button is pressed!");
                        button.LedConfig(Brightness, CycleTime, OffTime);

                        while (button.IsPressed())
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

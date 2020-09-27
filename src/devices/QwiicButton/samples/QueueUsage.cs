// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.QwiicButton.Samples
{
    /// <summary>
    /// Shows how to use the clicked and pressed FIFO queues on the Qwiic Button.
    /// </summary>
    internal class QueueUsage
    {
        public static void Run(QwiicButton button)
        {
            Console.WriteLine("FIFO queues sample started");
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("BEWARE: If your button has firmware version 258 (1.2) or earlier, you cannot pop values from the queues due to a bug.");
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

            do
            {
                if (!button.IsPressedQueueEmpty())
                {
                    // If the queue of pressed events is not empty then print the time since the last and first button press
                    Console.WriteLine($"{button.TimeSinceLastPress() / 1000.0}s since the button was last pressed");
                    Console.WriteLine($"{button.TimeSinceFirstPress() / 1000.0}s since the button was first pressed");
                }
                else
                {
                    Console.WriteLine("ButtonPressed queue is empty!");
                }

                if (!button.IsClickedQueueEmpty())
                {
                    // If the queue of clicked events is not empty then print the time since the last and first button click
                    Console.WriteLine($"{button.TimeSinceLastClick() / 1000.0}s since the button was last clicked");
                    Console.WriteLine($"{button.TimeSinceFirstClick() / 1000.0}s since the button was first clicked");
                }
                else
                {
                    Console.WriteLine("ButtonClicked queue is empty!");
                }

                Console.WriteLine();
                Console.WriteLine("Type P to pop value from pressed queue and C to pop value from clicked queue:");
                var consoleKeyInfo = Console.ReadKey();

                if (consoleKeyInfo.KeyChar.ToString().ToLowerInvariant() == "p")
                {
                    // If the character is p or P, then pop a value off of the pressed queue
                    button.PopPressedQueue();
                    Console.WriteLine();
                    Console.WriteLine("Popped value from pressed queue! Press Enter to continue, ESC to exit.");
                }

                if (consoleKeyInfo.KeyChar.ToString().ToLowerInvariant() == "c")
                {
                    // If the character is c or C, then pop a value off of the pressed Queue
                    button.PopClickedQueue();
                    Console.WriteLine();
                    Console.WriteLine("Popped value from clicked queue! Press Enter to continue, ESC to exit.");
                }

                Thread.Sleep(20); // Let's not hammer too hard on the I2C bus
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}

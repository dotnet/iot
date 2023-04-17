// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Tm16xx;

namespace Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("This is a demo using TM1637 on GPIO-23 for CLK and GPIO-24 for DIO.");

            // Create an instance of TM1637, using Gpio 23 for clock and 24 for IO.
            using (Tm1637 tm1637 = new Tm1637(23, 24))
            {
                // When creating the instance without GpioController provided, a new instance of GpioController is created using PinNumberingScheme.Logical from default constructor. The instance of GpioController is disposed with the instance of Tm16xx.
                // Provides an instance of GpioController when constructing Tm16xx instance when specified factory of GpioController is required or for reusing. The instance of GpioContoller provided is not disposed with the instance of Tm16xx.
                // Some board need a delay for self initializing.
                Thread.Sleep(100);

                // Set to the brightest for the next displaying.
                // Note: Setting state by using properties is also supported but using SetScreen is recommended. By using SetScreen instead, not supported properties could be ignored and meaningless device communications are avoided.
                tm1637.SetScreen(7, true, true);
                // Set waitForNextDisplay to true: no data is sent to device but leave it till next digit updates.
                // This will save one communication because the protocol of Tm1637 is defineded to send screen state and digits together.
                // No standalone command for changing screen state only without updating at least one digit.
                // If the screen state need to be changed immediately, leave waitForNextDisplay as false.
                // The default value of waitForNextDisplay is false.

                // Display 12.34
                tm1637.Display(Character.Digit1, Character.Digit2 | Character.Dot, Character.Digit3, Character.Digit4);

                Thread.Sleep(3000);

                // Update digits one by one to ABCD
                tm1637.Display((byte)0, Character.A);
                Thread.Sleep(300);
                tm1637.Display(1, Character.B);
                Thread.Sleep(300);
                tm1637.Display(2, Character.C);
                Thread.Sleep(300);
                tm1637.Display(3, Character.D);

                Thread.Sleep(3000);

                // Flash
                for (int i = 0; i < 5; i++)
                {
                    // turn off the screen
                    tm1637.IsScreenOn = false;
                    Thread.Sleep(200);

                    // turn on the screen and set the brightness to 3
                    tm1637.SetScreen(3, true, false);
                    Thread.Sleep(200);

                    // turn on the screen and set the brightness to 7
                    tm1637.SetScreen(7, true, false);
                    Thread.Sleep(200);
                }

                Console.WriteLine("Press any key to quit...");
                Console.ReadKey(true);

                // Clear before quit.
                tm1637.ClearDisplay();
            }
        }
    }
}

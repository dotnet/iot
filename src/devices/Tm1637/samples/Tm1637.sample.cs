// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Tm1637;
using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;

namespace Tm1637Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Tm1637!");
            Tm1637 tm1637 = new Tm1637(20, 21);
            tm1637.Brightness = 7;
            tm1637.ScreenOn = true;
            tm1637.ClearDisplay();
            // Displays 4 haracters
            // If you have a 4 display, all 4 will be displayed as well as on a 6
            Character[] toDisplay = new Character[4] {
                new Character() { Char = Display.Car4, Dot = false },
                new Character() { Char = Display.Car2, Dot = true},
                new Character() { Char = Display.Car3, Dot = false },
                new Character() { Char = Display.Car8, Dot = false },
            };
            tm1637.Display(toDisplay);
            Thread.Sleep(3000);

            // Changing order of the segments
            tm1637.SegmentOrder = new byte[] { 0, 1, 2, 5, 4, 3 };

            // Displays couple of raw data
            byte[] rawData = new byte[6] {
                // All led on including the dot
                0b1111_1111, 
                // All led off
                0b0000_0000,
                // top blanck, right on, turning like this including dot
                0b1010_1010,
                // top on, right black, turning like this no dot
                0b0101_0101,
                // half one half off
                0b0000_1111, 
                // half off half on
                0b1111_0000,
            };
            // If you have a 4 display, only the fisrt 4 will be displayed
            // on a 6 segment one, all 6 will be displayed
            tm1637.Display(rawData);
            Thread.Sleep(3000);
            // Blink the screen by switching on and off
            for (int i = 0; i < 10; i++)
            {
                tm1637.ScreenOn = !tm1637.ScreenOn;
                tm1637.Display(rawData);
                Thread.Sleep(500);
            }
            tm1637.ScreenOn = true;

            long bright = 0;
            while (!Console.KeyAvailable)
            {
                var dt = DateTime.Now;
                toDisplay[0] = new Character() { Char = (Display)(dt.Minute / 10), Dot = false };
                toDisplay[1] = new Character() { Char = (Display)(dt.Minute % 10), Dot = true };
                toDisplay[2] = new Character() { Char = (Display)(dt.Second / 10), Dot = false };
                toDisplay[3] = new Character() { Char = (Display)(dt.Second % 10), Dot = false };
                tm1637.Brightness = (byte)(bright++ % 8);
                tm1637.Display(toDisplay);
                Thread.Sleep(100);
            }
            tm1637.ScreenOn = false;
            tm1637.ClearDisplay();
            
        }        
    }
}

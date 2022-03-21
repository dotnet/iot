// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.CharacterLcd;

namespace CharacterLcd.Samples
{
    internal static class LargeValueSample
    {
        /// <summary>
        /// This demonstrates the use of a large font on a 20x4 display. This is useful especially when showing values that should be readable from farther away,
        /// such as the time, or a temperature.
        /// </summary>
        /// <param name="lcd">The display</param>
        public static void LargeValueDemo(Hd44780 lcd)
        {
            LcdValueUnitDisplay value = new LcdValueUnitDisplay(lcd, CultureInfo.CurrentCulture);
            value.InitForRom("A00");
            value.Clear();
            Console.WriteLine("Big clock test");
            while (!Console.KeyAvailable)
            {
                value.DisplayTime(DateTime.Now, "T");
                Thread.Sleep(200);
            }

            Console.ReadKey(true);
            Console.WriteLine("Showing fake temperature");
            value.DisplayValue("24.2 °C", "Temperature");
            Console.ReadKey(true);

            Console.WriteLine("Now showing a text");
            CancellationTokenSource src = new CancellationTokenSource();
            value.DisplayBigTextAsync("The quick brown fox jumps over the lazy dog at 10.45PM on May, 3rd", TimeSpan.FromSeconds(0.5), src.Token);
            Console.ReadKey(true);
            src.Cancel();
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.CharacterLcd.Samples
{
    internal static class Aip31068Sample
    {
        private const int DefaultBusId = 1;
        private const int DefaultAddress = 0x3E;

        public static void Run()
        {
            using I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(DefaultBusId, DefaultAddress));
            using Aip31068Lcd lcd = new(device);

            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.Write("AIP31068 sample");
            DisplayContrast(lcd);

            Console.WriteLine("AIP31068 LCD sample ready.");
            Console.WriteLine("Use '+' or '-' to adjust contrast, 'B' to toggle the booster, 'I' to toggle icons.");
            Console.WriteLine("Press Esc to exit.");

            while (true)
            {
                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(50);
                    continue;
                }

                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }

                switch (key.Key)
                {
                    case ConsoleKey.Add:
                    case ConsoleKey.OemPlus:
                        AdjustContrast(lcd, +1);
                        break;
                    case ConsoleKey.Subtract:
                    case ConsoleKey.OemMinus:
                        AdjustContrast(lcd, -1);
                        break;
                    case ConsoleKey.B:
                        lcd.BoosterEnabled = !lcd.BoosterEnabled;
                        Console.WriteLine($"Booster {(lcd.BoosterEnabled ? "enabled" : "disabled")}.");
                        break;
                    case ConsoleKey.I:
                        lcd.IconDisplayEnabled = !lcd.IconDisplayEnabled;
                        Console.WriteLine($"Icon display {(lcd.IconDisplayEnabled ? "enabled" : "disabled")}.");
                        break;
                    default:
                        continue;
                }

                DisplayContrast(lcd);
            }

            lcd.Clear();
            lcd.Write("Goodbye!");
        }

        private static void AdjustContrast(Aip31068Lcd lcd, int delta)
        {
            int contrast = lcd.Contrast + delta;
            contrast = Math.Clamp(contrast, 0, 63);
            lcd.Contrast = (byte)contrast;
            Console.WriteLine($"Contrast set to {lcd.Contrast}.");
        }

        private static void DisplayContrast(Aip31068Lcd lcd)
        {
            lcd.SetCursorPosition(0, 1);
            lcd.Write($"Contrast: {lcd.Contrast:D2}  ");
        }
    }
}


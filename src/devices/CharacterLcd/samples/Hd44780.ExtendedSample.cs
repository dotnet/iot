// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Timers;
using Iot.Device.Mcp23xxx;

namespace Iot.Device.CharacterLcd.Samples
{
    class ExtendedSample
    {
        const string Twenty = "123456789\u0008123456789\u0009";
        const string Thirty = Twenty + "123456789\u000a";
        const string Fourty = Thirty + "123456789\u000b";
        const string Eighty = Fourty + "123456789\u000c123456789\u000d123456789\u000e123456789\u000f";

        public static void Test()
        {
            Console.WriteLine("Starting...");

#if USEI2C
            var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x21));
            var controller = new Mcp23008(i2cDevice);
            var lcd = new Lcd1602(registerSelectPin: 1, enablePin: 2, dataPins: new int[] { 3, 4, 5, 6 }, backlightPin: 7, controller: controller);
#elif USERGB
            var i2cLcdDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x3E));
            var i2cRgbDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x62));
            var lcd = new LcdRgb1602(i2cLcdDevice, i2cRgbDevice);
#else
            Hd44780 lcd = new Hd44780(new Size(20, 4), LcdInterface.CreateGpio(12, 26, new int[] { 16, 17, 18, 19, 20, 21, 22, 23 }, readWritePin: 13));
#endif
            using (lcd)
            {
                Console.WriteLine("Initialized");
                Console.ReadLine();

                TestPrompt("SetCursor", lcd, SetCursorTest);
                TestPrompt("Underline", lcd, l => l.UnderlineCursorVisible = true);
                lcd.UnderlineCursorVisible = false;
                TestPrompt("Walker", lcd, WalkerTest);
                CreateTensCharacters(lcd);
                TestPrompt("CharacterSet", lcd, CharacterSet);

                // Shifting
                TestPrompt("Autoshift", lcd, AutoShift);
                TestPrompt("DisplayLeft", lcd, l => ShiftDisplayTest(l, a => a.ShiftDisplayLeft()));
                TestPrompt("DisplayRight", lcd, l => ShiftDisplayTest(l, a => a.ShiftDisplayRight()));
                TestPrompt("CursorLeft", lcd, l => ShiftCursorTest(l, a => a.ShiftCursorLeft()));
                TestPrompt("CursorRight", lcd, l => ShiftCursorTest(l, a => a.ShiftCursorRight()));

                // Long string
                TestPrompt("Twenty", lcd, l => l.Write(Twenty));
                TestPrompt("Fourty", lcd, l => l.Write(Fourty));
                TestPrompt("Eighty", lcd, l => l.Write(Eighty));

                TestPrompt("Twenty-", lcd, l => WriteFromEnd(l, Twenty));
                TestPrompt("Fourty-", lcd, l => WriteFromEnd(l, Fourty));
                TestPrompt("Eighty-", lcd, l => WriteFromEnd(l, Eighty));

                TestPrompt("Wrap", lcd, l => l.Write(new string('*', 80) + ">>>>>"));
                TestPrompt("Perf", lcd, PerfTests);

#if USERGB
                TestPrompt("Colors", lcd, SetBacklightColorTest);
#endif

                // Shift display right
                lcd.Write("Hello .NET!");
                try
                {
                    int state = 0;
                    Timer timer = new Timer(1000);
                    timer.Elapsed += (o, e) =>
                    {
                        lcd.SetCursorPosition(0, 1);
                        lcd.Write(DateTime.Now.ToLongTimeString() + " ");
                        if (state == 0)
                        {
                            state = 1;
                        }
                        else
                        {
                            lcd.ShiftDisplayRight();
                            state = 0;
                        }
                    };
                    timer.AutoReset = true;
                    timer.Enabled = true;
                    Console.ReadLine();
                }
                finally
                {
                    lcd.DisplayOn = false;
                    lcd.BacklightOn = false;
                    Console.WriteLine("Done...");
                }
            }
        }

        static void CharacterSet(Hd44780 lcd)
        {
            StringBuilder sb = new StringBuilder(256);

            for (int i = 0; i < 256; i++)
            {
                sb.Append((char)i);
            }

            int character = 0;
            int line = 0;
            Size size = lcd.Size;
            while (character < 256)
            {
                lcd.SetCursorPosition(0, line);
                lcd.Write(sb.ToString(character, Math.Min(size.Width, 256 - character)));
                line++;
                character += size.Width;
                if (line >= size.Height)
                {
                    line = 0;
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        static void AutoShift(Hd44780 lcd)
        {
            lcd.AutoShift = true;
            Size size = lcd.Size;
            lcd.Write(Eighty.Substring(0, size.Width + size.Width / 2));
            lcd.AutoShift = false;
        }

        static void ShiftTest(Hd44780 lcd, Action<Hd44780> action)
        {
            Size size = lcd.Size;
            for (int i = 0; i <= size.Width; i++)
            {
                action(lcd);
                System.Threading.Thread.Sleep(250);
            }
        }

        static void ShiftDisplayTest(Hd44780 lcd, Action<Hd44780> action)
        {
            Size size = lcd.Size;
            lcd.Write(Eighty.Substring(0, size.Height * size.Width));
            ShiftTest(lcd, action);
        }

        static void ShiftCursorTest(Hd44780 lcd, Action<Hd44780> action)
        {
            lcd.BlinkingCursorVisible = true;
            ShiftTest(lcd, action);
            lcd.BlinkingCursorVisible = false;
        }

        static void WriteFromEnd(Hd44780 lcd, string value)
        {
            lcd.Increment = false;
            lcd.SetCursorPosition(lcd.Size.Width - 1, lcd.Size.Height - 1);
            lcd.Write(value);
            lcd.Increment = true;
        }

        static void WalkerTest(Hd44780 lcd)
        {
            CreateWalkCharacters(lcd);
            string walkOne = new string('\x8', lcd.Size.Width);
            string walkTwo = new string('\x9', lcd.Size.Width);
            for (int i = 0; i < 5; i++)
            {
                lcd.SetCursorPosition(0, 0);
                lcd.Write(walkOne);
                System.Threading.Thread.Sleep(500);
                lcd.SetCursorPosition(0, 0);
                lcd.Write(walkTwo);
                System.Threading.Thread.Sleep(500);
            }
        }

        static void SetCursorTest(Hd44780 lcd)
        {
            Size size = lcd.Size;
            int number = 0;
            for (int i = 0; i < size.Height; i++)
            {
                lcd.SetCursorPosition(0, i);
                lcd.Write($"{number++}");
                lcd.SetCursorPosition(size.Width - 1, i);
                lcd.Write($"{number++}");
            }
        }

        static void PerfTests(Hd44780 lcd)
        {
            string stars = new string('*', 80);
            Stopwatch stopwatch = Stopwatch.StartNew();
            lcd.Clear();
            for (int i = 0; i < 25; i++)
            {
                lcd.Write(Eighty);
                lcd.Write(stars);
            }

            lcd.Clear();
            stopwatch.Stop();
            string result = $"Elapsed ms: {stopwatch.ElapsedMilliseconds}";
            lcd.Write(result);
            Console.WriteLine(result);
        }

        static void SetBacklightColorTest(LcdRgb1602 lcd)
        {
            Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Aqua, Color.Azure,
                Color.Brown, Color.Chocolate, Color.LemonChiffon, Color.Lime, Color.Tomato, Color.Yellow };

            foreach (var color in colors)
            {
                lcd.Clear();
                lcd.Write(color.Name);

                lcd.SetBacklightColor(color);
                System.Threading.Thread.Sleep(1000);
            }

            lcd.Clear();
            lcd.SetBacklightColor(Color.White);
        }

        static void TestPrompt<T>(string test, T lcd, Action<T> action) where T : Hd44780
        {
            string prompt = $"Test {test}:";
            lcd.Clear();
            lcd.Write(prompt);
            lcd.BlinkingCursorVisible = true;
            Console.Write(prompt);
            Console.ReadLine();
            lcd.BlinkingCursorVisible = false;
            lcd.Clear();
            action(lcd);
            Console.Write("Test Complete:");
            Console.ReadLine();
            lcd.Clear();
        }

        static void CreateWalkCharacters(Hd44780 lcd)
        {
            // Walk 1
            lcd.CreateCustomCharacter(0,
                0b_00110,
                0b_00110,
                0b_01100,
                0b_10111,
                0b_00100,
                0b_01110,
                0b_01010,
                0b_10001);
            // Walk 2
            lcd.CreateCustomCharacter(1,
                0b_00110,
                0b_00110,
                0b_01100,
                0b_01100,
                0b_00110,
                0b_00110,
                0b_01010,
                0b_01010);
        }

        static void CreateTensCharacters(Hd44780 lcd)
        {
            // 10
            lcd.CreateCustomCharacter(0,
                0b_10000,
                0b_10000,
                0b_10000,
                0b_10000,
                0b_10111,
                0b_00101,
                0b_00101,
                0b_00111);
            // 20
            lcd.CreateCustomCharacter(1,
                0b_11100,
                0b_00100,
                0b_11100,
                0b_10000,
                0b_11111,
                0b_00101,
                0b_00101,
                0b_00111);
            // 30
            lcd.CreateCustomCharacter(2,
                0b_11100,
                0b_00100,
                0b_11100,
                0b_00100,
                0b_11111,
                0b_00101,
                0b_00101,
                0b_00111);
            // 40
            lcd.CreateCustomCharacter(3,
                0b_10100,
                0b_10100,
                0b_11100,
                0b_00100,
                0b_00111,
                0b_00101,
                0b_00101,
                0b_00111);
            // 50
            lcd.CreateCustomCharacter(4,
                0b_11100,
                0b_10000,
                0b_11100,
                0b_00100,
                0b_11111,
                0b_00101,
                0b_00101,
                0b_00111);
            // 60
            lcd.CreateCustomCharacter(5,
                0b_11100,
                0b_10000,
                0b_11100,
                0b_10100,
                0b_11111,
                0b_00101,
                0b_00101,
                0b_00111);
            // 70
            lcd.CreateCustomCharacter(6,
                0b_11100,
                0b_00100,
                0b_01000,
                0b_01000,
                0b_01111,
                0b_00101,
                0b_00101,
                0b_00111);
            // 80
            lcd.CreateCustomCharacter(7,
                0b_11100,
                0b_10100,
                0b_11100,
                0b_10100,
                0b_11111,
                0b_00101,
                0b_00101,
                0b_00111);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Spi;
using System.Drawing;
using CommandLine;
using Iot.Device.Graphics;
using Ws28xx.Samples;

namespace Iot.Device.Ws28xx.Samples
{
    internal class Program
    {
        // Configure the count of pixels
        private const int Count = 8;
        private static Ws28xx s_neo;
        public static void Main()
        {
            Console.Clear();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);
            Run();
        }

        private static void Run()
        {
            var settings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 2_400_000,
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };
            var spi = SpiDevice.Create(settings);

#if WS2808
            s_neo = new Ws2808(spi, count);
#else
            s_neo = new Ws2812b(spi, Count);
#endif
            while (true)
            {
                ColorWipe(s_neo, Color.White, Count);
                ColorWipe(s_neo, Color.Red, Count);
                ColorWipe(s_neo, Color.Green, Count);
                ColorWipe(s_neo, Color.Blue, Count);

                TheatreChase(s_neo, Color.White, Count);
                TheatreChase(s_neo, Color.Red, Count);
                TheatreChase(s_neo, Color.Green, Count);
                TheatreChase(s_neo, Color.Blue, Count);

                Rainbow(s_neo, Count);
                RainbowCycle(s_neo, Count);
                TheaterChaseRainbow(s_neo, Count);

            }

        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            throw new NotImplementedException();
        }

        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            BitmapImage img = s_neo.Image;
            img.Clear();
            s_neo.Update();
            Console.Clear();
        }

        public static void ColorWipe(Ws28xx neo, Color color, int count)
        {
            BitmapImage img = neo.Image;
            for (var i = 0; i < count; i++)
            {
                img.SetPixel(i, 0, color);
                neo.Update();
            }
        }

        public static void TheatreChase(Ws28xx neo, Color color, int count, int iterations = 10)
        {
            BitmapImage img = neo.Image;
            for (var i = 0; i < iterations; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    for (var k = 0; k < count; k += 3)
                    {
                        img.SetPixel(j + k, 0, color);
                    }

                    neo.Update();
                    System.Threading.Thread.Sleep(100);
                    for (var k = 0; k < count; k += 3)
                    {
                        img.SetPixel(j + k, 0, Color.Black);
                    }
                }
            }
        }

        public static Color Wheel(int position)
        {
            if (position < 85)
            {
                return Color.FromArgb(position * 3, 255 - position * 3, 0);
            }
            else if (position < 170)
            {
                position -= 85;
                return Color.FromArgb(255 - position * 3, 0, position * 3);
            }
            else
            {
                position -= 170;
                return Color.FromArgb(0, position * 3, 255 - position * 3);
            }
        }

        public static void Rainbow(Ws28xx neo, int count, int iterations = 1)
        {
            BitmapImage img = neo.Image;
            for (var i = 0; i < 255 * iterations; i++)
            {
                for (var j = 0; j < count; j++)
                {
                    img.SetPixel(j, 0, Wheel((i + j) & 255));
                }

                neo.Update();
            }
        }

        public static void RainbowCycle(Ws28xx neo, int count, int iterations = 1)
        {
            BitmapImage img = neo.Image;
            for (var i = 0; i < 255 * iterations; i++)
            {
                for (var j = 0; j < count; j++)
                {
                    img.SetPixel(j, 0, Wheel(((int)(j * 255 / count) + i) & 255));
                }

                neo.Update();
            }
        }

        public static void TheaterChaseRainbow(Ws28xx neo, int count)
        {
            BitmapImage img = neo.Image;
            for (var i = 0; i < 255; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    for (var k = 0; k < count; k += 3)
                    {
                        img.SetPixel(k + j, 0, Wheel((k + i) % 255));
                    }

                    neo.Update();
                    System.Threading.Thread.Sleep(100);

                    for (var k = 0; k < count; k += 3)
                    {
                        img.SetPixel(k + j, 0, Color.Black);
                    }
                }
            }
        }

    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace Iot.Device.Apa102.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var spiDevice = SpiDevice.Create(new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 30_000_000, // up to 30 MHz
                DataFlow = DataFlow.MsbFirst,
                Mode = SpiMode.Mode0 // ensure data is ready at clock rising edge
            });

            var apa102 = new Apa102(spiDevice, 16);

            var random = new Random();
            for (var i = 0; i < apa102.Count(); i++)
            {
                apa102.Pixels[i] = Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256));
            }

            while (true)
            {
                Flow(apa102.Pixels);
                apa102.Flush();
                Thread.Sleep(10);
            }
        }

        private static void Flow(Span<Color> pixels)
        {
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.FromArgb(pixels[i].A, pixels[i].R, pixels[i].G, pixels[i].B);
            }
        }
    }
}

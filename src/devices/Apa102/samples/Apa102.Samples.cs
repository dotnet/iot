// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading;

namespace Iot.Device.Apa102.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var random = new Random();

            var spiDevice = SpiDevice.Create(new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 20_000_000,
                DataFlow = DataFlow.MsbFirst,
                Mode = SpiMode.Mode0 // ensure data is ready at clock rising edge
            });
            var apa102 = new Apa102(spiDevice, 16);

            while (true)
            {
                for (var i = 0; i < apa102.Pixels.Length; i++)
                {
                    apa102.Pixels[i] = Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256));
                }

                apa102.Flush();
                Thread.Sleep(1000);
            }
        }
    }
}

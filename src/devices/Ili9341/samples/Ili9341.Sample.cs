// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Threading;

namespace Iot.Device.Ili9341.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Ili9341 display = new Ili9341(CreateDefaultSpi(), 6, 5))
            {
                Bitmap bmp = null;//new Bitmap("foo.bmp");
                display.Draw(bmp);

                //Thread.Sleep(2000);
            }
        }

        private static SpiDevice CreateDefaultSpi()
        {
            var settings = new SpiConnectionSettings(0, 0) {
                ClockFrequency = 64_000_000,
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };
            
            return new UnixSpiDevice(settings);
        }
    }
}

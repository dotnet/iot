// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Drawing;

namespace Iot.Device.Ws2812b.Samples
{
    class Program
    {
        static void Main()
        {
		    // Create a Neo Pixel x8 stick on spi 0.0
            var spi = new UnixSpiDevice(new SpiConnectionSettings(0, 0));
            var neo = new Ws2812b(spi, 8);

            // Display basic colors for 5 sec
            BitmapImageNeo3 img = neo.Image;
            img.SetPixel(0, 0, Color.White);
            img.SetPixel(1, 0, Color.Red);
            img.SetPixel(2, 0, Color.Green);
            img.SetPixel(3, 0, Color.Blue);
            img.SetPixel(4, 0, Color.Yellow);
            img.SetPixel(5, 0, Color.Cyan);
            img.SetPixel(6, 0, Color.Magenta);
            img.SetPixel(7, 0, Color.FromArgb(unchecked((int)0xffff8000)));
            neo.Update();
            System.Threading.Thread.Sleep(5000);

            // Fade in first pixel
            byte b = 0;
            img.Clear();
            while (true)
            {
                img.SetPixel(0, 0, Color.FromArgb(0xff, b, b, b));
                neo.Update();
                System.Threading.Thread.Sleep(10);
                b++;
            }
        }
    }
}

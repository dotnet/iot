// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Bindings.Utils;
using Iot.Device.Bindings.WS2812B;

namespace led_blink
{
    class Program
    {
        static void Main()
        {
            var spi = new System.Device.Spi.Drivers.UnixSpiDevice(new System.Device.Spi.SpiConnectionSettings(0, 0));
            var neo = new WS2812B(spi, 8);

            // Display basic colors for 5 sec
            BitmapImageNeo3 img = neo.Image;
            img.SetPixel(0, 0, Color.White   >> 6); // Shift down color brightness (same as divide each component by 64)
            img.SetPixel(1, 0, Color.Red     >> 6);
            img.SetPixel(2, 0, Color.Green   >> 6);
            img.SetPixel(3, 0, Color.Blue    >> 6);
            img.SetPixel(4, 0, Color.Yellow  >> 6);
            img.SetPixel(5, 0, Color.Cyan    >> 6);
            img.SetPixel(6, 0, Color.Magenta >> 6);
            img.SetPixel(7, 0, (Color)0xffff8000 >> 6); // Create orange from ARGB constant
            neo.Update();
            System.Threading.Thread.Sleep(5000);

            // Fade in first pixel
            byte b = 0;
            img.Clear();
            while (true)
            {
                img.SetPixel(0, 0, new Color(b++));
                neo.Update();
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}

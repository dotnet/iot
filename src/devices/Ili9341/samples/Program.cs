// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iot.Device.Ili9341;

const int pinID_DC = 23;
const int pinID_Reset = 24;

using Bitmap dotnetBM = new(240, 320);
using Graphics g = Graphics.FromImage(dotnetBM);
using SpiDevice displaySPI = SpiDevice.Create(new SpiConnectionSettings(0, 0) { Mode = SpiMode.Mode3, DataBitLength = 8, ClockFrequency = 12_000_000 /* 12MHz */ });
using Ili9341 ili9341 = new(displaySPI, pinID_DC, pinID_Reset);

while (true)
{
    foreach (string filepath in Directory.GetFiles(@"images", "*.png").OrderBy(f => f))
    {
        using Bitmap bm = (Bitmap)Bitmap.FromFile(filepath);
        g.Clear(Color.Black);
        g.DrawImageUnscaled(bm, 0, 0);
        ili9341.SendBitmap(dotnetBM);
        Task.Delay(1000).Wait();
    }
}

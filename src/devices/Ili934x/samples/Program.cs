// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iot.Device.Ft4222;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ili934x;

#pragma warning disable CA1416 // Temporarily, will be removed after binding is updated
Console.WriteLine("Are you using Ft4222? Type 'yes' and press ENTER if so, anything else will be treated as no.");
bool isFt4222 = Console.ReadLine() == "yes";

int pinDC = isFt4222 ? 1 : 23;
int pinReset = isFt4222 ? 0 : 24;
int pinLed = isFt4222 ? 2 : -1;

SkiaSharpAdapter.Register();
using SpiDevice displaySPI = isFt4222 ? GetSpiFromFt4222() : GetSpiFromDefault();
GpioController gpio = isFt4222 ? GetGpioControllerFromFt4222() : new GpioController();
using Ili9341 ili9341 = new(displaySPI, pinDC, pinReset, backlightPin: pinLed, gpioController: gpio);

while (true)
{
    using var backBuffer = ili9341.GetBackBufferCompatibleImage();
    foreach (string filepath in Directory.GetFiles(@"images", "*.png").OrderBy(f => f))
    {
        Console.WriteLine($"Drawing {filepath}");
        using var image = BitmapImage.CreateFromFile(filepath);
        var api = backBuffer.GetDrawingApi();
        api.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height));
        ili9341.DrawBitmap(backBuffer);
        ili9341.SendFrame(true);
        Task.Delay(1000).Wait();
    }

    Console.WriteLine("FillRect(Color.Red, 120, 160, 60, 80)");
    ili9341.FillRect(Color.Red, 120, 160, 60, 80);
    ili9341.SendFrame(true);
    Task.Delay(1000).Wait();

    Console.WriteLine("FillRect(Color.Blue, 0, 0, 240, 320)");
    ili9341.FillRect(Color.Blue, 0, 0, 240, 320);
    ili9341.SendFrame(true);
    Task.Delay(1000).Wait();

    Console.WriteLine("ClearScreen()");
    ili9341.ClearScreen();
    Task.Delay(1000).Wait();

    Console.WriteLine("FillRect(Color.Green, 0, 0, 120, 160)");
    ili9341.FillRect(Color.Green, 0, 0, 120, 160);
    ili9341.SendFrame(true);
    Task.Delay(1000).Wait();
}

GpioController GetGpioControllerFromFt4222()
{
    return new GpioController(new Ft4222Gpio());
}

SpiDevice GetSpiFromFt4222()
{
    return new Ft4222Spi(new SpiConnectionSettings(0, 1) { ClockFrequency = Ili9341.DefaultSpiClockFrequency, Mode = Ili9341.DefaultSpiMode });
}

SpiDevice GetSpiFromDefault()
{
    return SpiDevice.Create(new SpiConnectionSettings(0, 0) { ClockFrequency = Ili9341.DefaultSpiClockFrequency, Mode = Ili9341.DefaultSpiMode });
}

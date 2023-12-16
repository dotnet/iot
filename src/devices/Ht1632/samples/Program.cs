// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.Device.Gpio;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ht1632;

SkiaSharpAdapter.Register();
Console.WriteLine("Initialize HT1632 with 24-ROW x 16-COM, RC-primary, PWM 1/16 duty.");
Console.WriteLine("cs: 27, wr: 22, data: 17");
using var ht1632 = new Ht1632(new Ht1632PinMapping(cs: 27, wr: 22, data: 17), new GpioController())
{
    ComOption = ComOption.NMos16Com,
    ClockMode = ClockMode.RcPrimary,
    Enabled = true,
    PwmDuty = 1,
    Blink = false,
    LedOn = true
};

Clear();
Console.ReadLine();

RandomDots();
Console.ReadLine();

ShowImage();

void Clear()
{
    Console.WriteLine();
    Console.WriteLine("Clear");

    // HT1632 has 4-bit RAM, one byte corresponds to one address
    // Only lower 4 bits are valid
    var data = new byte[24 * 16 / 4];
    ht1632.WriteData(0, data);
}

void RandomDots()
{
    Console.WriteLine("Random dots");

    var data = new byte[24 * 16 / 4];
    var random = new Random();
    for (var i = 0; i < data.Length; i++)
    {
        data[i] = (byte)random.Next();
    }

    ht1632.WriteData(0, data);
}

void ShowImage()
{
    Console.WriteLine("Show image");

    var image = BitmapImage.CreateFromFile("./dotnet-bot.bmp");
    ht1632.ShowImageWith16Com(image);
}

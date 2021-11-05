// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using Iot.Device.Ht1632;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

Console.WriteLine("Initialize HT1632 with 24-ROW x 16-COM, RC-master, PWM 1/16 duty.");
Console.WriteLine("cs: 27, wr: 22, data: 17");
using var ht1632 = new Ht1632(new Ht1632PinMapping(cs: 27, wr: 22, data: 17), new GpioController())
{
    ComOption = ComOption.NMos16Com,
    ClockMode = ClockMode.RcMaster,
    Enabled = true,
    PwmDuty = 1,
    Blink = false,
    LedOn = true
};
{
    Console.WriteLine();
    Console.WriteLine("Clear");

    var data = new byte[24 * 16 / 4];
    ht1632.WriteData(0, data);
    Console.ReadLine();
}

{
    Console.WriteLine("Random dots");

    var data = new byte[24 * 16 / 4];
    var random = new Random();
    for (var i = 0; i < data.Length; i++)
    {
        data[i] = (byte)(random.Next());
    }

    ht1632.WriteData(0, data);
    Console.ReadLine();
}

{
    Console.WriteLine("Show image");

    var image = Image.Load<Rgba32>("./dotnet-bot.bmp");
    var data = new byte[24 * 16 / 4];

    for (var y = 0; y < 24; y++)
    {
        for (var x = 0; x < 16; x += 4)
        {
            var index = (x + 16 * y) / 4;
            var value = (byte)(
                (image[x + 0, y].R > 127 ? 0b00000001 << 3 : 0) |
                (image[x + 1, y].R > 127 ? 0b00000001 << 2 : 0) |
                (image[x + 2, y].R > 127 ? 0b00000001 << 1 : 0) |
                (image[x + 3, y].R > 127 ? 0b00000001 << 0 : 0));
            data[index] = value;
        }
    }

    ht1632.WriteData(0, data);
    Console.ReadLine();
}

Console.WriteLine("All end");

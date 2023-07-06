// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using System.Device.Spi;
using Iot.Device.Ssd1331;

const int pinID_DC = 23;
const int pinID_Reset = 24;

using SpiDevice displaySPI = SpiDevice.Create(new SpiConnectionSettings(0, 0) { Mode = SpiMode.Mode3, DataBitLength = 8, ClockFrequency = 12_000_000 /* 12MHz */ });
using Ssd1331 ssd1331 = new(displaySPI, pinID_DC, pinID_Reset);

ssd1331.Reset();
ssd1331.Initialize();
Console.WriteLine("Init done");
while (true)
{
    ssd1331.Contrast(9);
    ssd1331.FillScreen(Color.Red);
    Delay(500);
    ssd1331.FillScreen(Color.Green);
    Delay(500);
    ssd1331.FillScreen(Color.Blue);
    Delay(500);
    ssd1331.FillScreen(Color.White);
    Delay(500);
    ssd1331.ClearScreen();
    Delay(500);
    ssd1331.Circle(20, 40, 30, Color.Blue, true);
    ssd1331.Circle(20, 50, 35, Color.White, false);
    ssd1331.Circle(20, 60, 40, Color.Red, false);
    ssd1331.Line(0, 0, 95, 63, Color.FromArgb(0, 255, 255));
    ssd1331.Line(95, 0, 0, 63, Color.FromArgb(255, 0, 255));
    ssd1331.Rect(10, 10, 90, 60, Color.FromArgb(255, 255, 0));
    ssd1331.FillRect(20, 20, 40, 40, Color.White, Color.Green);
    Delay(2000);

    for (int y = 9; y >= 0; y--)
    {
        ssd1331.Contrast(y);
        ssd1331.Foreground(Color.White);
        ssd1331.Locate(1, 10);
        ssd1331.Print($"Contrast: {y}\nline 2");
        Delay(300);
    }

    Delay(2000);
    ssd1331.ClearScreen();
    ssd1331.Contrast(9);

    ssd1331.SetFontSize(FontSize.High);
    ssd1331.Foreground(Color.Green);
    ssd1331.Locate(0, 10);
    ssd1331.Print("HIGH 12345");

    ssd1331.SetFontSize(FontSize.Wide);
    ssd1331.Foreground(Color.Blue);
    ssd1331.Locate(0, 28);
    ssd1331.Print("WIDE 123");

    ssd1331.SetFontSize(FontSize.WideHigh);
    ssd1331.Foreground(Color.Red);
    ssd1331.Locate(0, 40);
    ssd1331.Print("WH 123");

    ssd1331.SetFontSize(FontSize.Normal);
    ssd1331.Foreground(Color.White);

    ssd1331.ScrollSet(0, 8, 18, 1, 0);
    ssd1331.StartScrolling();
    ShowTime();

    ssd1331.ScrollSet(0, 8, 18, -2, 0);
    ssd1331.StartScrolling();
    ShowTime();

    ssd1331.ScrollSet(0, 8, 18, 3, 0);
    ssd1331.StartScrolling();
    ShowTime();

    ssd1331.ScrollSet(0, 8, 18, -4, 0);
    ssd1331.StartScrolling();
    ShowTime();

    ssd1331.StopScrolling();

    ssd1331.Bitmap16FS(0, 0, "balloon.bmp");
    Delay(5000);
}

void ShowTime()
{
    GetTime();
    Delay(1000);
    GetTime();
    Delay(1000);
    GetTime();
    Delay(1000);
}

void GetTime()
{
    ssd1331.Locate(0, 0);
    ssd1331.Print(DateTime.Now.ToString("h:mm:ss tt"));
}

void Delay(int ms)
{
    Task.Delay(ms).Wait();
}
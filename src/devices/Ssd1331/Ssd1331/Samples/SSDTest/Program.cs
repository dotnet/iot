// See https://aka.ms/new-console-template for more information

using System.Drawing;
using System.Device.Spi;
using Iot.Device.Ssd1331;
const int pinID_DC = 23;
const int pinID_Reset = 24;

//using Bitmap dotnetBM = new(96, 64);
//using Graphics g = Graphics.FromImage(dotnetBM);
using SpiDevice displaySPI = SpiDevice.Create(new SpiConnectionSettings(0, 0) { Mode = SpiMode.Mode3, DataBitLength = 8, ClockFrequency = 12_000_000 /* 12MHz */ });
using Ssd1331 ssd1331 = new(displaySPI, pinID_DC, pinID_Reset);

ssd1331.Reset();
ssd1331.Initialize();
Console.WriteLine("Init done");
while (true)
{
    ssd1331.Contrast(9);
    ssd1331.FillScreen(Color.Red);
    delay(500);
    ssd1331.FillScreen(Color.Green);
    delay(500);
    ssd1331.FillScreen(Color.Blue);
    delay(500);
    ssd1331.FillScreen(Color.White);
    delay(500);
    ssd1331.ClearScreen();
    delay(500);
    ssd1331.Circle(20, 40, 30, Color.Blue, true);
    ssd1331.Circle(20, 50, 35, Color.White, false);
    ssd1331.Circle(20, 60, 40, Color.Red, false);
    ssd1331.Line(0, 0, 95, 63, Color.FromArgb(0, 255, 255));
    ssd1331.Line(95, 0, 0, 63, Color.FromArgb(255, 0, 255));
    ssd1331.Rect(10, 10, 90, 60, Color.FromArgb(255, 255, 0));
    ssd1331.FillRect(20, 20, 40, 40, Color.White, Color.Green);
    delay(2000);

    for (int y = 9; y >= 0; y--)
    {
        ssd1331.Contrast(y);
        ssd1331.Foreground(Color.White);
        ssd1331.Locate(1, 10);
        ssd1331.Print($"Contrast: {y}\nline 2");
        delay(300);
    }
    delay(2000);
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
    gettime(); delay(1000); gettime(); delay(1000); gettime(); delay(1000);

    ssd1331.ScrollSet(0, 8, 18, -2, 0);
    ssd1331.StartScrolling();
    gettime(); delay(1000); gettime(); delay(1000); gettime(); delay(1000);

    ssd1331.ScrollSet(0, 8, 18, 3, 0);
    ssd1331.StartScrolling();
    gettime(); delay(1000); gettime(); delay(1000); gettime(); delay(1000);

    ssd1331.ScrollSet(0, 8, 18, -4, 0);
    ssd1331.StartScrolling();
    gettime(); delay(1000); gettime(); delay(1000); gettime(); delay(1000);

    ssd1331.StopScrolling();

    ssd1331.Bitmap16FS(0, 0, "balloon.bmp");
    delay(5000);
}

void gettime()
{
    ssd1331.Locate(0, 0);
    ssd1331.Print(DateTime.Now.ToString("h:mm:ss tt"));
}

void delay(int ms)
{
    Task.Delay(ms).Wait();
}
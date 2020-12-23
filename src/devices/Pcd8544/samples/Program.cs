// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;
using System.Device.Gpio;
using System.Device.Pwm.Drivers;
using System.Drawing;
using System.IO;
using System.Device.Pwm;
using System;
using System.Threading;
using Iot.Device.Display;
using Iot.Device.Ft4222;

Console.WriteLine("Hello PCD8544, screen of Nokia 5110!");
Console.WriteLine("Please select the platform you want to use:");
Console.WriteLine("  1. Native device like a Raspberry Pi");
Console.WriteLine("  2. FT4222");
var platformChoice = Console.ReadKey();
Console.WriteLine();

if (platformChoice is not object or { KeyChar: not('1' or '2') })
{
    Console.WriteLine("You have to choose a valid platform");
    return;
}

SpiConnectionSettings spiConnection = new(0, 1) { ClockFrequency = 5_000_000, Mode = SpiMode.Mode0, DataFlow = DataFlow.MsbFirst, ChipSelectLineActiveState = PinValue.Low };
Pcd8544 lcd;

int pwmPin = -1;
Console.WriteLine("Which pin/channel do you want to use for PWM? (use negative number for none)");
var pinChoice = Console.ReadLine();
try
{
    pwmPin = Convert.ToInt32(pinChoice);
}
catch
{
    Console.WriteLine("Can't convert the pin number, will assume you don't want to use PWM.");
}

int resetPin = -1;
Console.WriteLine("Which pin do you want to use for Reset? (use negative number for none)");
pinChoice = Console.ReadLine();
try
{
    resetPin = Convert.ToInt32(pinChoice);
}
catch
{
    Console.WriteLine("Can't convert the pin number, will assume you don't want to use Reset.");
}

int dataCommandPin = -1;
Console.WriteLine("Which pin do you want to use for Data Command?");
pinChoice = Console.ReadLine();
try
{
    dataCommandPin = Convert.ToInt32(pinChoice);
}
catch
{
}

if (dataCommandPin < 0)
{
    Console.WriteLine("Can't continue as Data Command pin has to be valid.");
    return;
}

GpioController? gpio = null;
if (platformChoice.KeyChar == '1')
{
    PwmChannel? pwmChannel = pwmPin >= 0 ? PwmChannel.Create(0, pwmPin) : null;
    SpiDevice spi = SpiDevice.Create(spiConnection);
    lcd = new(dataCommandPin, resetPin, spi, pwmChannel);
}
else
{
    gpio = new(PinNumberingScheme.Logical, new Ft4222Gpio());
    Ft4222Spi ft4222Spi = new(spiConnection);
    SoftwarePwmChannel? softPwm = pwmPin >= 0 ? new(pwmPin, 1000, usePrecisionTimer: true, controller: gpio, shouldDispose: false, dutyCycle: 0) : null;
    lcd = new(dataCommandPin, resetPin, ft4222Spi, softPwm, gpio, false);
}

if (lcd is not object)
{
    Console.WriteLine("Something went wrong, can't initialize PCD8544");
    return;
}

for (int i = 0; i < 10; i++)
{
    lcd.SetCursorPosition(0, 0);
    lcd.WriteLine("Test for brightness");
    lcd.Write($"{i * 10} %");
    lcd.BacklightBrightness = i / 10.0f;
    Thread.Sleep(1000);
}

lcd.Clear();
lcd.BacklightBrightness = 0.2f;

for (byte i = 0; i < 128; i++)
{
    lcd.SetCursorPosition(0, 0);
    lcd.WriteLine("Test for contrast");
    lcd.Write($"{i}");
    lcd.Contrast = i;
    Thread.Sleep(100);
}

lcd.Contrast = 40;
lcd.Clear();

lcd.WriteLine("Test temp 0");
lcd.Temperature = Iot.Device.Display.Pcd8544Enums.Temperature.Coefficient0;
Thread.Sleep(1500);
lcd.WriteLine("Test temp 1");
lcd.Temperature = Iot.Device.Display.Pcd8544Enums.Temperature.Coefficient1;
Thread.Sleep(1500);
lcd.WriteLine("Test temp 2");
lcd.Temperature = Iot.Device.Display.Pcd8544Enums.Temperature.Coefficient2;
Thread.Sleep(1500);
lcd.WriteLine("Test temp 3");
lcd.Temperature = Iot.Device.Display.Pcd8544Enums.Temperature.Coefficient3;
Thread.Sleep(1500);
lcd.Temperature = Iot.Device.Display.Pcd8544Enums.Temperature.Coefficient0;
lcd.Clear();

lcd.Write("Display is on and will switch on and off");
for (int i = 0; i < 5; i++)
{
    Thread.Sleep(1000);
    lcd.Enabled = !lcd.Enabled;
}

lcd.Enabled = true;
lcd.Clear();

lcd.SetCursorPosition(0, 0);
lcd.Write("First line");
lcd.SetCursorPosition(0, 1);
lcd.Write("Second one");
lcd.SetCursorPosition(0, 2);
lcd.Write("3rd");
lcd.SetCursorPosition(0, 3);
lcd.Write("Guess!");
lcd.SetCursorPosition(0, 4);
lcd.Write("One more...");
lcd.SetCursorPosition(0, 5);
lcd.Write("last line");

// this will blink the screen
for (int i = 0; i < 6; i++)
{
    lcd.InverseMode = !lcd.InverseMode;
    Thread.Sleep(1000);
}

Bitmap bitmapMe = new(Path.Combine("me.bmp"), true);
var bitmap1 = BitmapToByteArray(bitmapMe);
Bitmap bitmapNokia = new(Path.Combine("nokia_bw.bmp"), true);
var bitmap2 = BitmapToByteArray(bitmapNokia);
// Open a non bitmap and resize it
Bitmap bitmapLarge = new(Image.FromFile(Path.Combine("nonbmp.jpg")), Pcd8544.PixelScreenSize);
Bitmap newBitmapSmall1color = bitmapLarge.Clone(new Rectangle(0, 0, Pcd8544.PixelScreenSize.Width, Pcd8544.PixelScreenSize.Height), System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
var bitmap3 = BitmapToByteArray(newBitmapSmall1color);

for (byte i = 0; i < 2; i++)
{
    lcd.Clear();
    lcd.Write("    Bonjour     .NET IoT!");
    Thread.Sleep(2000);
    lcd.Clear();

    lcd.SetCursorPosition(0, 0);
    lcd.Write("This is me");
    Thread.Sleep(1000);
    // Shows the first bitmap
    lcd.SetByteMap(bitmap1);
    lcd.Refresh();
    Thread.Sleep(1500);

    lcd.SetCursorPosition(0, 0);
    lcd.WriteLine("This is a nice");
    lcd.WriteLine("picture with a");
    lcd.WriteLine("heart displayed");
    lcd.WriteLine("on the screen");
    Thread.Sleep(1000);
    // Shows the second bitmap
    lcd.SetByteMap(bitmap2);
    lcd.Refresh();
    Thread.Sleep(1500);
    lcd.SetCursorPosition(0, 0);
    lcd.WriteLine("Large picture");
    lcd.WriteLine("resized and");
    lcd.WriteLine("changed to");
    lcd.WriteLine("monochrome");
    Thread.Sleep(1000);
    // Shows the second bitmap
    lcd.SetByteMap(bitmap3);
    lcd.Refresh();
    Thread.Sleep(1500);
}

// Adjusting the bias
for (byte i = 0; i < 7; i++)
{
    // Adjusting the bias
    lcd.Bias = i;
    lcd.SetCursorPosition(0, 0);
    lcd.WriteLine("Adjusting bias");
    lcd.Write($"bias = {i}");
    Thread.Sleep(2000);
    lcd.Clear();
}

lcd.Bias = 4;

lcd.Clear();
lcd.WriteLine("This will display a line of random bits");
Thread.Sleep(1500);

byte[] textToSend = new byte[80];
var rand = new Random(123456);
rand.NextBytes(textToSend);
lcd.Clear();
lcd.SetCursorPosition(0, 0);
lcd.Write(textToSend);
Thread.Sleep(1000);
lcd.Clear();

lcd.DrawPoint(5, 5, true);
lcd.DrawLine(0, 0, 15, 35, true);
lcd.DrawRectangle(10, 30, 10, 20, true, true);
lcd.DrawRectangle(12, 32, 6, 16, false, true);
// You should not forget to refresh to draw everything
lcd.Refresh();
Thread.Sleep(2000);
lcd.Clear();

Console.WriteLine("Thank you for your attention!");
lcd.Dispose();
// In case we're using FT4222, gpio needs to be disposed after the screen
gpio?.Dispose();

byte[] BitmapToByteArray(Bitmap bitmap)
{
    if ((bitmap.Width != Pcd8544.PixelScreenSize.Width) || (bitmap.Height != Pcd8544.PixelScreenSize.Height))
    {
        throw new ArgumentException($"{nameof(bitmap)} should be same size as the screen {Pcd8544.PixelScreenSize.Width}x{Pcd8544.PixelScreenSize.Height}");
    }

    byte[] toReturn = new byte[Pcd8544.ScreenSizeBytes];
    int width = Pcd8544.Size.Width;
    Color colWhite = Color.FromArgb(255, 255, 255, 255);
    for (int position = 0; position < Pcd8544.ScreenSizeBytes; position++)
    {
        byte toStore = 0;
        for (int bit = 0; bit < 8; bit++)
        {
            toStore = (byte)(toStore | ((bitmap.GetPixel(position % width, position / width * 8 + bit) == colWhite ? 0 : 1) << bit));
        }

        toReturn[position] = toStore;
    }

    return toReturn;
}

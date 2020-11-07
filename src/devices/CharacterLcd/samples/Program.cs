// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.Gpio;
using System.Device.I2c;
using System.Globalization;
using Iot.Device.Mcp23xxx;
using Iot.Device.CharacterLcd;
using Iot.Device.CharacterLcd.Samples;

// Choose the right setup for your display:
// UsingGpioPins()
// UsingMcp()
UsingHd44780OverI2C();

void UsingGpioPins()
{
    using Lcd1602 lcd = new Lcd1602(registerSelectPin: 22, enablePin: 17, dataPins: new int[] { 25, 24, 23, 18 });
    lcd.Clear();
    lcd.Write("Hello World");
}

void UsingMcp()
{
    using I2cDevice i2CDevice = I2cDevice.Create(new I2cConnectionSettings(1, 0x21));
    using Mcp23008 driver = new (i2CDevice);
    int[] dataPins = { 3, 4, 5, 6 };
    int registerSelectPin = 1;
    int enablePin = 2;
    int backlight = 7;
    using Lcd1602 lcd = new Lcd1602(registerSelectPin, enablePin, dataPins, backlight, controller: new GpioController(PinNumberingScheme.Logical, driver));
    lcd.Clear();

    lcd.Write("Hello World");
    lcd.SetCursorPosition(0, 1);
    lcd.Write(".NET Core");
}

void UsingHd44780OverI2C()
{
    using I2cDevice i2CDevice = I2cDevice.Create(new I2cConnectionSettings(1, 0x27));
    using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2CDevice, false);
    using Hd44780 hd44780 = new Lcd2004(lcdInterface);
    hd44780.UnderlineCursorVisible = false;
    hd44780.BacklightOn = true;
    hd44780.DisplayOn = true;
    hd44780.Clear();
    Console.WriteLine("Display initialized. Press Enter to start tests.");
    Console.ReadLine();
    LcdConsoleSamples.WriteTest(hd44780);
    ExtendedSample.Test(hd44780);
}

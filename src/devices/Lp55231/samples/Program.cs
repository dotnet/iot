using System;
using System.Threading;
using Iot.Device.Lp55231;

using var ledDriver = new Lp55231();

ledDriver.Reset();

Thread.Sleep(100);

ledDriver.Enabled = true;

ledDriver.Misc = MiscFlags.ClockSourceSelection
               | MiscFlags.ExternalClockDetection
               | MiscFlags.ChargeModeGainHighBit
               | MiscFlags.AddressAutoIncrementEnable;

ledDriver.RgbLeds[0].Red = 0xFF;
ledDriver.RgbLeds[1].Green = 0xFF;
ledDriver.RgbLeds[2].Blue = 0xFF;

Console.WriteLine("Should be showing red, green, blue");

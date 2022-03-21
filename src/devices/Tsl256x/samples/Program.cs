// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Tsl256x;

const int PinInterrupt = 4;

Console.WriteLine("Hello TSL256x");
I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Tsl256x.DefaultI2cAddress));
Tsl256x tsl256X = new(i2cDevice, PackageType.Other);
var ver = tsl256X.Version;
string msg = (ver.Major & 0x01) == 0x01 ? $"This is a TSL2561, Version {ver}" : $"This is a TSL2560, Version {ver}";
Console.WriteLine(msg);

tsl256X.IntegrationTime = IntegrationTime.Integration402Milliseconds;
tsl256X.Gain = Gain.Normal;

Console.WriteLine("This will get the illuminance with the standard period of 402 ms integration and normal gain");

while (!Console.KeyAvailable)
{
    var lux = tsl256X.MeasureAndGetIlluminance();
    Console.WriteLine($"Illuminance is {lux.Lux} Lux");
    tsl256X.GetRawChannels(out ushort channel0, out ushort channel1);
    Console.WriteLine($"Raw data channel 0: {channel0}, channel 1: {channel1}");
    Thread.Sleep(500);
}

Console.WriteLine($"Try changing the gain and read it from {tsl256X.Gain} to {Gain.High}");
tsl256X.Gain = Gain.High;
Console.WriteLine($"New gain {tsl256X.Gain}");

Console.WriteLine($"Try changing the integration time and read it from {tsl256X.IntegrationTime} to {IntegrationTime.Integration13_7Milliseconds}");
tsl256X.IntegrationTime = IntegrationTime.Integration13_7Milliseconds;
Console.WriteLine($"New integration time {tsl256X.IntegrationTime}");

Console.WriteLine("Set power on and check it");
tsl256X.Enabled = true;
Console.WriteLine($"Power should be true: {tsl256X.Enabled}");
tsl256X.Enabled = false;
Console.WriteLine($"Power should be false: {tsl256X.Enabled}");
tsl256X.Enabled = true;

GpioController controller = new();
controller.OpenPin(PinInterrupt, PinMode.Input);
Console.WriteLine($"Pin status: {controller.Read(PinInterrupt)}");
Console.WriteLine("Set interruption to test. Read the interrupt pin");
tsl256X.InterruptControl = InterruptControl.TestMode;
tsl256X.Enabled = true;
while (controller.Read(PinInterrupt) == PinValue.High)
{
    Thread.Sleep(1);
}

tsl256X.Enabled = false;
Console.WriteLine($"Interrupt detected, read the value to clear the interrupt");
tsl256X.GetRawChannels(out ushort ch0, out ushort ch1);
Console.WriteLine($"Raw data channel 0 {ch0}, channel 1 {ch1}");
if (controller.Read(PinInterrupt) == PinValue.Low)
{
    Console.WriteLine("Interrupt properly cleaned");
}
else
{
    Console.WriteLine("Interrupt not cleaned");
}

// Adjust those values with a previous measurement to understand the conditions, find a level where then you can
// hide the sensor with your arm and make it going under the minimum level or vice versa with a lamp
tsl256X.SetThreshold(0x0000, 0x00FF);
tsl256X.InterruptPersistence = InterruptPersistence.OutOfRange06IntegrationTimePeriods;
tsl256X.InterruptControl = InterruptControl.LevelInterrupt;
tsl256X.Enabled = true;
while (controller.Read(PinInterrupt) == PinValue.High)
{
    Thread.Sleep(1);
}

Console.WriteLine($"Interrupt detected, read the value to clear the interrupt");
tsl256X.Enabled = false;
tsl256X.GetRawChannels(out ch0, out ch1);
Console.WriteLine($"Raw data channel 0 {ch0}, channel 1 {ch1}");

Console.WriteLine("This will use a manual integration for 5 seconds");
tsl256X.StartManualIntegration();
Thread.Sleep(5000);
tsl256X.StopManualIntegration();
tsl256X.GetRawChannels(out ch0, out ch1);
Console.WriteLine($"Raw data channel 0 {ch0}, channel 1 {ch1}");
Console.WriteLine($"Integration time should then be {IntegrationTime.Manual} = {tsl256X.IntegrationTime}");

﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using System.Collections.Generic;
using Iot.Device.Bno055;
using Iot.Device.Ft4222;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using UnitsNet;
using Iot.Device.FtCommon;

Console.WriteLine("Hello I2C, SPI and GPIO FTFI! FT4222");
Console.WriteLine("Select the test you want to run:");
Console.WriteLine(" 1 Run I2C tests with a BNO055");
Console.WriteLine(" 2 Run SPI tests with a simple HC595 with led blinking on all ports");
Console.WriteLine(" 3 Run GPIO tests with a simple led blinking on GPIO2 port and reading the port");
Console.WriteLine(" 4 Run callback test event on GPIO2 on Failing and Rising");
var key = Console.ReadKey();
Console.WriteLine();

List<FtDevice> devices = FtCommon.GetDevices();
Console.WriteLine($"{devices.Count} FT4222 elements found");
foreach (FtDevice device in devices)
{
    Console.WriteLine($"Description: {device.Description}");
    Console.WriteLine($"Flags: {device.Flags}");
    Console.WriteLine($"Id: {device.Id}");
    Console.WriteLine($"Location Id: {device.LocId}");
    Console.WriteLine($"Serial Number: {device.SerialNumber}");
    Console.WriteLine($"Device type: {device.Type}");
}

if (devices.Count == 0)
{
    Console.WriteLine("No devices connected to run tests.");
    return;
}

// Assuming the device 0 is the first FT4222
Ft4222Device firstDevice = Ft4222Device.GetFt4222()[0];

var (chip, dll) = Ft4222Common.GetVersions();
Console.WriteLine($"Chip version: {chip}");
Console.WriteLine($"Dll version: {dll}");

if (key.KeyChar == '1')
{
    TestI2c(firstDevice);
}

if (key.KeyChar == '2')
{
    TestSpi();
}

if (key.KeyChar == '3')
{
    TestGpio();
}

if (key.KeyChar == '4')
{
    TestEvents();
}

void TestI2c(Ft4222Device device)
{
    using I2cBus ftI2c = device.CreateOrGetI2cBus(0);
    using Bno055Sensor bno055 = new(ftI2c.CreateDevice(Bno055Sensor.DefaultI2cAddress));
    using Bme280 bme280 = new(ftI2c.CreateDevice(Bme280.DefaultI2cAddress));
    bme280.SetPowerMode(Bmx280PowerMode.Normal);

    Console.WriteLine($"Id: {bno055.Info.ChipId}, AccId: {bno055.Info.AcceleratorId}, GyroId: {bno055.Info.GyroscopeId}, MagId: {bno055.Info.MagnetometerId}");
    Console.WriteLine($"Firmware version: {bno055.Info.FirmwareVersion}, Bootloader: {bno055.Info.BootloaderVersion}");
    Console.WriteLine($"Temperature source: {bno055.TemperatureSource}, Operation mode: {bno055.OperationMode}, Units: {bno055.Units}");

    if (bme280.TryReadTemperature(out Temperature temperature))
    {
        Console.WriteLine($"Temperature: {temperature}");
    }
}

void TestSpi()
{
    using Ft4222Spi ftSpi = new(new SpiConnectionSettings(0, 1) { ClockFrequency = 1_000_000, Mode = SpiMode.Mode0 });

    while (!Console.KeyAvailable)
    {
        ftSpi.WriteByte(0xFF);
        Thread.Sleep(500);
        ftSpi.WriteByte(0x00);
        Thread.Sleep(500);
    }
}

void TestGpio()
{
    const int Gpio2 = 2;
    using GpioController gpioController = new(new Ft4222Gpio());

    // Opening GPIO2
    gpioController.OpenPin(Gpio2);
    gpioController.SetPinMode(Gpio2, PinMode.Output);

    Console.WriteLine("Blinking GPIO2");
    while (!Console.KeyAvailable)
    {
        gpioController.Write(Gpio2, PinValue.High);
        Thread.Sleep(500);
        gpioController.Write(Gpio2, PinValue.Low);
        Thread.Sleep(500);
    }

    Console.ReadKey();
    Console.WriteLine("Reading GPIO2 state");
    gpioController.SetPinMode(Gpio2, PinMode.Input);
    while (!Console.KeyAvailable)
    {
        Console.Write($"State: {gpioController.Read(Gpio2)} ");
        Console.CursorLeft = 0;
        Thread.Sleep(50);
    }
}

void TestEvents()
{
    const int Gpio2 = 2;
    using GpioController gpioController = new(new Ft4222Gpio());

    // Opening GPIO2
    gpioController.OpenPin(Gpio2);
    gpioController.SetPinMode(Gpio2, PinMode.Input);

    Console.WriteLine("Setting up events on GPIO2 for rising and failing");

    gpioController.RegisterCallbackForPinValueChangedEvent(Gpio2, PinEventTypes.Falling | PinEventTypes.Rising, MyCallbackFailing);

    Console.WriteLine("Event setup, press a key to remove the failing event");
    while (!Console.KeyAvailable)
    {
        WaitForEventResult res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Falling, new TimeSpan(0, 0, 0, 0, 50));
        if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
        {
            MyCallbackFailing(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
        }

        res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
        if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
        {
            MyCallbackFailing(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
        }
    }

    Console.ReadKey();
    gpioController.UnregisterCallbackForPinValueChangedEvent(Gpio2, MyCallbackFailing);
    gpioController.RegisterCallbackForPinValueChangedEvent(Gpio2, PinEventTypes.Rising, MyCallback);

    Console.WriteLine("Event removed, press a key to remove all events and quit");
    while (!Console.KeyAvailable)
    {
        WaitForEventResult res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
        if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
        {
            MyCallback(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
        }
    }

    gpioController.UnregisterCallbackForPinValueChangedEvent(Gpio2, MyCallback);
}

void MyCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs) =>
    Console.WriteLine($"Event on GPIO {pinValueChangedEventArgs.PinNumber}, event type: {pinValueChangedEventArgs.ChangeType}");

void MyCallbackFailing(object sender, PinValueChangedEventArgs pinValueChangedEventArgs) =>
    Console.WriteLine($"Event on GPIO {pinValueChangedEventArgs.PinNumber}, event type: {pinValueChangedEventArgs.ChangeType}");

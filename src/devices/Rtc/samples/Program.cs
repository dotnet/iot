// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Rtc;

static void TestSystemTime()
{
    DateTime dt = SystemClock.GetSystemTimeUtc();
    Console.WriteLine($"The system time is now {dt}");

    DateTime newTime = new DateTime(2019, 4, 3, 20, 10, 10);
    Console.WriteLine($"Do you want to set the time to {newTime}?");
    if (Console.ReadLine()!.StartsWith("y"))
    {
        SystemClock.SetSystemTimeUtc(newTime);
    }
}

TestSystemTime();

// This project contains DS1307, DS3231, PCF8563
I2cConnectionSettings settings = new(1, Ds3231.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using Ds3231 rtc = new(device);
// set time
rtc.DateTime = DateTime.Now;

// loop
while (true)
{
    // read time
    DateTimeOffset dt = rtc.DateTime;

    Console.WriteLine($"Time: {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
    Console.WriteLine($"Temperature: {rtc.Temperature.DegreesCelsius} ℃");
    Console.WriteLine();

    // wait for a second
    Thread.Sleep(1000);
}

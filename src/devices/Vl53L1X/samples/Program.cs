// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Vl53L1X;

const int xShutPinNo = 23;
const int newI2CAddress = 0x30;

Console.WriteLine("Hello VL53L1X!");

// Turn the on device.
var gpioController = new GpioController();
gpioController.OpenPin(xShutPinNo, PinMode.Output);
gpioController.Write(xShutPinNo, PinValue.High);

using var defaultI2CDevice = I2cDevice.Create(new I2cConnectionSettings(1, Vl53L1X.DefaultI2cAddress));
Vl53L1X.ChangeI2CAddress(defaultI2CDevice, newI2CAddress);
using Vl53L1X vl53L1X = new(I2cDevice.Create(new I2cConnectionSettings(1, newI2CAddress)));

Console.WriteLine($"SensorID: {vl53L1X.SensorId:X}");
vl53L1X.Precision = Precision.Short;

while (!Console.KeyAvailable)
{
   try
   {
       Console.WriteLine($"Distance: {vl53L1X.Distance.Millimeters}");
       Console.WriteLine($"RangeStatus {vl53L1X.RangeStatus}");
   }
   catch (Exception ex)
   {
      Console.WriteLine($"Exception: {ex.Message}");
   }

   Thread.Sleep(500);
}

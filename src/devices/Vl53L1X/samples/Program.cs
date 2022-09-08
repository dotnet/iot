// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading;
using Iot.Device.Vl53L1X;

const int xShutPinNo = 23;
const int newI2CAddress = 0x30;

Console.WriteLine("Hello VL53L1X!");

using Vl53L1X vl53L1X = new(i2cAddress: newI2CAddress, xShutPin: xShutPinNo);

Console.WriteLine($"SensorID: {vl53L1X.GetSensorId():X}");
Console.WriteLine($"Offset in µm: {vl53L1X.GetOffset()}, Signal rate: {vl53L1X.GetSignalRate()}");
Console.WriteLine($"Distance Mode: {vl53L1X.GetDistanceMode()}");
Console.WriteLine($"TimingBudget: {vl53L1X.GetTimingBudgetInMs()}");
vl53L1X.SetDistanceMode(DistanceMode.Short);
Console.WriteLine($"Distance Mode: {vl53L1X.GetDistanceMode()}");
Console.WriteLine($"TimingBudget: {vl53L1X.GetTimingBudgetInMs()}");
Console.WriteLine($"SpadNb: {vl53L1X.GetSpadNb()}");
Console.WriteLine($"InterMeasurementInMs: {vl53L1X.GetInterMeasurementInMs()}");

while (!Console.KeyAvailable)
{
   try
   {
      var dist = vl53L1X.Distance;
      var rangeStatus = vl53L1X.GetRangeStatus();
      Console.WriteLine($"RangeStatus {rangeStatus}");
      Console.WriteLine($"Distance: {dist}");
   }
   catch (Exception ex)
   {
      Console.WriteLine($"Exception: {ex.Message}");
   }

   Thread.Sleep(500);
}

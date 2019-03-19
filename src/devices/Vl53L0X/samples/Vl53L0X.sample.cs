// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;
using Iot.Device.Vl53L0X;

namespace Vl53L0Xsample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello VL53L0X!");
            Vl53L0X vL53L0X = new Vl53L0X(new UnixI2cDevice(new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress)));
            Console.WriteLine($"Rev: {vL53L0X.Information.Revision}, Prod: {vL53L0X.Information.ProductId}, Mod: {vL53L0X.Information.ModuleId}");
            vL53L0X.MeasurementMode = MeasurementMode.Continuous;
            while (!Console.KeyAvailable)
            {
                try
                {
                    var dist = vL53L0X.Distance;
                    if (dist != (UInt16)OperationRange.OutOfRange)
                    {
                        Console.WriteLine($"Distance: {dist}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid data");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
                Thread.Sleep(500);
            }

        }
    }
}

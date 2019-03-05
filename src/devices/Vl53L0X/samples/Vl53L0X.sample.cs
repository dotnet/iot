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
            Console.WriteLine($"Rev: {vL53L0X.Info.Revision}, Prod: {vL53L0X.Info.ProductId}, Mod: {vL53L0X.Info.ModuleId}");
            // vL53L0X.SetPrecision(Precision.HighPrecision);
            while (!Console.KeyAvailable)
            {
                try
                {
                    var dist = vL53L0X.DistanceContinousMillimeters;
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
                // You can use as well single measurement, be aware that reading it may not be that accurate
                // So using this function will return a safe averaged value with few readings or OutOfRange 
                // in case of any issue
                // Console.WriteLine($"Distance: {vL53L0X.GetDistanceSingleMillimeters(true)}");
                Thread.Sleep(500);
            }

        }
    }
}

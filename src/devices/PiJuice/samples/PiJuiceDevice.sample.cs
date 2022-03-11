// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.PiJuiceDevice;
using Iot.Device.PiJuiceDevice.Models;
using UnitsNet;
using UnitsNet.Units;

namespace PiJuiceDevice.Sample
{
    internal class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello PiJuice!");
            I2cConnectionSettings i2CConnectionSettings = new(1, PiJuice.DefaultI2cAddress);
            PiJuice piJuice = new(I2cDevice.Create(i2CConnectionSettings));
            Console.WriteLine($"Manufacturer :{piJuice.PiJuiceInfo.Manufacturer}");
            Console.WriteLine($"Board: {piJuice.PiJuiceInfo.Board}");
            Console.WriteLine($"Firmware version: {piJuice.PiJuiceInfo.FirmwareVersion}");
            PiJuiceStatus piJuiceStatus = new(piJuice);
            PiJuicePower piJuicePower = new(piJuice);
            PiJuiceConfig piJuiceConfig = new(piJuice);
            while (!Console.KeyAvailable)
            {
                Console.Clear();
                Status status = piJuiceStatus.GetStatus();
                Console.WriteLine($"Battery state: {status.Battery}");
                Console.WriteLine($"Battery charge level: {piJuiceStatus.GetChargeLevel()}%");
                Console.WriteLine($"Battery temperature: {piJuiceStatus.GetBatteryTemperature()}");
                ChargingConfig chargeConfig = piJuiceConfig.GetChargingConfig();
                Console.WriteLine($"Battery charging enabled: {chargeConfig.Enabled}");

                // Wake up on charge functionality
                WakeUpOnCharge wakeUp = piJuicePower.WakeUpOnCharge;
                Console.WriteLine($"Is wake up on charge disabled: {wakeUp.Disabled}, Wake up at charging percentage: {wakeUp.WakeUpPercentage}%");
                Console.WriteLine("Set wake up on charge percentage to 60%");
                piJuicePower.WakeUpOnCharge = new(false, new Ratio(60, RatioUnit.Percent));
                Thread.Sleep(5);
                wakeUp = piJuicePower.WakeUpOnCharge;
                Console.WriteLine($"Is wake up on charge disabled: {wakeUp.Disabled}, Wake up at charging percentage: {wakeUp.WakeUpPercentage.Value}%");
                piJuicePower.WakeUpOnCharge = new(true, new Ratio(0, RatioUnit.Percent));

                Thread.Sleep(2000);
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using UnitsNet;
using Iot.Device.Ip5306;

Console.WriteLine("Hello from IP5306!");

I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, Ip5306.SecondaryI2cAddress));
Ip5306 power = new(i2c);

Console.WriteLine($"Current configuration:");
DisplayInfo();

// Configuration for M5Stack
power.ButtonOffEnabled = true;
power.BoostOutputEnabled = false;
power.AutoPowerOnEnabled = true;
power.ChargerEnabled = true;
power.BoostEnabled = true;
power.LowPowerOffEnabled = true;
power.FlashLightBehavior = ButtonPress.Doubleclick;
power.SwitchOffBoostBehavior = ButtonPress.LongPress;
power.BoostWhenVinUnpluggedEnabled = true;
power.ChargingUnderVoltage = ChargingUnderVoltage.V4_55;
power.ChargingLoopSelection = ChargingLoopSelection.Vin;
power.ChargingCurrent = ElectricCurrent.FromMilliamperes(2250);
power.ConstantChargingVoltage = ConstantChargingVoltage.Vm28;
power.ChargingCuttOffVoltage = ChargingCutOffVoltage.V4_17;
power.LightDutyShutdownTime = LightDutyShutdownTime.S32;
power.ChargingCutOffCurrent = ChargingCutOffCurrent.C500mA;
power.ChargingCuttOffVoltage = ChargingCutOffVoltage.V4_2;

Console.WriteLine($"Current for M5Stack:");
DisplayInfo();

Console.WriteLine("Careful when pressing buttons, the behavior can switch off the board.");
while (!Console.KeyAvailable)
{
    var button = power.GetButtonStatus();
    switch (button)
    {
        case ButtonPressed.DoubleClicked:
            Console.WriteLine("double clicked");
            break;
        case ButtonPressed.LongPressed:
            Console.WriteLine("Long pressed");
            break;
        case ButtonPressed.ShortPressed:
            Console.WriteLine("Short pressed");
            break;
        case ButtonPressed.NotPressed:
        default:
            break;
    }

    Thread.Sleep(200);
}

void DisplayInfo()
{
    Console.WriteLine($"  AutoPowerOnEnabled: {power.AutoPowerOnEnabled}");
    Console.WriteLine($"  BoostOutputEnabled: {power.BoostOutputEnabled}");
    Console.WriteLine($"  BoostWhenVinUnpluggedEnabled: {power.BoostWhenVinUnpluggedEnabled}");
    Console.WriteLine($"  BostEnabled: {power.BoostEnabled}");
    Console.WriteLine($"  ButtonOffEnabled: {power.ButtonOffEnabled}");
    Console.WriteLine($"  ChargerEnabled: {power.ChargerEnabled}");
    Console.WriteLine($"  ChargingBatteryVoltage: {power.ChargingBatteryVoltage}");
    Console.WriteLine($"  ChargingCurrent: {power.ChargingCurrent}");
    Console.WriteLine($"  ChargingCutOffCurrent: {power.ChargingCutOffCurrent}");
    Console.WriteLine($"  ChargingCuttOffVoltage{power.ChargingCuttOffVoltage}");
    Console.WriteLine($"  ChargingLoopSelection: {power.ChargingLoopSelection}");
    Console.WriteLine($"  ChargingUnderVoltage: {power.ChargingUnderVoltage}");
    Console.WriteLine($"  ConstantChargingVoltage: {power.ConstantChargingVoltage}");
    Console.WriteLine($"  FlashLightBehavior {power.FlashLightBehavior}");
    Console.WriteLine($"  IsBatteryFull: {power.IsBatteryFull}");
    Console.WriteLine($"  IsCharging: {power.IsCharging}");
    Console.WriteLine($"  IsOutputLoadHigh: {power.IsOutputLoadHigh}");
    Console.WriteLine($"  LightDutyShutdownTime: {power.LightDutyShutdownTime}");
    Console.WriteLine($"  LowPowerOffEnabled: {power.LowPowerOffEnabled}");
    Console.WriteLine($"  ShortPressToSwitchBosst: {power.ShortPressToSwitchBosst}");
    Console.WriteLine($"  SwitchOffBoostBehavior: {power.SwitchOffBoostBehavior}");
    Console.WriteLine($"  GetButtonStatus: {power.GetButtonStatus()}");
}

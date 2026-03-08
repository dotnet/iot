// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device;
using Iot.Device.Adc;
using Iot.Device.Arduino;
using Iot.Device.Board;
using UnitsNet;

// To work with an arduino:
using ArduinoBoard board = new ArduinoBoard("COM5", 115200);
using Ina236 device = new(board.CreateI2cDevice(new I2cConnectionSettings(0, 0x40)), ElectricResistance.FromMilliohms(8),
    ElectricCurrent.FromAmperes(10.0));

// Use this on a Raspberry Pi:
////using Ina236 device2 = new Ina236(I2cDevice.Create(new I2cConnectionSettings(1, 0x40)), ElectricResistance.FromMilliohms(8),
////    ElectricCurrent.FromAmperes(10.0));

Console.WriteLine("Device initialized. Default settings used:");
Console.WriteLine($"Operating Mode: {device.OperatingMode}");
Console.WriteLine($"Number of Samples to average: {device.AverageOverNoSamples}");
Console.WriteLine($"Bus conversion time: {device.BusConversionTime}us");
Console.WriteLine($"Shunt conversion time: {device.ShuntConversionTime}us");

while (!Console.KeyAvailable)
{
    // write out the current values from the INA219 device.
    Console.WriteLine($"Bus Voltage {device.ReadBusVoltage()} Shunt Voltage {device.ReadShuntVoltage().Millivolts}mV Current {device.ReadCurrent()} Power {device.ReadPower()}");
    Thread.Sleep(1000);
}

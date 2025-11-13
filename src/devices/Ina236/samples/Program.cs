// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device;
using Iot.Device.Adc;
using Iot.Device.Arduino;
using UnitsNet;

using ArduinoBoard board = new ArduinoBoard("COM4", 115200);
using Ina236 device = new(board.CreateI2cDevice(new I2cConnectionSettings(0, 0x80)), ElectricResistance.FromMilliohms(8),
    ElectricCurrent.FromAmperes(10.0));

while (!Console.KeyAvailable)
{
    // write out the current values from the INA219 device.
    Console.WriteLine($"Bus Voltage {device.ReadBusVoltage()} Shunt Voltage {device.ReadShuntVoltage().Millivolts}mV Current {device.ReadCurrent()} Power {device.ReadPower()}");
    Thread.Sleep(1000);
}

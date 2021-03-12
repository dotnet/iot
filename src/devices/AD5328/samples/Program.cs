// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;
using System.Threading;
using Iot.Device.DAC;
using UnitsNet;

var spisettings = new SpiConnectionSettings(0, 1)
{
    Mode = SpiMode.Mode2
};

var spidev = SpiDevice.Create(spisettings);
using var dac = new AD5328(spidev, ElectricPotential.FromVolts(2.5), ElectricPotential.FromVolts(2.5));
Thread.Sleep(1000);
dac.SetVoltage(0, ElectricPotential.FromVolts(1));

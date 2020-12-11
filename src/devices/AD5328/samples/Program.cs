// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.AD5328;

var spisettings = new SpiConnectionSettings(0, 1)
{
    Mode = SpiMode.Mode2
};

var spidev = SpiDevice.Create(spisettings)
var dac = new AD5328(spidev, ElectricPotential.FromVolts(2.5), ElectricPotential.FromVolts(2.5));
Thread.Sleep(1000);
dac.SetVoltage(0, ElectricPotential.FromVolts(1));

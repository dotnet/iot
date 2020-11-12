// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.UFire;
using UnitsNet;

#pragma warning disable SA1011

const int BusId = 1;

PrintHelp();

I2cConnectionSettings settings = new(BusId, UFireIse.I2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
Console.WriteLine(
        $"UFire_ISE is ready on I2C bus {device.ConnectionSettings.BusId} with address {device.ConnectionSettings.DeviceAddress}");

Console.WriteLine();

while (true)
{
    string[]? command = Console.ReadLine()?.ToLower()?.Split(' ');
    if (command?[0] is not { Length: >0 })
    {
        return;
    }

    switch (command[0][0])
    {
        case 'b':
            Basic(device);
            return;
        case '0':
            Orp(device);
            return;
        case 'p':
            Ph(device);
            return;
    }
}

void PrintHelp()
{
    Console.WriteLine("Command:");
    Console.WriteLine("    B           Basic");
    Console.WriteLine("    O           Read Orp (Oxidation-reduction potential) value");
    Console.WriteLine("    P           Read pH (Power of Hydrogen) value");
    Console.WriteLine();
}

void Basic(I2cDevice device)
{
    using UFireIse uFireIse = new UFireIse(device);
    Console.WriteLine("mV:" + uFireIse.ReadElectricPotential().Millivolts);
}

void Orp(I2cDevice device)
{
    using UFireOrp uFireOrp = new(device);
    if (uFireOrp.TryMeasureOxidationReductionPotential(out ElectricPotential orp))
    {
        Console.WriteLine("Eh:" + orp.Millivolts);
    }
    else
    {
        Console.WriteLine("Not possible to measure pH");
    }
}

void Ph(I2cDevice device)
{
    using UFirePh uFire_pH = new UFirePh(device);
    Console.WriteLine("mV:" + uFire_pH.ReadElectricPotential().Millivolts);

    if (uFire_pH.TryMeasurepH(out float pH))
    {
        Console.WriteLine("pH:" + pH);
    }
    else
    {
        Console.WriteLine("Not possible to measure pH");
    }
}

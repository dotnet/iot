﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Linq;
using System.Threading;
using Iot.Device.Usb;
using UnitsNet;

var settings = new I2cConnectionSettings(2, StUsb4500.DefaultI2cAddress);
var stUsb = new StUsb4500(I2cDevice.Create(settings));
Console.WriteLine($"{nameof(stUsb.DeviceId)}: 0x{stUsb.DeviceId:x}");
UsbCCableConnection connection = stUsb.CableConnection;
Console.WriteLine($"{nameof(stUsb.CableConnection)}: {connection}{Environment.NewLine}");

PrintLocalPdo();

PrintNvmData();
Console.WriteLine("Update NVM? (y/N)");
if (Console.ReadLine()?.ToLowerInvariant() == "y")
{
    UpdateNvmData();
}

if (connection == UsbCCableConnection.Disconnected)
{
    return;
}

PrintRdo();
PrintSourcePdo(); // ATTENTION: This triggers a new USB PD contract negotiation and can cause a short power-disruption.

Console.WriteLine("Update local PDOs? (y/N)");
if (Console.ReadLine()?.ToLowerInvariant() != "y")
{
    return;
}

UpdateLocalPdo();
stUsb.PerformUsbPdSoftwareReset();
PrintLocalPdo();
PrintRdo();

void PrintLocalPdo()
{
    Console.WriteLine("Local sink PDOs:");
    PowerDeliveryObject[] sinkPdos = stUsb.SinkPowerDeliveryObjects;
    for (int i = 0; i < sinkPdos.Length; i++)
    {
        PowerDeliveryObject pdo = sinkPdos[i];
        Console.WriteLine($"PDO #{i + 1}: {pdo}");
    }

    Console.WriteLine();
}

void PrintNvmData()
{
    Console.WriteLine("NVM data:");
    byte[] nvmData = stUsb.NvmData;
    Console.WriteLine($"0x00: 0x{nvmData[0]:X2} 0x{nvmData[1]:X2} 0x{nvmData[2]:X2} 0x{nvmData[3]:X2} 0x{nvmData[4]:X2} 0x{nvmData[5]:X2} 0x{nvmData[6]:X2} 0x{nvmData[7]:X2}");
    Console.WriteLine($"0x08: 0x{nvmData[8]:X2} 0x{nvmData[9]:X2} 0x{nvmData[10]:X2} 0x{nvmData[11]:X2} 0x{nvmData[12]:X2} 0x{nvmData[13]:X2} 0x{nvmData[14]:X2} 0x{nvmData[15]:X2}");
    Console.WriteLine($"0x16: 0x{nvmData[16]:X2} 0x{nvmData[17]:X2} 0x{nvmData[18]:X2} 0x{nvmData[19]:X2} 0x{nvmData[20]:X2} 0x{nvmData[21]:X2} 0x{nvmData[22]:X2} 0x{nvmData[23]:X2}");
    Console.WriteLine($"0x24: 0x{nvmData[24]:X2} 0x{nvmData[25]:X2} 0x{nvmData[26]:X2} 0x{nvmData[27]:X2} 0x{nvmData[28]:X2} 0x{nvmData[29]:X2} 0x{nvmData[30]:X2} 0x{nvmData[31]:X2}");
    Console.WriteLine($"0x32: 0x{nvmData[32]:X2} 0x{nvmData[33]:X2} 0x{nvmData[34]:X2} 0x{nvmData[35]:X2} 0x{nvmData[36]:X2} 0x{nvmData[37]:X2} 0x{nvmData[38]:X2} 0x{nvmData[39]:X2}{Environment.NewLine}");
}

void UpdateNvmData()
{
    byte[] nvmData =
        {
            0x00, 0x00, 0xB0, 0xAA, 0x00, 0x45, 0x00, 0x00,
            0x10, 0x40, 0x9C, 0x1C, 0xF0, 0x01, 0x00, 0xDF,
            0x02, 0x40, 0x0F, 0x00, 0x32, 0x00, 0xFC, 0xF1,
            0x00, 0x19, 0x57, 0xAF, 0xF6, 0x4F, 0xFF, 0x00,
            0x00, 0x4B, 0x90, 0x21, 0x03, 0x00, 0x48, 0xFB,
        };
    stUsb.NvmData = nvmData;
    PrintNvmData();
}

void PrintSourcePdo()
{
    Power maxPower = Power.Zero;
    Console.WriteLine("Source PDOs:");
    PowerDeliveryObject[] sourcePdos = stUsb.SourcePowerDeliveryObjects;
    for (int i = 0; i < sourcePdos.Length; i++)
    {
        PowerDeliveryObject pdo = sourcePdos[i];
        Console.WriteLine($"PDO #{i + 1}: {pdo}");
        if (pdo.Power > maxPower)
        {
            maxPower = pdo.Power;
        }
    }

    Console.WriteLine($"P(max) = {maxPower:0.##}{Environment.NewLine}");
}

void PrintRdo()
{
    Console.WriteLine("RDO (negotiated power):");
    RequestDataObject rdo = stUsb.RequestDataObject;
    Console.WriteLine($"Requested position: PDO #{rdo.ObjectPosition}");
    Console.WriteLine($"Operating current: {rdo.OperatingCurrent:0.##}");
    Console.WriteLine($"Max. current: {rdo.MaximalCurrent:0.##}");
    Console.WriteLine($"USB communications capable: {rdo.UsbCommunicationsCapable}");
    Console.WriteLine($"Capability mismatch: {rdo.CapabilityMismatch}");
    ElectricPotential voltage = stUsb.RequestedVoltage;
    Console.WriteLine($"Requested voltage: {voltage:0.##}");
    Console.WriteLine($"Available Power: {Power.FromWatts(voltage.Volts * rdo.MaximalCurrent.Amperes):0.##}{Environment.NewLine}");
}

void UpdateLocalPdo()
{
    Console.WriteLine($"Update Local sink PDOs...{Environment.NewLine}");
    PowerDeliveryObject[] sinkPdos = stUsb.SinkPowerDeliveryObjects;
    if (sinkPdos.Length > 1 && sinkPdos[1] is FixedSupplyObject fixedSupplyObject1)
    {
        fixedSupplyObject1.Voltage = ElectricPotential.FromVolts(12);
        fixedSupplyObject1.OperationalCurrent = ElectricCurrent.FromAmperes(2.0);
    }
    else
    {
        List<PowerDeliveryObject> objects = sinkPdos.ToList();
        objects.Add(new FixedSupplyObject(sinkPdos.First().Value)
            {
                Voltage = ElectricPotential.FromVolts(12),
                OperationalCurrent = ElectricCurrent.FromAmperes(2.0)
            });
        sinkPdos = objects.ToArray();
    }

    if (sinkPdos.Length > 2 && sinkPdos[2] is FixedSupplyObject fixedSupplyObject2)
    {
        fixedSupplyObject2.Voltage = ElectricPotential.FromVolts(20);
        fixedSupplyObject2.OperationalCurrent = ElectricCurrent.FromAmperes(1.25);
    }
    else
    {
        List<PowerDeliveryObject> objects = sinkPdos.ToList();
        objects.Add(new FixedSupplyObject(sinkPdos.First().Value)
            {
                Voltage = ElectricPotential.FromVolts(20),
                OperationalCurrent = ElectricCurrent.FromAmperes(1.25)
            });
        sinkPdos = objects.ToArray();
    }

    stUsb.SinkPowerDeliveryObjects = sinkPdos;
}

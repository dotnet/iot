// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Adc;
using force_sensitive_resistor;

Console.WriteLine("Hello Fsr408 capacitor Sample!");
// Use this sample when using ADC for reading          
StartReadingWithADC();

// Use this sample if using capacitor for reading
// StartReadingWithCapacitor();

void StartReadingWithADC()
{
    FsrWithAdcSample fsrWithAdc = new();
    
    while (true)
    {
        int value = fsrWithAdc.Read(0);
        double voltage = fsrWithAdc.CalculateVoltage(value);
        double resistance = fsrWithAdc.CalculateFsrResistance(voltage);
        double force = fsrWithAdc.CalculateForce(resistance);
        Console.WriteLine($"Read value: {value}, milli voltage: {voltage.ToString("f2")}, resistance: {resistance.ToString("f2")}, approximate force in Newtons: {force.ToString("f2")}");
        Thread.Sleep(500);
    }
}

void StartReadingWithCapacitor()
{
    FsrWithCapacitorSample fsrWithCapacitor = new();

    while (true)
    {
        int value = fsrWithCapacitor.ReadCapacitorChargingDuration();

        if (value == 30000)
        {   // 30000 is count limit, if we got this count it means Fsr has its highest resistance, so it is not pressed
            Console.WriteLine("Not pressed");
        }
        else
        {
            Console.WriteLine($"Pressed {value}");
        }
        Thread.Sleep(500);
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Multiplexing;

using Mbi5027 sr = new(Mbi5027PinMapping.Complete);
CancellationTokenSource cancellationSource = new();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cancellationSource.Cancel();
};

Console.WriteLine($"Driver for {nameof(Mbi5027)}");
Console.WriteLine($"Register bit length: {sr.BitLength}");

CheckCircuit(sr);
BinaryCounter(sr, cancellationSource);
CheckCircuit(sr);
sr.ShiftClear();

void BinaryCounter(ShiftRegister sr, CancellationTokenSource cancellationSource)
{
    int endValue = 65_536;
    Console.WriteLine($"Write 0 through {endValue}");
    int delay = 20;

    for (int i = 0; i < endValue; i++)
    {
        for (int j = (sr.BitLength / 8) - 1; j > 0; j--)
        {
            int shift = j * 8;
            int downShiftedValue = i >> shift;
            sr.ShiftByte((byte)downShiftedValue, false);
        }

        sr.ShiftByte((byte)i);
        Thread.Sleep(delay);

        if (IsCanceled(sr, cancellationSource))
        {
            return;
        }
    }
}

void CheckCircuit(Mbi5027 sr)
{
    Console.WriteLine("Checking circuit");
    sr.EnableDetectionMode();

    int index = sr.BitLength - 1;

    foreach (var value in sr.ReadOutputErrorStatus())
    {
        Console.WriteLine($"Bit {index--}: {value}");
    }

    sr.EnableNormalMode();
}

bool IsCanceled(ShiftRegister sr, CancellationTokenSource cancellationSource)
{
    if (cancellationSource.IsCancellationRequested)
    {
        sr.ShiftClear();
        return true;
    }

    return false;
}

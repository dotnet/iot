// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.Multiplexing;
using static System.Console;

int[] pins = new int[] { 4, 17, 27, 22, 5, 6, 13, 19 };
GpioOutputSegment segment = new(pins);
TimeSpan delay = TimeSpan.FromSeconds(5);

WriteLine("Light all LEDs");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, 1);
}

Latch(delay);

WriteLine("Light every other LED");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, i % 2);
}

Latch(delay);

WriteLine("Light every other LED");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, i % 2);
}

Latch(delay);

WriteLine("Display binary 128");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(128);
}

Latch(delay);
segment.Clear();
WriteLine("Done.");

void Latch(TimeSpan delay)
{
    CancellationToken ct = new();
    ct.WaitHandle.WaitOne(delay);
    segment.Display(ct);
}

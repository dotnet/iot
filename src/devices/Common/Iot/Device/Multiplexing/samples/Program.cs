// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.Multiplexing;
using static System.Console;

int[] pins = new int[] { 4, 17, 27, 22, 5, 6, 13, 19 };
CancellationTokenSource cts = new();
CancellationToken token = cts.Token;
GpioOutputSegment gpioSegment = new GpioOutputSegment(pins, cts.Token);
IOutputSegment segment = gpioSegment;
TimeSpan delay = TimeSpan.FromSeconds(5);

Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    segment.Clear();
    gpioSegment.Dispose();
};

WriteLine("Light all LEDs");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, 1);
}

segment.Display(delay);

WriteLine("Light all LEDs");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, 1);
}

segment.Display(delay);

WriteLine("Light every other LED");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, i % 2);
}

segment.Display(delay);

if (token.IsCancellationRequested)
{
    return;
}

WriteLine("Light every other (other) LED");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, (i + 1) % 2);
}

segment.Display(delay);

if (token.IsCancellationRequested)
{
    return;
}

WriteLine("Display binary 128");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(128);
}

segment.Display(delay);
segment.Clear();
WriteLine("Done.");

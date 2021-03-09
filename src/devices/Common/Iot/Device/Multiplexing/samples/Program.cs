// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.Multiplexing;
using static System.Console;

int[] pins = new int[] { 4, 17, 27, 22, 5, 6, 13, 19 };
CancellationTokenSource appSource = new();
CancellationToken appToken = appSource.Token;
CancellationTokenSource displaySource = new CancellationTokenSource();
IOutputSegment segment = new GpioOutputSegment(pins);
TimeSpan delay = TimeSpan.FromSeconds(5);

Console.CancelKeyPress += (s, e) =>
{
    displaySource.Cancel();
    appSource.Cancel();
    segment.Clear();
    segment.Dispose();
};

WriteLine("Light all LEDs");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, 1);
}

if (DisplayShouldCancel())
{
    return;
}

WriteLine("Light every other LED");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, i % 2);
}

segment.Display(delay);

if (DisplayShouldCancel())
{
    return;
}

WriteLine("Light every other (other) LED");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, (i + 1) % 2);
}

if (DisplayShouldCancel())
{
    return;
}

WriteLine("Display binary 128");
segment.Clear();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(128);
}

if (DisplayShouldCancel())
{
    return;
}

segment.Clear();
WriteLine("Done.");

bool DisplayShouldCancel()
{
    displaySource = new CancellationTokenSource(delay);
    segment.Display(displaySource.Token);
    return appSource.IsCancellationRequested;
}

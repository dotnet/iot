// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using Iot.Device.Multiplexing;
using static System.Console;

int[] pins = new int[] { 4, 17, 27, 22, 5, 6, 13, 19 };
using IOutputSegment segment = new GpioOutputSegment(pins);

CancellationTokenSource cts = new();
CancellationToken token = cts.Token;
bool controlCRequested = false;
TimeSpan delay = TimeSpan.FromSeconds(5);

Console.CancelKeyPress += (s, e) =>
{
    controlCRequested = true;
    cts.Cancel();
    segment.TurnOffAll();
    segment.Dispose();
};

WriteLine("Light all LEDs");
segment.TurnOffAll();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, 1);
}

if (DisplayShouldCancel())
{
    return;
}

WriteLine("Light every other LED");
segment.TurnOffAll();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, i % 2);
}

if (DisplayShouldCancel())
{
    return;
}

WriteLine("Light every other (other) LED");
segment.TurnOffAll();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(i, (i + 1) % 2);
}

if (DisplayShouldCancel())
{
    return;
}

WriteLine("Display binary 128");
segment.TurnOffAll();
for (int i = 0; i < pins.Length; i++)
{
    segment.Write(128);
}

if (DisplayShouldCancel())
{
    return;
}

segment.TurnOffAll();
WriteLine("Done.");

bool DisplayShouldCancel()
{
    using CancellationTokenSource displaySource = new(delay);
    cts = displaySource;
    segment.Display(displaySource.Token);
    return controlCRequested;
}

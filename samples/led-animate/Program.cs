// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Multiplexing;

int[] pins = new int[] {4,5,6,12,13,16,17,18,19,20};
CancellationTokenSource cts = new();
CancellationToken token = cts.Token;
using IOutputSegment segment = new GpioOutputSegment(pins);
using AnimateLeds leds = new(segment);
Console.CancelKeyPress += (s, e) => 
{ 
    cts.Cancel();
    segment.Dispose();
};
            
Console.WriteLine($"Animate! {pins.Length} pins are initialized.");

while (!token.IsCancellationRequested)
{
    Console.WriteLine($"Lit: {leds.LitTime}ms; Dim: {leds.DimTime}");
    leds.FrontToBack(token, true);
    leds.BacktoFront(token);
    leds.MidToEnd(token);
    leds.EndToMid(token);
    leds.MidToEnd(token);
    leds.LightAll(token);
    leds.DimAllAtRandom(token);

    if (leds.LitTime < 20)
    {
        leds.ResetTime();
    }
    else
    {
        leds.LitTime = (int)(leds.LitTime * 0.7);
        leds.DimTime = (int)(leds.DimTime * 0.7);
    }
}

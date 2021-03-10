// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Multiplexing;

// This sample is configured to use 8 leds, but can be changed to a use different number.

// To use with directly connected GPIO pins
// int[] pins = new int[] { 4, 17, 27, 22, 5, 6, 13, 19 };
// using IOutputSegment segment = new GpioOutputSegment(pins);

// To use with charlieplexing
// int[] pins = new int[] { 4, 17, 27, 22};
// using IOutputSegment segment = new CharlieplexSegment(pins, 8);

// To use a shift register
using IOutputSegment segment = new ShiftRegister(ShiftRegisterPinMapping.Minimal, 8);


using AnimateLeds leds = new(segment);

CancellationTokenSource cts = new();
CancellationToken token = cts.Token;
Console.CancelKeyPress += (s, e) => 
{ 
    e.Cancel = true;
    cts.Cancel();
    Thread.Sleep(250);
};
            
Console.WriteLine($"Animate! {segment.Length} pins are initialized.");

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

segment.Dispose();

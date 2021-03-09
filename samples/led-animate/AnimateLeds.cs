// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Iot.Device.Multiplexing;

public class AnimateLeds
{
    private readonly IOutputSegment _segment;
    private readonly CancellationToken _token;
    private readonly int[] _leds;
    private readonly int[] _ledsRev;
    public int LitTimeDefault = 200;
    public int DimTimeDefault = 50;
    public int LitTime = 200;
    public int DimTime = 50;

    public AnimateLeds(IOutputSegment segment)
    {
        _segment = segment;
        _token = _segment.CancellationToken;
        _leds = Enumerable.Range(0,_segment.Length).ToArray();
        _ledsRev = _leds.Reverse().ToArray();
    }

    private void CycleLeds(params int[] outputs)
    {
        if (_token.IsCancellationRequested)
        {
            return;
        }

        // light time
        foreach (int output in outputs)
        {
            _segment.Write(output, PinValue.High);
        }
        _segment.Display(LitTime);

        if (_token.IsCancellationRequested)
        {
            return;
        }

        // dim time
        foreach (int output in outputs)
        {
            _segment.Write(output, PinValue.Low);
        }
        _segment.Display(DimTime);
    }

    public void ResetTime()
    {
        LitTime = LitTimeDefault;
        DimTime = DimTimeDefault;
    }

    public void Sequence(IEnumerable<int> outputs)
    {
        Console.WriteLine(nameof(Sequence));
        foreach (int output in outputs)
        {
            CycleLeds(output);
        }
    }

    public void FrontToBack(bool skipLast = false)
    {
        Console.WriteLine(nameof(FrontToBack));
        int iterations = skipLast ? _segment.Length : _segment.Length - 2;
        Sequence(_leds.AsSpan(0,iterations).ToArray());
    }
    public void BacktoFront()
    {
        Console.WriteLine(nameof(BacktoFront));
        Sequence(_ledsRev);
    }

    public void MidToEnd()
    {
        Console.WriteLine(nameof(MidToEnd));
        var half = _leds.Length / 2;

        if (half % 2 == 1)
        {
            CycleLeds(half);
        }

        for (var i = 1; i < half+1; i ++)
        {
            var ledA= half - i;
            var ledB = half - 1 + i;

            CycleLeds(ledA, ledB);
        }
    }

    public void EndToMid()
    {
        Console.WriteLine(nameof(EndToMid));
        var half = _leds.Length / 2;

        for (var i = 0; i < half ; i++)
        {
            var ledA = i;
            var ledB = _segment.Length - 1 - i;

            CycleLeds(ledA, ledB);
        }

        if (half % 2 == 1)
        {
            CycleLeds(half);
        }
    }

    public void LightAll()
    {
        Console.WriteLine(nameof(LightAll));
        for(int i = 0; i < _leds.Length; i++)
        {
            _segment.Write(i, PinValue.High);

            if (_token.IsCancellationRequested)
            {
                return;
            }
        }
        _segment.Display(LitTime);
    }

    public void DimAllAtRandom()
    {
        Console.WriteLine(nameof(DimAllAtRandom));
        var random = new Random();

        var ledList = _leds.ToList();

        while (ledList.Count > 0)
        {
            var led = random.Next(_leds.Length);

            if (ledList.Remove(led))
            {
                _segment.Write(led, PinValue.Low);
                _segment.Display(DimTime);
            }

            if (_token.IsCancellationRequested)
            {
                return;
            }
        }

    }
}

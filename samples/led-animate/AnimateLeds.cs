// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Iot.Device.Multiplexing;

public class AnimateLeds : IDisposable
{
    private readonly int[] _leds;
    private readonly int[] _ledsRev;
    private IOutputSegment _segment;
    public int LitTimeDefault = 200;
    public int DimTimeDefault = 50;
    public int LitTime = 200;
    public int DimTime = 50;

    public AnimateLeds(IOutputSegment segment)
    {
        _segment = segment;
        _leds = Enumerable.Range(0,_segment.Length).ToArray();
        _ledsRev = _leds.Reverse().ToArray();
    }

    private void CycleLeds(CancellationToken token, params int[] outputs)
    {
        if (token.IsCancellationRequested) return;

        // light time
        foreach (int output in outputs)
        {
            _segment.Write(output, PinValue.High);
        }
        
        if (DisplayShouldCancel(token, LitTime)) return;

        // dim time
        foreach (int output in outputs)
        {
            _segment.Write(output, PinValue.Low);
        }

        if (DisplayShouldCancel(token, DimTime)) return;
    }

    public void ResetTime()
    {
        LitTime = LitTimeDefault;
        DimTime = DimTimeDefault;
    }

    public void Sequence(CancellationToken token, IEnumerable<int> outputs)
    {
        Console.WriteLine(nameof(Sequence));
        if (token.IsCancellationRequested) return;
        foreach (int output in outputs)
        {
            CycleLeds(token, output);
        }
    }

    public void FrontToBack(CancellationToken token, bool skipLast = false)
    {
        Console.WriteLine(nameof(FrontToBack));
        if (token.IsCancellationRequested) return;
        int iterations = skipLast ? _segment.Length : _segment.Length - 2;
        Sequence(token, _leds.AsSpan(0,iterations).ToArray());
    }
    public void BacktoFront(CancellationToken token)
    {
        Console.WriteLine(nameof(BacktoFront));
        if (token.IsCancellationRequested) return;
        Sequence(token,_ledsRev);
    }

    public void MidToEnd(CancellationToken token)
    {
        Console.WriteLine(nameof(MidToEnd));
        if (token.IsCancellationRequested) return;
        var half = _leds.Length / 2;

        if (half % 2 == 1)
        {
            CycleLeds(token, half);
        }

        for (var i = 1; i < half+1; i ++)
        {
            var ledA= half - i;
            var ledB = half - 1 + i;

            CycleLeds(token, ledA, ledB);
        }
    }

    public void EndToMid(CancellationToken token)
    {
        Console.WriteLine(nameof(EndToMid));
        if (token.IsCancellationRequested) return;
        var half = _leds.Length / 2;

        for (var i = 0; i < half ; i++)
        {
            var ledA = i;
            var ledB = _segment.Length - 1 - i;

            CycleLeds(token, ledA, ledB);
        }

        if (half % 2 == 1)
        {
            CycleLeds(token, half);
        }
    }

    public void LightAll(CancellationToken token)
    {
        Console.WriteLine(nameof(LightAll));
        if (token.IsCancellationRequested) return;
        for(int i = 0; i < _leds.Length; i++)
        {
            _segment.Write(i, PinValue.High);

            if (token.IsCancellationRequested) return;
        }
        if (DisplayShouldCancel(token, LitTime)) return;
    }

    public void DimAllAtRandom(CancellationToken token)
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
                if (DisplayShouldCancel(token, DimTime)) return;
            }

            if (token.IsCancellationRequested) return;
        }

    }

    public void Dispose()
    {
        _segment?.Dispose();
        _segment = null!;
    }

    bool DisplayShouldCancel(CancellationToken token, int delay)
    {
        using CancellationTokenSource delaySource = CancellationTokenSource.CreateLinkedTokenSource(token);
        delaySource.CancelAfter(delay);
        _segment.Display(delaySource.Token);
        return token.IsCancellationRequested;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Seesaw;

internal class Volume
{
    public static Volume EnableVolume(Seesaw seesawDevice) =>
        new Volume(seesawDevice);

    private Seesaw _seesawDevice;
    private int _lastValue = 0;

    public Volume(Seesaw seesawDevice)
    {
        _seesawDevice = seesawDevice;
        Init();
    }

    public int GetVolumeValue()
    {
        double value = _seesawDevice.AnalogRead(2);
        value = value / 10.24;
        value = Math.Round(value);
        return (int)value;
    }

    private void Init() =>
        _lastValue = GetVolumeValue();

    public (bool update, int value) GetSleepForVolume(int sleep)
    {
        var value = GetVolumeValue();
        if (value > _lastValue - 2 && value < _lastValue + 2)
        {
            return (false, 0);
        }

        _lastValue = value;
        Console.WriteLine($"Volume: {value}");

        var tenth = value / 10;

        if (tenth == 5)
        {
            return (true, sleep);
        }

        double factor = 5 - tenth;
        factor = factor * 2;
        factor = Math.Abs(factor);

        var newValue = 0;

        if (tenth < 5)
        {
            factor = 1 / factor;
        }

        newValue = (int)(sleep / factor);

        if (newValue >= 10 && newValue <= 1000)
        {
            return (true, newValue);
        }

        return (true, sleep);
    }
}

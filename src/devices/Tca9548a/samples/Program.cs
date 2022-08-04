// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Tca9548a;

Console.WriteLine("Hello TCA9548A!");
using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Tca9548A.DefaultI2cAddress));
using Tca9548A tca9548a = new Tca9548A(i2cDevice);

foreach (Channels channel in Tca9548A.DeviceChannels)
{
    tca9548a.SelectChannel(Channels.Channel0);
    if (tca9548a.TryGetSelectedChannel(out Channels selectedChannel))
    {
        Console.WriteLine(selectedChannel);
    }
}

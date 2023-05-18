// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Drawing;
using System.Threading;
using Iot.Device.Lp55231;

var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Lp55231.DefaultI2cAddress));

using var ledDriver = new Lp55231(i2cDevice);

ledDriver.Reset();

Thread.Sleep(100);

ledDriver.Enabled = true;

ledDriver.Misc = MiscFlags.ClockSourceSelection
               | MiscFlags.ExternalClockDetection
               | MiscFlags.ChargeModeGainHighBit
               | MiscFlags.AddressAutoIncrementEnable;

ledDriver[0] = Color.FromArgb(255, 0, 0);
ledDriver[1] = Color.FromArgb(0, 255, 0);
ledDriver[2] = Color.FromArgb(0, 0, 255);

Console.WriteLine("Should be showing red, green, blue");

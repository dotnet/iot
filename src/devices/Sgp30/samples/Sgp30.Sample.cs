// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Sgp30;

Console.WriteLine("Hello Sgp30 Sample!");

I2cDevice sgp30Device = I2cDevice.Create(new I2cConnectionSettings(1, Sgp30.DefaultI2cAddress));
Sgp30 sgp30 = new Sgp30(sgp30Device);

sgp30.GetSerialId();

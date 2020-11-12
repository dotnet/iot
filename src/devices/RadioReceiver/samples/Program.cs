// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.RadioReceiver;
using UnitsNet;

I2cConnectionSettings settings = new(1, Tea5767.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);

using Tea5767 radio = new Tea5767(device, FrequencyRange.Other, Frequency.FromMegahertz(103.3));
Console.ReadKey();

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Device.I2c;
using System.Threading;
using Iot.Device.Vcnl4040;

Thread.Sleep(6000);
I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, 0x60));

Vcnl4040Device vcnl4040 = new Vcnl4040Device(i2cDevice);
vcnl4040.AmbientLightSensor.Test = 42;

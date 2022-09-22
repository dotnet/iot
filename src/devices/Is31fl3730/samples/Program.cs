// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Display;

using Is31fl3730 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Is31fl3730.DefaultI2cAddress)));

matrix.Initialize();
matrix[0, 5, 0] = 1;
matrix[1, 5, 1] = 1;

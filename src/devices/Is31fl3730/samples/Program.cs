// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Display;

using Is31fl3730 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Is31fl3730.DefaultI2cAddress)));

// matrix.Brightness = 127;
matrix.Initialize();
matrix.DisableAllLeds();
Thread.Sleep(100);
matrix.EnableAllLeds();
// Thread.Sleep(100);
// matrix[0, 4, 3] = 1;
// matrix[0, 4, 4] = 1;
// matrix[0, 4, 5] = 1;
// matrix[0, 4, 6] = 1;
// matrix[0, 0, 7] = 1;

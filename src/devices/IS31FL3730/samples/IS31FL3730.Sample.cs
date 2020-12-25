// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using Iot.Device.IS31FL3730;

Console.WriteLine("IS31FL3730 Sample!");

I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(1, IS31FL3730.DefaultI2cAddress));
IS31FL3730 matrixController = new IS31FL3730(device, new DriverConfiguration()
{
  IsShutdown = false,
  IsAudioInputEnabled = false,
  Layout = MatrixLayout.Matrix8by8,
  Mode = MatrixMode.Both
});

matrixController.Reset();

matrixController.SetMatrix(MatrixMode.Both, new byte[] { 0xFF, 0x7F, 0x0F, 0xFF, 0xF7, 0xF0, 0x77, 0xAA });

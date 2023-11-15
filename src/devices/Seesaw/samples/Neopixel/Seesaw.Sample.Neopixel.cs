// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Gpio;
using System.Drawing;
using System.Linq;
using Iot.Device.Seesaw;

const byte AdafruitSeesawNeopixelI2cAddress = 0x60;
const byte AdafruitSeesawNeopixelI2cBus = 0x1;
const byte NeopixelPin = 0xF;
const int NumberOfPixels = 10;
const ushort NeopixelBufferLength = NumberOfPixels * 3;

using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(AdafruitSeesawNeopixelI2cBus, AdafruitSeesawNeopixelI2cAddress));
using Seesaw ssDevice = new(i2cDevice);

ssDevice.SetNeopixelPin(NeopixelPin);
ssDevice.SetNeopixelSpeed(NeopixelSpeed.Speed_800MHz);
ssDevice.SetNeopixelBufferLength(NeopixelBufferLength);

byte[] buffer;
int g = 0, r = 255, b = 0;
int dg = 0, dr = -1, db = +1;

while (!Console.KeyAvailable)
{
    if (dg < 0 && g == 0)
    {
        dg = 0;
        dr = -1;
        db = +1;
    }
    else if (dr < 0 && r == 0)
    {
        dg = +1;
        dr = 0;
        db = -1;
    }
    else if (db < 0 && b == 0)
    {
        dg = -1;
        dr = +1;
        db = 0;
    }

    buffer = Enumerable.Repeat(new byte[] { (byte)g, (byte)r, (byte)b }, NumberOfPixels).SelectMany(x => x).ToArray();
    ssDevice.SetNeopixelBuffer(buffer);
    ssDevice.SetNeopixelShow();

    g += dg;
    r += dr;
    b += db;
}

buffer = Enumerable.Repeat((byte)0, NumberOfPixels * 3).ToArray();
ssDevice.SetNeopixelBuffer(buffer);
ssDevice.SetNeopixelShow();

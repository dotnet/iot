// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;
using System.Threading;
using Iot.Device.Board;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx;
using Ssd13xx.Samples.Simulations;

SkiaSharpAdapter.Register();

var spiSettings = new SpiConnectionSettings(0)
{
    ClockFrequency = 8_000_000
};

var board = new RaspberryPiBoard();
var spiDevice = board.CreateSpiDevice(spiSettings);
var gpioController = board.CreateGpioController();

var display = new Ssd1309(spiDevice, gpioController, csGpioPin: 8, dcGpioPin: 25, rstGpioPin: 27, width: 128, height: 64);

var simulation = new FallingSandSimulation(display, fps: 30, debug: true);
await simulation.StartAsync(iterations: 5000, initialDelayMs: 500);

display.ClearScreen();

gpioController.Dispose();
spiDevice.Dispose();
display.Dispose();
board.Dispose();

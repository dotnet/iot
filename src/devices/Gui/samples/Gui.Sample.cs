// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.Gui;

Console.WriteLine("Attempting to control your mouse");

var screen = new ScreenCapture();
Rectangle size = screen.ScreenSize();
Console.WriteLine($"Screen size is {size.Width}*{size.Height}.");
Console.WriteLine($"Trying with absolute coordinates");
var myMouse = VirtualPointingDevice.CreateAbsolute(size.Width, size.Height);

// Note: The sample is not attempting to click anywhere, because that could result in some undesired effect
for (int i = 0; i < 5; i++)
{
    myMouse.MoveTo(0, 0);
    Thread.Sleep(1000);
    myMouse.MoveTo(50, 100);
    Thread.Sleep(1000);
    myMouse.MoveTo(size.Width, size.Height);
    Thread.Sleep(1000);
}

myMouse.Dispose();

Console.WriteLine($"Trying with relative coordinates");
myMouse = VirtualPointingDevice.CreateRelative();

for (int i = 0; i < 10; i++)
{
    myMouse.MoveBy(10, 10);
    Thread.Sleep(1000);
    myMouse.MoveBy(20, 5);
    Thread.Sleep(1000);
    myMouse.MoveBy(-30, -15);
    Thread.Sleep(1000);
}

myMouse.Dispose();

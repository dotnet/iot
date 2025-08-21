// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio.Drivers;

/// <summary>
/// A GPIO driver for the Raspberry Pi Compute Module 3
/// </summary>
internal class RaspberryPiCm3Driver : RaspberryPi3LinuxDriver
{
    /// <summary>
    /// Raspberry CM3 has 48 GPIO pins.
    /// </summary>
    protected internal override int PinCount => 48;



}

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

    /// <summary>
    /// Converts a board pin number to the driver's logical numbering scheme.
    /// </summary>
    /// <param name="pinNumber">The board pin number to convert.</param>
    /// <returns>The pin number in the driver's logical numbering scheme.</returns>
    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
    {
        // CM3 has no physical numbering scheme
        return pinNumber;
    }
}

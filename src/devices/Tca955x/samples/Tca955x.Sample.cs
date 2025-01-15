// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Tca955x;

var tca9554 = GetTca9554Device();

// Set the first 4 bits to Input the others as Output
tca9554.WriteByte(Register.ConfigurationPort, 0x0F);

byte readInputs = tca9554.ReadByte(Register.InputPort);
Console.WriteLine($"Current input state: {readInputs.ToString("X2")}");

// Set the output to high
tca9554.WriteByte(Register.OutputPort, 0xF0);

// Enable Interrupt on pin 1
GpioController controller = new GpioController(tca9554);
controller.RegisterCallbackForPinValueChangedEvent(0, PinEventTypes.Rising, Interrupt);

void Interrupt(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
{
    Console.WriteLine($"Interrupt on pin: {pinValueChangedEventArgs.PinNumber} with changetype: {pinValueChangedEventArgs.ChangeType}");
}

Tca9554 GetTca9554Device()
{
    I2cConnectionSettings i2cConnectionSettings = new(1, 0x20);
    I2cDevice i2cDevice = I2cDevice.Create(i2cConnectionSettings);
    return new Tca9554(i2cDevice);
}

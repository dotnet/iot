// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Tca955x;

I2cConnectionSettings i2cConnectionSettings = new(1, 0x20);
I2cDevice i2cDevice = I2cDevice.Create(i2cConnectionSettings);

var tca9554 = new Tca9554(i2cDevice);

// Use the TCA955X either directly and Read and Write the Register or use an GPIO Controller.
// Example 1: Use the TXA9554 directly

// Set the first 4 bits to Input the others as Output
tca9554.WriteByte(Register.ConfigurationPort, 0x0F);

// Reads the full Output Register
byte readInputs = tca9554.ReadByte(Register.InputPort);
Console.WriteLine($"Current input state: {readInputs.ToString("X2")}");

// Writes to the full Output Register
// Set the output to high
tca9554.WriteByte(Register.OutputPort, 0xF0);

// Example 2: Use the TCA9555 with the GPIO Controller
// Create an GPIO Controller where the Interrupt Pin connected is.
GpioController controller = new GpioController();

I2cConnectionSettings i2cConnectionSettings_tca9555 = new(1, Tca955x.DefaultI2cAdress);
I2cDevice i2cDevice_tca9555 = I2cDevice.Create(i2cConnectionSettings_tca9555);
var tca9555 = new Tca9555(i2cDevice_tca9555, 4);
// Create an GPIO Controller which represent the TCA9554
GpioController tca9554Controller = new GpioController(tca9554);
tca9554Controller.OpenPin(0, PinMode.Input);
tca9554Controller.OpenPin(1, PinMode.Output);

Console.WriteLine("Write Pin 1 to High");
tca9554Controller.Write(1, PinValue.High);
Thread.Sleep(1000);
Console.WriteLine("Write Pin 1 to Low");
tca9554Controller.Write(1, PinValue.Low);

Console.WriteLine($"Current input state on Pin 0: {tca9554Controller.Read(0)}");
Console.WriteLine("Wait for Pin Event Rising");
Console.ReadKey();

// Enable Interrupt on pin 0
controller.RegisterCallbackForPinValueChangedEvent(0, PinEventTypes.Rising, Interrupt);

void Interrupt(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
{
    Console.WriteLine($"Interrupt on pin: {pinValueChangedEventArgs.PinNumber} with changetype: {pinValueChangedEventArgs.ChangeType}");
}


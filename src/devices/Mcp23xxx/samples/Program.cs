// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Mcp23xxx;

int s_deviceAddress = 0x20;

Console.WriteLine("Hello Mcp23xxx!");

using Mcp23xxx mcp23xxx = GetMcp23xxxDevice(Mcp23xxxDevice.Mcp23017);
using GpioController controllerUsingMcp = new(mcp23xxx);
// Samples are currently written specifically for the 16 bit variant
if (mcp23xxx is Mcp23x1x mcp23x1x)
{
    // Uncomment sample to run
    ReadSwitchesWriteLeds(mcp23x1x);
    // WriteByte(mcp23x1x);
    // WriteUshort(mcp23x1x);
    // WriteBits(mcp23x1x, controllerUsingMcp);
}

// Uncomment sample to run
// ReadBits(controllerUsingMcp);
// Program methods
Mcp23xxx GetMcp23xxxDevice(Mcp23xxxDevice mcp23xxxDevice) => mcp23xxxDevice switch
{
    // I2C
    Mcp23xxxDevice.Mcp23008 => new Mcp23008(NewI2c()),
    Mcp23xxxDevice.Mcp23009 => new Mcp23009(NewI2c()),
    Mcp23xxxDevice.Mcp23017 => new Mcp23017(NewI2c()),
    Mcp23xxxDevice.Mcp23018 => new Mcp23018(NewI2c()),
    // SPI.
    Mcp23xxxDevice.Mcp23S08 => new Mcp23s08(NewSpi(), s_deviceAddress),
    Mcp23xxxDevice.Mcp23S09 => new Mcp23s09(NewSpi()),
    Mcp23xxxDevice.Mcp23S17 => new Mcp23s17(NewSpi(), s_deviceAddress),
    Mcp23xxxDevice.Mcp23S18 => new Mcp23s18(NewSpi()),
    _ => throw new Exception($"Invalid Mcp23xxxDevice: {nameof(mcp23xxxDevice)}"),

};

I2cDevice NewI2c() => I2cDevice.Create(new(1, s_deviceAddress));

SpiDevice NewSpi() => SpiDevice.Create(new(0, 0)
{
    ClockFrequency = 1000000, Mode = SpiMode.Mode0
});

void ReadSwitchesWriteLeds(Mcp23x1x mcp23x1x)
{
    Console.WriteLine("Read Switches & Write LEDs");

    // Input direction for switches.
    mcp23x1x.WriteByte(Register.IODIR, 0b1111_1111, Port.PortA);
    // Output direction for LEDs.
    mcp23x1x.WriteByte(Register.IODIR, 0b0000_0000, Port.PortB);

    while (true)
    {
        // Read switches.
        byte data = mcp23x1x.ReadByte(Register.GPIO, Port.PortA);
        // Write data to LEDs.
        mcp23x1x.WriteByte(Register.GPIO, data, Port.PortB);
        Console.WriteLine(data);
        Thread.Sleep(500);
    }
}

void WriteByte(Mcp23x1x mcp23x1x)
{
    // This assumes the device is in default Sequential Operation mode.
    Console.WriteLine("Write Individual Byte");

    Register register = Register.IODIR;

    void IndividualRead(Mcp23xxx mcp, Register registerToRead)
    {
        byte dataRead = mcp23x1x.ReadByte(registerToRead, Port.PortB);
        Console.WriteLine($"\tIODIRB: 0x{dataRead:X2}");
    }

    Console.WriteLine("Before Write");
    IndividualRead(mcp23x1x, register);

    mcp23x1x.WriteByte(register, 0x12, Port.PortB);

    Console.WriteLine("After Write");
    IndividualRead(mcp23x1x, register);

    mcp23x1x.WriteByte(register, 0xFF, Port.PortB);

    Console.WriteLine("After Writing Again");
    IndividualRead(mcp23x1x, register);
}

void WriteUshort(Mcp23x1x mcp23x1x)
{
    // This assumes the device is in default Sequential Operation mode.
    Console.WriteLine("Write Sequential Bytes");

    void SequentialRead(Mcp23x1x mcp)
    {
        ushort dataRead = mcp.ReadUInt16(Register.IODIR);
        Console.WriteLine($"\tIODIRA: 0x{(byte)dataRead:X2}");
        Console.WriteLine($"\tIODIRB: 0x{(byte)dataRead >> 8:X2}");
    }

    Console.WriteLine("Before Write");
    SequentialRead(mcp23x1x);

    mcp23x1x.WriteUInt16(Register.IODIR, 0x3412);

    Console.WriteLine("After Write");
    SequentialRead(mcp23x1x);

    mcp23x1x.WriteUInt16(Register.IODIR, 0xFFFF);

    Console.WriteLine("After Writing Again");
    SequentialRead(mcp23x1x);
}

// This is now Read(pinNumber)
void ReadBits(GpioController controller)
{
    Console.WriteLine("Read Bits");

    for (int bitNumber = 0; bitNumber < 8; bitNumber++)
    {
        PinValue bit = controller.Read(bitNumber);
        Console.WriteLine($"{bitNumber}: {bit}");
    }
}

// This is now Write(pinNumber)
void WriteBits(Mcp23x1x mcp23x1x, GpioController controller)
{
    Console.WriteLine("Write Bits");

    // Make port output and set all pins.
    // (SetPinMode will also set the direction for each GPIO pin)
    mcp23x1x.WriteByte(Register.IODIR, 0x00, Port.PortB);
    mcp23x1x.WriteByte(Register.GPIO, 0xFF, Port.PortB);

    for (int bitNumber = 9; bitNumber < 16; bitNumber++)
    {
        controller.Write(bitNumber, PinValue.Low);
        Console.WriteLine($"Bit {bitNumber} low");
        Thread.Sleep(500);
        controller.Write(bitNumber, PinValue.High);
        Console.WriteLine($"Bit {bitNumber} high");
        Thread.Sleep(500);
    }
}

/// <summary>
/// Devive types for Mcp23xxx devices.
/// </summary>
internal enum Mcp23xxxDevice
{
    // I2C.
    Mcp23008,
    Mcp23009,
    Mcp23017,
    Mcp23018,

    // SPI.
    Mcp23S08,
    Mcp23S09,
    Mcp23S17,
    Mcp23S18
}

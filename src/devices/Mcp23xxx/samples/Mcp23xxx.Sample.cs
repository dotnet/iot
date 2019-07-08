// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;

namespace Iot.Device.Mcp23xxx.Samples
{
    class Program
    {
        private static readonly int s_deviceAddress = 0x20;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Mcp23xxx!");

            using (Mcp23xxx mcp23xxx = GetMcp23xxxDevice(Mcp23xxxDevice.Mcp23017))
            {

                // Samples are currently written specifically for the 16 bit variant
                if (mcp23xxx is Mcp23x1x mcp23x1x)
                {
                    // Uncomment sample to run.
                    ReadSwitchesWriteLeds(mcp23x1x);
                    //WriteByte(mcp23x1x);
                    //WriteUshort(mcp23x1x);
                    //WriteBits(mcp23x1x);
                }

                //ReadBits(mcp23xxx);
            }
        }

        private enum Mcp23xxxDevice
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

        private static Mcp23xxx GetMcp23xxxDevice(Mcp23xxxDevice mcp23xxxDevice)
        {
            var i2cConnectionSettings = new I2cConnectionSettings(1, s_deviceAddress);
            var i2cDevice = I2cDevice.Create(i2cConnectionSettings);

            // I2C.
            switch (mcp23xxxDevice)
            {
                case Mcp23xxxDevice.Mcp23008:
                    return new Mcp23008(i2cDevice);
                case Mcp23xxxDevice.Mcp23009:
                    return new Mcp23009(i2cDevice);
                case Mcp23xxxDevice.Mcp23017:
                    return new Mcp23017(i2cDevice);
                case Mcp23xxxDevice.Mcp23018:
                    return new Mcp23018(i2cDevice);
            }

            var spiConnectionSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 1000000,
                Mode = SpiMode.Mode0
            };

            var spiDevice = SpiDevice.Create(spiConnectionSettings);

            // SPI.
            switch (mcp23xxxDevice)
            {
                case Mcp23xxxDevice.Mcp23S08:
                    return new Mcp23s08(spiDevice, s_deviceAddress);
                case Mcp23xxxDevice.Mcp23S09:
                    return new Mcp23s09(spiDevice);
                case Mcp23xxxDevice.Mcp23S17:
                    return new Mcp23s17(spiDevice, s_deviceAddress);
                case Mcp23xxxDevice.Mcp23S18:
                    return new Mcp23s18(spiDevice);
            }

            throw new Exception($"Invalid Mcp23xxxDevice: {nameof(mcp23xxxDevice)}");
        }

        private static void ReadSwitchesWriteLeds(Mcp23x1x mcp23x1x)
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

        private static void WriteByte(Mcp23x1x mcp23x1x)
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

        private static void WriteUshort(Mcp23x1x mcp23x1x)
        {
            // This assumes the device is in default Sequential Operation mode.
            Console.WriteLine("Write Sequential Bytes");

            void SequentialRead(Mcp23x1x mcp)
            {
                ushort dataRead = mcp.ReadUInt16(Register.IODIR);
                Console.WriteLine($"\tIODIRA: 0x{(byte)dataRead:X2}");
                Console.WriteLine($"\tIODIRB: 0x{(byte)dataRead>>8:X2}");
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
        private static void ReadBits(Mcp23xxx mcp23xxx)
        {
            Console.WriteLine("Read Bits");

            for (int bitNumber = 0; bitNumber < 8; bitNumber++)
            {
                PinValue bit = mcp23xxx.Read(bitNumber);
                Console.WriteLine($"{bitNumber}: {bit}");
            }
        }

        // This is now Write(pinNumber)
        private static void WriteBits(Mcp23x1x mcp23x1x)
        {
            Console.WriteLine("Write Bits");

            // Make port output and set all pins.
            // (SetPinMode will also set the direction for each GPIO pin)
            mcp23x1x.WriteByte(Register.IODIR, 0x00, Port.PortB);
            mcp23x1x.WriteByte(Register.GPIO, 0xFF, Port.PortB);

            for (int bitNumber = 9; bitNumber < 16; bitNumber++)
            {
                mcp23x1x.Write(bitNumber, PinValue.Low);
                Console.WriteLine($"Bit {bitNumber} low");
                Thread.Sleep(500);
                mcp23x1x.Write(bitNumber, PinValue.High);
                Console.WriteLine($"Bit {bitNumber} high");
                Thread.Sleep(500);
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Threading;

namespace Iot.Device.Mcp23xxx.Samples
{
    class Program
    {
        private static readonly int s_deviceAddress = 0x20;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Mcp23xxx!");

            using (var mcp23xxx = GetMcp23xxxDevice(Mcp23xxxDevice.Mcp23017))
            {
                // Uncomment sample to run.
                if (mcp23xxx.PinCount == 16)
                    ReadSwitchesWriteLeds((Mcp23x1x)mcp23xxx);
                // ReadAllRegisters(mcp23xxx);
                // WriteIndividualByte(mcp23xxx);
                // WriteSequentialBytes(mcp23xxx);
                // ReadBits(mcp23xxx);
                // WriteBits(mcp23xxx);
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
            var i2cDevice = new UnixI2cDevice(i2cConnectionSettings);

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

            var spiDevice = new UnixSpiDevice(spiConnectionSettings);

            // SPI.
            switch (mcp23xxxDevice)
            {
                case Mcp23xxxDevice.Mcp23S08:
                    return new Mcp23S08(s_deviceAddress, spiDevice);
                case Mcp23xxxDevice.Mcp23S09:
                    return new Mcp23S09(s_deviceAddress, spiDevice);
                case Mcp23xxxDevice.Mcp23S17:
                    return new Mcp23S17(s_deviceAddress, spiDevice);
                case Mcp23xxxDevice.Mcp23S18:
                    return new Mcp23S18(s_deviceAddress, spiDevice);
            }

            throw new Exception($"Invalid Mcp23xxxDevice: {nameof(mcp23xxxDevice)}");
        }

        private static void ReadSwitchesWriteLeds(Mcp23x1x mcp23x1x)
        {
            Console.WriteLine("Read Switches & Write LEDs");

            using (mcp23x1x)
            {
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
        }

#if TODO
        // Why would you want to raw read out of the Mcp* instead of the SPI/I2C? It can be done
        // by deriving and exposing the protected members. Assumption is that it is an advance
        // scenario that doesn't need to be in the face of normal users.

        private static void ReadAllRegisters(Mcp23xxx mcp23xxx)
        {
            // This assumes the device is in default Sequential Operation mode.
            Console.WriteLine("Read All Registers");

            using (mcp23xxx)
            {
                // Start at first register.  Total of 22 registers for MCP23x17.
                byte[] data = mcp23xxx.Read(0, 22, Port.PortA, Bank.Bank0);

                for (int index = 0; index < data.Length; index++)
                {
                    Console.WriteLine($"0x{index:X2}: 0x{data[index]:X2}");
                }
            }
        }

        // Just need to update this- supported through read/write byte

        private static void WriteIndividualByte(Mcp23xxx mcp23xxx)
        {
            // This assumes the device is in default Sequential Operation mode.
            Console.WriteLine("Write Individual Byte");

            using (mcp23xxx)
            {
                Register.Address address = Register.Address.IODIR;

                void IndividualRead(Mcp23xxx mcp, Register.Address addressToRead)
                {
                    byte[] dataRead = mcp23xxx.Read(addressToRead, 1, Port.PortB, Bank.Bank0);
                    Console.WriteLine($"\tIODIRB: 0x{dataRead[0]:X2}");
                }

                Console.WriteLine("Before Write");
                IndividualRead(mcp23xxx, address);

                byte[] dataWrite = new byte[] { 0x12 };
                mcp23xxx.Write(address, dataWrite, Port.PortB, Bank.Bank0);

                Console.WriteLine("After Write");
                IndividualRead(mcp23xxx, address);

                dataWrite = new byte[] { 0xFF };
                mcp23xxx.Write(address, dataWrite, Port.PortB, Bank.Bank0);

                Console.WriteLine("After Writing Again");
                IndividualRead(mcp23xxx, address);
            }
        }

        // The key scenario is supported through read/write ushort

        private static void WriteSequentialBytes(Mcp23xxx mcp23xxx)
        {
            // This assumes the device is in default Sequential Operation mode.
            Console.WriteLine("Write Sequential Bytes");

            using (mcp23xxx)
            {
                void SequentialRead(Mcp23xxx mcp)
                {
                    byte[] dataRead = mcp23xxx.Read(0, 2, Port.PortA, Bank.Bank0);
                    Console.WriteLine($"\tIODIRA: 0x{dataRead[0]:X2}");
                    Console.WriteLine($"\tIODIRB: 0x{dataRead[1]:X2}");
                }

                Console.WriteLine("Before Write");
                SequentialRead(mcp23xxx);

                byte[] dataWrite = new byte[] { 0x12, 0x34 };
                mcp23xxx.Write(0, dataWrite, Port.PortA, Bank.Bank0);

                Console.WriteLine("After Write");
                SequentialRead(mcp23xxx);

                dataWrite = new byte[] { 0xFF, 0xFF };
                mcp23xxx.Write(0, dataWrite, Port.PortA, Bank.Bank0);

                Console.WriteLine("After Writing Again");
                SequentialRead(mcp23xxx);
            }
        }

        // This is now Read(pinNumber)
        private static void ReadBits(Mcp23xxx mcp23xxx)
        {
            Console.WriteLine("Read Bits");

            using (mcp23xxx)
            {
                for (int bitNumber = 0; bitNumber < 8; bitNumber++)
                {
                    bool bit = mcp23xxx.ReadBit(Register.Address.GPIO, bitNumber, Port.PortA, Bank.Bank0);
                    Console.WriteLine($"{bitNumber}: {bit}");
                }
            }
        }

        // This is now Write(pinNumber)
        private static void WriteBits(Mcp23xxx mcp23xxx)
        {
            Console.WriteLine("Write Bits");

            // Make port output and set all pins.
            mcp23xxx.Write(Register.Address.IODIR, 0x00, Port.PortB, Bank.Bank0);
            mcp23xxx.Write(Register.Address.GPIO, 0xFF, Port.PortB, Bank.Bank0);

            using (mcp23xxx)
            {
                for (int bitNumber = 0; bitNumber < 8; bitNumber++)
                {
                    mcp23xxx.WriteBit(Register.Address.GPIO, bitNumber, false, Port.PortB, Bank.Bank0);
                    Console.WriteLine($"Bit {bitNumber} low");
                    Thread.Sleep(500);
                    mcp23xxx.WriteBit(Register.Address.GPIO, bitNumber, true, Port.PortB, Bank.Bank0);
                    Console.WriteLine($"Bit {bitNumber} high");
                    Thread.Sleep(500);
                }
            }
        }
#endif
    }
}

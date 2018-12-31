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

            Mcp23xxx mcp23xxx;
            mcp23xxx = GetMcp230xx(); // I2C
            //mcp23xxx = GetMcp23Sxx(); // SPI

            // Uncomment sample to run.
            // ReadSwitchesWriteLeds(mcp23xxx);
            // ReadAllRegisters(mcp23xxx);
            WriteIndividualByte(mcp23xxx);
            // WriteSequentialBytes(mcp23xxx);
        }

        private static Mcp23xxx GetMcp230xx()
        {
            Console.WriteLine("Using I2C protocol");

            var connectionSettings = new I2cConnectionSettings(1, s_deviceAddress);
            var i2cDevice = new UnixI2cDevice(connectionSettings);
            var mcp23Sxx = new Mcp230xx(i2cDevice);
            return mcp23Sxx;
        }

        private static Mcp23xxx GetMcp23Sxx()
        {
            Console.WriteLine("Using SPI protocol");

            var connectionSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 1000000,
                Mode = SpiMode.Mode0
            };

            var spiDevice = new UnixSpiDevice(connectionSettings);
            var mcp23Sxx = new Mcp23Sxx(s_deviceAddress, spiDevice);
            return mcp23Sxx;
        }

        private static void ReadSwitchesWriteLeds(Mcp23xxx mcp23xxx)
        {
            Console.WriteLine("Read Switches & Write LEDs");

            using (mcp23xxx)
            {
                // Input direction for switches.
                mcp23xxx.Write(Register.Address.IODIR, 0b1111_1111, Port.PortA, Bank.Bank0);                
                // Output direction for LEDs.
                mcp23xxx.Write(Register.Address.IODIR, 0b0000_0000, Port.PortB, Bank.Bank0);

                while (true)
                {
                    // Read switches.
                    byte data = mcp23xxx.Read(Register.Address.GPIO, Port.PortA, Bank.Bank0);
                    // Write data to LEDs.
                    mcp23xxx.Write(Register.Address.GPIO, data, Port.PortB, Bank.Bank0);
                    Console.WriteLine(data);
                    Thread.Sleep(500);
                }
            }
        }

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
    }
}

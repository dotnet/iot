// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Threading;

namespace Iot.Device.Mcp23xxx.Samples
{
    class Program
    {
        private static readonly int s_deviceAddress = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Mcp23xxx!");

            Mcp23xxx mcp23xxx = GetMcp23xxxWithSpi();

            // Uncomment sample to run.
            ReadSwitchesWriteLeds(mcp23xxx);
            // ReadAllRegisters(mcp23xxx);
            // WriteSequentialBytes(mcp23xxx);
        }

        private static Mcp23xxx GetMcp23xxxWithSpi()
        {
            Console.WriteLine("Using SPI protocol");

            var connection = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 1000000,
                Mode = SpiMode.Mode0
            };

            var spi = new UnixSpiDevice(connection);
            var mcp23xxx = new Mcp23xxx(spi);
            return mcp23xxx;
        }

        private static void ReadSwitchesWriteLeds(Mcp23xxx mcp23xxx)
        {
            Console.WriteLine("Read Switches & Write LEDs");

            using (mcp23xxx)
            {
                // Input direction for switches.
                mcp23xxx.Write(s_deviceAddress, Register.Address.IODIR, 0b1111_1111, Port.PortA, Bank.Bank0);                
                // Output direction for LEDs.
                mcp23xxx.Write(s_deviceAddress, Register.Address.IODIR, 0b0000_0000, Port.PortB, Bank.Bank0);

                while (true)
                {
                    // Read switches.
                    byte data = mcp23xxx.Read(s_deviceAddress, Register.Address.GPIO, Port.PortA, Bank.Bank0);
                    // Write data to LEDs.
                    mcp23xxx.Write(s_deviceAddress, Register.Address.GPIO, data, Port.PortB, Bank.Bank0);
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
                byte[] data = mcp23xxx.Read(s_deviceAddress, 0, 22, Port.PortA, Bank.Bank0);

                for (int index = 0; index < data.Length; index++)
                {
                    Console.WriteLine($"0x{index:X2}: 0x{data[index]:X2}");
                }
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
                    byte[] dataRead = mcp23xxx.Read(s_deviceAddress, 0, 2, Port.PortA, Bank.Bank0);
                    Console.WriteLine($"\tIODIRA: 0x{dataRead[0]:X2}");
                    Console.WriteLine($"\tIODIRB: 0x{dataRead[1]:X2}");
                }

                Console.WriteLine("Before Write");
                SequentialRead(mcp23xxx);

                byte[] dataWrite = new byte[] { 0x12, 0x34 };
                mcp23xxx.Write(s_deviceAddress, 0, dataWrite, Port.PortA, Bank.Bank0);

                Console.WriteLine("After Write");
                SequentialRead(mcp23xxx);
            }
        }
    }
}

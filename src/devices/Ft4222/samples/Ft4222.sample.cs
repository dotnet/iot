// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Bno055;
using System;
using System.Device.Ft4222;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;

namespace Ft4222.sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello I2C, SPI and GPIO FTFI! FT4222");
            Console.WriteLine("Select the test you want to run:");
            Console.WriteLine(" 1 Run I2C tests with a BNO055");
            Console.WriteLine(" 2 Run SPI tests with a simple HC595 with led blinking on all ports");
            var key = Console.ReadKey();
            Console.WriteLine();

            var devices = FtCommon.GetDevices();
            Console.WriteLine($"{devices.Count} FT4222 elements found");
            foreach (var device in devices)
            {
                Console.WriteLine($"Description: {device.Description}");
                Console.WriteLine($"Flags: {device.Flags}");
                Console.WriteLine($"Handle: {device.FtHandle}");
                Console.WriteLine($"Id: {device.Id}");
                Console.WriteLine($"Location Id: {device.LocId}");
                Console.WriteLine($"Serial Number: {device.SerialNumber}");
                Console.WriteLine($"Device type: {device.Type}");
            }

            var (chip, dll) = FtCommon.GetVersions();
            Console.WriteLine($"Chip version: {chip}");
            Console.WriteLine($"Dll version: {dll}");

            if (key.KeyChar == '1')
                TestI2c();

            if (key.KeyChar == '2')
                TestSpi();

        }

        private static void TestI2c()
        {
            Ft4222I2c ftI2c = new Ft4222I2c(new I2cConnectionSettings(0, Bno055Sensor.DefaultI2cAddress));

            Bno055Sensor bno055Sensor = new Bno055Sensor(ftI2c);

            Console.WriteLine($"Id: {bno055Sensor.Info.ChipId}, AccId: {bno055Sensor.Info.AcceleratorId}, GyroId: {bno055Sensor.Info.GyroscopeId}, MagId: {bno055Sensor.Info.MagnetometerId}");
            Console.WriteLine($"Firmware version: {bno055Sensor.Info.FirmwareVersion}, Bootloader: {bno055Sensor.Info.BootloaderVersion}");
            Console.WriteLine($"Temperature source: {bno055Sensor.TemperatureSource}, Operation mode: {bno055Sensor.OperationMode}, Units: {bno055Sensor.Units}");
            Console.WriteLine($"Powermode: {bno055Sensor.PowerMode}");
        }

        private static void TestSpi()
        {
            Ft4222Spi ftSpi = new Ft4222Spi(new SpiConnectionSettings(0, 1) { ClockFrequency = 1_000_000, Mode = SpiMode.Mode0 });

            while (!Console.KeyAvailable)
            {
                ftSpi.WriteByte(0xFF);
                Thread.Sleep(500);
                ftSpi.WriteByte(0x00);
                Thread.Sleep(500);
            }
        }
    }
}

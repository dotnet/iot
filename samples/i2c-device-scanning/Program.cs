using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Runtime.InteropServices;
using System.Threading;

namespace I2cDeviceScanning
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("I2C device scanning...");

            while (true)
            {
                ScanDevice(1);

                Thread.Sleep(5000);
            }
        }

        public static void ScanDevice(int busId)
        {
            I2cDevice device;
            bool isFound = false;

            for (int address = 1; address < 127; address++)
            {
                I2cConnectionSettings settings = new I2cConnectionSettings(busId, address);
                device = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? (I2cDevice)new UnixI2cDevice(settings) : new Windows10I2cDevice(settings);

                try
                {
                    using (device)
                    {
                        device.WriteByte(0);

                        Console.WriteLine($"I2C device found at: 0x{Convert.ToString(address, 16).ToUpper()}");
                        isFound = true;
                    }
                }
                catch (Exception)
                {

                }
            }

            if (!isFound)
            {
                Console.WriteLine("No I2C device found.");
            }
        }
    }
}
